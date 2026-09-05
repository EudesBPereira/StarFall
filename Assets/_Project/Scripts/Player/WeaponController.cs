using Starfall.Audio;
using Starfall.Combat;
using Starfall.Core;
using Starfall.Enemies;
using Starfall.Logic;
using Starfall.Pooling;
using Starfall.VFX;
using UnityEngine;

namespace Starfall.Player
{
    /// <summary>
    /// Fires the equipped weapon. Handles level patterns, pierce (Railgun), homing (Missiles), charge
    /// (Energy Cannon) and critical hits. All numbers come from <see cref="WeaponDefinition"/> + <see cref="PlayerLoadout"/>.
    /// </summary>
    public sealed class WeaponController : MonoBehaviour
    {
        [SerializeField] internal Transform muzzle;
        [SerializeField] internal SpriteRenderer chargeGlow;

        private WeaponDefinition _definition;
        private PoolService _pools;
        private VfxSpawner _vfx;
        private EnemyRegistry _enemies;
        private PlayerLoadout _loadout;
        private float _cooldown;
        private int _level = 1;
        private float _charge;
        private bool _charging;

        public WeaponDefinition Definition => _definition;
        public int Level => _level;
        public int MaxLevel => _definition != null ? _definition.MaxLevel : 1;
        /// <summary>Charge progress in [0,1] for the HUD (Energy Cannon only).</summary>
        public float ChargeFraction => _definition != null && _definition.ChargeSeconds > 0f ? Mathf.Clamp01(_charge / _definition.ChargeSeconds) : 0f;

        public void Configure(WeaponDefinition definition, in PlayerLoadout loadout, PoolService pools, VfxSpawner vfx, EnemyRegistry enemies)
        {
            _definition = definition;
            _loadout = loadout;
            _pools = pools;
            _vfx = vfx;
            _enemies = enemies;
            _charge = 0f;
            _charging = false;
            if (chargeGlow != null) chargeGlow.enabled = false;
            SetLevel(1);
        }

        public void SetLevel(int level)
        {
            _level = Mathf.Clamp(level, 1, MaxLevel);
            GameSignals.RaiseWeaponLevelChanged(_level);
        }

        /// <summary>Raises the level by one. Returns false when already at max.</summary>
        public bool Upgrade()
        {
            if (_level >= MaxLevel) return false;
            SetLevel(_level + 1);
            return true;
        }

        public void ResetCooldown()
        {
            _cooldown = 0f;
            _charge = 0f;
            _charging = false;
            if (chargeGlow != null) chargeGlow.enabled = false;
        }

        public void Tick(bool fireHeld, float damageMultiplier)
        {
            if (_definition == null) return;
            float dt = Time.deltaTime;
            _cooldown -= dt;

            if (_definition.ChargeSeconds > 0f)
            {
                TickCharge(fireHeld, damageMultiplier, dt);
                return;
            }

            if (!fireHeld || _cooldown > 0f) return;
            _cooldown = _definition.FireInterval / Mathf.Max(0.1f, _loadout.FireRateMultiplier);
            Fire(damageMultiplier, 1f, 1f);
        }

        private void TickCharge(bool fireHeld, float damageMultiplier, float dt)
        {
            float chargeTime = _definition.ChargeSeconds / Mathf.Max(0.1f, _loadout.FireRateMultiplier);
            if (fireHeld && _cooldown <= 0f)
            {
                if (!_charging) AudioManager.PlaySfx(SfxId.Charge, 0.5f);
                _charging = true;
                _charge = Mathf.Min(chargeTime, _charge + dt);
                if (chargeGlow != null)
                {
                    chargeGlow.enabled = true;
                    float k = _charge / chargeTime;
                    chargeGlow.transform.localScale = Vector3.one * Mathf.Lerp(0.2f, 0.9f, k);
                    var c = _definition.ProjectileColor;
                    c.a = Mathf.Lerp(0.3f, 0.9f, k);
                    chargeGlow.color = c;
                }
                // Auto-fire when fully charged so touch players do not need to release.
                if (_charge >= chargeTime) ReleaseCharge(damageMultiplier, chargeTime);
                return;
            }

            if (_charging)
                ReleaseCharge(damageMultiplier, chargeTime);
        }

        private void ReleaseCharge(float damageMultiplier, float chargeTime)
        {
            float k = Mathf.Clamp01(_charge / chargeTime);
            _charging = false;
            _charge = 0f;
            if (chargeGlow != null) chargeGlow.enabled = false;
            if (k < 0.2f) return; // tap: nothing happens
            float dmgMult = Mathf.Lerp(1f, _definition.ChargeDamageMultiplier, k);
            float scaleMult = Mathf.Lerp(1f, _definition.ChargeScaleMultiplier, k);
            _cooldown = _definition.FireInterval / Mathf.Max(0.1f, _loadout.FireRateMultiplier);
            Fire(damageMultiplier, dmgMult, scaleMult);
        }

        private void Fire(float damageMultiplier, float extraDamageMultiplier, float scaleMultiplier)
        {
            var def = _definition;
            var levelData = def.GetLevel(_level);
            Vector2 origin = muzzle != null ? (Vector2)muzzle.position : (Vector2)transform.position;
            float baseDamage = def.Damage * levelData.DamageMultiplier * damageMultiplier * _loadout.DamageMultiplier * extraDamageMultiplier;
            float lifetime = def.ProjectileLifetime * _loadout.RangeMultiplier;

            Transform homingTarget = def.Homing ? FindNearestEnemy(origin) : null;

            var shots = levelData.Shots;
            for (int i = 0; i < shots.Length; i++)
            {
                float damage = DamageInfo.ApplyCritical(baseDamage, _loadout.CritChance, _loadout.CritMultiplier, Random.value, out bool crit);
                var spec = new ProjectileSpec
                {
                    Speed = def.ProjectileSpeed,
                    Damage = damage,
                    Lifetime = lifetime,
                    Scale = def.ProjectileScale * scaleMultiplier * (crit ? 1.35f : 1f),
                    Color = crit ? Color.Lerp(def.ProjectileColor, Color.white, 0.5f) : def.ProjectileColor,
                    Faction = Faction.Player,
                    Source = DamageSource.Player,
                    Type = def.DamageType,
                    Pierce = def.Pierce,
                    Homing = def.Homing,
                    TurnRateDegrees = def.HomingTurnRate,
                    HomingTarget = homingTarget,
                    Critical = crit,
                    Sprite = def.ProjectileSprite,
                    SplashRadius = def.SplashRadius,
                };
                Vector2 dir = ProjectileLauncher.Rotate(Vector2.up, shots[i].Angle);
                Vector2 pos = origin + new Vector2(shots[i].OffsetX, 0f);
                ProjectileLauncher.Fire(_pools, def.ProjectilePrefab, spec, pos, dir);
            }

            if (_vfx != null) _vfx.SpawnMuzzleFlash(origin, def.ProjectileColor);
            AudioManager.PlaySfx(def.FireSfx, scaleMultiplier > 1.5f ? 1f : 0.8f);
        }

        private Transform FindNearestEnemy(Vector2 from)
        {
            if (_enemies == null) return null;
            var enemy = _enemies.Nearest(from, true);
            return enemy != null ? enemy.transform : null;
        }
    }
}
