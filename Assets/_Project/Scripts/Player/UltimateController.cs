using System.Collections.Generic;
using Starfall.Audio;
using Starfall.Combat;
using Starfall.Core;
using Starfall.Enemies;
using Starfall.Logic;
using UnityEngine;

namespace Starfall.Player
{
    /// <summary>
    /// Ultimate energy + activation. Energy is earned by player kills; activation wipes common enemies and
    /// damages elites/bosses by a configurable amount (scaled by ship and upgrades).
    /// </summary>
    public sealed class UltimateController : MonoBehaviour
    {
        private EnergyModel _energy;
        private GameConfig _config;
        private float _chargeMultiplier = 1f;
        private float _powerMultiplier = 1f;
        private PlayerLoadout _loadout;
        private readonly List<Enemy> _buffer = new List<Enemy>(64);
        private bool _subscribed;

        public EnergyModel Energy => _energy ??= new EnergyModel(_config != null ? _config.UltimateEnergyMax : 100f);
        public bool IsReady => Energy.IsFull;
        /// <summary>Temporary charge multiplier (Overdrive). 1 = none.</summary>
        public float ExternalChargeMultiplier { get; set; } = 1f;
        /// <summary>Charge multiplier from the temporary build (Energy Cells). 1 = none.</summary>
        public float BuildChargeMultiplier { get; set; } = 1f;

        public void Initialize(GameConfig config, in PlayerLoadout loadout)
        {
            _config = config;
            _chargeMultiplier = loadout.UltimateChargeMultiplier;
            _powerMultiplier = loadout.UltimatePowerMultiplier;
            _loadout = loadout;
            _energy = new EnergyModel(config != null ? config.UltimateEnergyMax : 100f);
            Subscribe();
        }

        private void OnEnable() => Subscribe();

        private void OnDisable()
        {
            if (!_subscribed) return;
            GameSignals.EnemyDestroyed -= OnEnemyDestroyed;
            _subscribed = false;
        }

        private void Subscribe()
        {
            if (_subscribed) return;
            GameSignals.EnemyDestroyed += OnEnemyDestroyed;
            _subscribed = true;
        }

        private void OnEnemyDestroyed(EnemyKilledInfo info)
        {
            // Only weapon kills charge the bar (Ultimate kills would otherwise refund themselves).
            if (info.Source != DamageSource.Player || info.Definition == null) return;
            Energy.Add(info.Definition.EnergyOnKill * _chargeMultiplier * ExternalChargeMultiplier * BuildChargeMultiplier);
        }

        public void AddEnergy(float amount) => Energy.Add(amount);

        public void ResetEnergy() => Energy.Reset();

        /// <summary>Activates the Ultimate when the bar is full. Returns false otherwise.</summary>
        public bool TryActivate()
        {
            if (!Energy.TryConsumeAll())
            {
                AudioManager.PlaySfx(SfxId.UiError);
                return false;
            }
            Execute();
            return true;
        }

        private void Execute()
        {
            var ctx = GameplayContext.Current;
            GameSignals.RaiseUltimateActivated();
            AudioManager.PlaySfx(SfxId.Ultimate);
            if (ctx == null) return;

            var kind = _loadout.Ship != null ? _loadout.Ship.Ultimate : UltimateKind.OrbitalStrike;
            switch (kind)
            {
                case UltimateKind.Swarm: ExecuteSwarm(ctx); return;
                case UltimateKind.Emp: ExecuteEmp(ctx); return;
                default: ExecuteOrbitalStrike(ctx); return;
            }
        }

