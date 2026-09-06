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
        private readonly List<Enemy> _buffer = new List<Enemy>(64);
        private bool _subscribed;

        public EnergyModel Energy => _energy ??= new EnergyModel(_config != null ? _config.UltimateEnergyMax : 100f);
        public bool IsReady => Energy.IsFull;
        /// <summary>Temporary charge multiplier (Overdrive). 1 = none.</summary>
        public float ExternalChargeMultiplier { get; set; } = 1f;

        public void Initialize(GameConfig config, in PlayerLoadout loadout)
        {
            _config = config;
            _chargeMultiplier = loadout.UltimateChargeMultiplier;
            _powerMultiplier = loadout.UltimatePowerMultiplier;
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
            Energy.Add(info.Definition.EnergyOnKill * _chargeMultiplier * ExternalChargeMultiplier);
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
