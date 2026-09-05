using Starfall.Audio;
using Starfall.Combat;
using Starfall.Core;
using Starfall.Pooling;
using Starfall.VFX;
using UnityEngine;

namespace Starfall.Player
{
    /// <summary>Fires the player's main weapon. Weapon level changes the shot pattern (see docs/BALANCING.md).</summary>
    public sealed class WeaponController : MonoBehaviour
    {
        [SerializeField] internal Transform muzzle;

        private WeaponDefinition _definition;
        private PoolService _pools;
        private VfxSpawner _vfx;
        private float _cooldown;
        private int _level = 1;

        public WeaponDefinition Definition => _definition;
        public int Level => _level;
        public int MaxLevel => _definition != null ? _definition.MaxLevel : 1;

        public void Configure(WeaponDefinition definition, PoolService pools, VfxSpawner vfx)
        {
            _definition = definition;
            _pools = pools;
            _vfx = vfx;
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

        public void ResetCooldown() => _cooldown = 0f;

        public void Tick(bool fireHeld, float damageMultiplier)
        {
            _cooldown -= Time.deltaTime;
            if (!fireHeld || _cooldown > 0f || _definition == null) return;
            _cooldown = _definition.FireInterval;
            Fire(damageMultiplier);
        }

        private void Fire(float damageMultiplier)
        {
            var def = _definition;
            Vector2 origin = muzzle != null ? (Vector2)muzzle.position : (Vector2)transform.position;
            float damage = def.Damage * (1f + def.DamagePerLevel * (_level - 1)) * damageMultiplier;
            var spec = ProjectileSpec.Player(damage, def.ProjectileSpeed, def.ProjectileLifetime, def.ProjectileColor, def.ProjectileScale);

            Vector2 up = Vector2.up;
            float s = def.ParallelSpacing;
            Vector2 left = ProjectileLauncher.Rotate(up, def.SideShotAngle);
            Vector2 right = ProjectileLauncher.Rotate(up, -def.SideShotAngle);

            switch (_level)
            {
                case 1:
                    Shoot(spec, origin, up);
                    break;
                case 2:
                    Shoot(spec, origin + new Vector2(-s * 0.5f, 0f), up);
                    Shoot(spec, origin + new Vector2(s * 0.5f, 0f), up);
                    break;
                case 3:
                    Shoot(spec, origin, up);
                    Shoot(spec, origin + new Vector2(-s, -0.1f), left);
                    Shoot(spec, origin + new Vector2(s, -0.1f), right);
                    break;
                case 4:
                    Shoot(spec, origin + new Vector2(-s * 0.5f, 0f), up);
                    Shoot(spec, origin + new Vector2(s * 0.5f, 0f), up);
                    Shoot(spec, origin + new Vector2(-s * 1.5f, -0.1f), left);
                    Shoot(spec, origin + new Vector2(s * 1.5f, -0.1f), right);
                    break;
                default:
                    Shoot(spec, origin, up);
                    Shoot(spec, origin + new Vector2(-s, 0f), up);
                    Shoot(spec, origin + new Vector2(s, 0f), up);
                    Shoot(spec, origin + new Vector2(-s * 2f, -0.1f), left);
                    Shoot(spec, origin + new Vector2(s * 2f, -0.1f), right);
                    break;
            }

            if (_vfx != null) _vfx.SpawnMuzzleFlash(origin, def.ProjectileColor);
            AudioManager.PlaySfx(SfxId.Laser);
        }

        private void Shoot(in ProjectileSpec spec, Vector2 position, Vector2 direction)
        {
            ProjectileLauncher.Fire(_pools, _definition.ProjectilePrefab, spec, position, direction);
        }
    }
}