        /// <summary>Biomech: a swarm of seeking spores hunts enemies for a few seconds (no instant wipe).</summary>
        private void ExecuteSwarm(GameplayContext ctx)
        {
            var green = new Color(0.36f, 1f, 0.62f);
            if (ctx.Vfx != null)
            {
                ctx.Vfx.FlashScreen(new Color(0.4f, 1f, 0.6f, 0.6f), 0.4f);
                ctx.Vfx.SpawnShockwave(transform.position, 6f, green);
            }
            if (ctx.CameraShake != null) ctx.CameraShake.Shake(0.25f, 0.4f);
            var prefab = _loadout.Weapon != null ? _loadout.Weapon.ProjectilePrefab : null;
            if (prefab == null) return;
            int count = FactionRules.SwarmSeekers;
            for (int i = 0; i < count; i++)
            {
                var spec = new ProjectileSpec
                {
                    Speed = 9f,
                    Damage = FactionRules.SwarmSeekerDamage * _powerMultiplier,
                    Lifetime = FactionRules.SwarmSeekerLifetime,
                    Scale = 0.9f,
                    Color = green,
                    Faction = Faction.Player,
                    Source = DamageSource.Player,
                    Type = DamageType.Explosive,
                    Homing = true,
                    TurnRateDegrees = 260f,
                    SplashRadius = 0.7f,
                    Sprite = _loadout.Weapon != null ? _loadout.Weapon.ProjectileSprite : null,
                };
                Vector2 dir = ProjectileLauncher.Rotate(Vector2.up, -80f + 160f * i / Mathf.Max(1, count - 1));
                ProjectileLauncher.Fire(ctx.Pools, prefab, spec, transform.position, dir);
            }
        }

        /// <summary>Cyber: stuns everything, clears bullets, marks every target and deals moderate damage.</summary>
        private void ExecuteEmp(GameplayContext ctx)
        {
            var cyan = new Color(0.14f, 0.84f, 1f);
            if (ctx.Vfx != null)
            {
                ctx.Vfx.FlashScreen(new Color(0.5f, 0.9f, 1f, 0.8f), 0.5f);
                ctx.Vfx.SpawnShockwave(transform.position, ctx.PlayArea != null ? ctx.PlayArea.Height : 16f, cyan);
            }
            if (ctx.CameraShake != null) ctx.CameraShake.Shake(0.35f, 0.5f);
            float damage = FactionRules.EmpDamage * _powerMultiplier;
            ctx.Enemies.CopyTo(_buffer);
            for (int i = 0; i < _buffer.Count; i++)
            {
                var enemy = _buffer[i];
                if (enemy == null || !enemy.IsActiveInstance || !enemy.Health.IsAlive) continue;
                if (enemy.Definition != null && enemy.Definition.ImmuneToUltimate) continue;
                enemy.Stun(FactionRules.EmpStunSeconds);
                enemy.ApplyMark(FactionRules.EmpMarkSeconds);
                enemy.Health.ApplyDamage(new DamageInfo(damage, DamageSource.Ultimate, DamageType.Energy), enemy.transform.position);
            }
            _buffer.Clear();
            Projectile.ClearEnemyProjectiles();
        }

        /// <summary>Federation: orbital strike wipes common enemies and damages elites/bosses.</summary>
        private void ExecuteOrbitalStrike(GameplayContext ctx)
        {
            if (ctx.Vfx != null)
            {
                ctx.Vfx.FlashScreen(new Color(0.7f, 0.9f, 1f, 0.9f), 0.6f);
                ctx.Vfx.SpawnShockwave(transform.position, ctx.PlayArea != null ? ctx.PlayArea.Height : 16f, new Color(0.6f, 0.9f, 1f));
            }
            if (ctx.CameraShake != null) ctx.CameraShake.Shake(0.45f, 0.6f);

            float eliteDamage = (_config != null ? _config.UltimateEliteDamage : 150f) * _powerMultiplier;
            ctx.Enemies.CopyTo(_buffer);
            for (int i = 0; i < _buffer.Count; i++)
            {
                var enemy = _buffer[i];
                if (enemy == null || !enemy.IsActiveInstance || !enemy.Health.IsAlive) continue;
                if (enemy.Definition != null && enemy.Definition.ImmuneToUltimate) continue;

                if (enemy.IsCommon)
                    enemy.Health.Kill(DamageSource.Ultimate);
                else
                    enemy.Health.ApplyDamage(new DamageInfo(eliteDamage, DamageSource.Ultimate, DamageType.Explosive), enemy.transform.position);
            }
            _buffer.Clear();

            if (_config == null || _config.UltimateClearsEnemyProjectiles)
                Projectile.ClearEnemyProjectiles();
        }
    }
}
