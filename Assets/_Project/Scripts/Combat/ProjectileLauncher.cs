using Starfall.Pooling;
using UnityEngine;

namespace Starfall.Combat
{
    /// <summary>Helper used by weapons and attack patterns to fire pooled projectiles.</summary>
    public static class ProjectileLauncher
    {
        public static Projectile Fire(PoolService pools, PooledObject prefab, in ProjectileSpec spec, Vector2 position, Vector2 direction)
        {
            if (pools == null || prefab == null) return null;
            var projectile = pools.Spawn<Projectile>(prefab, position, Quaternion.identity);
            if (projectile == null) return null;
            projectile.Launch(spec, position, direction);
            return projectile;
        }

        /// <summary>Fires <paramref name="count"/> projectiles fanned around <paramref name="direction"/>.</summary>
        public static void FireSpread(PoolService pools, PooledObject prefab, in ProjectileSpec spec, Vector2 position,
            Vector2 direction, int count, float spreadAngleDegrees)
        {
            if (count <= 1)
            {
                Fire(pools, prefab, spec, position, direction);
                return;
            }
            float start = -spreadAngleDegrees * 0.5f;
            float step = spreadAngleDegrees / (count - 1);
            for (int i = 0; i < count; i++)
            {
                Vector2 dir = Rotate(direction, start + step * i);
                Fire(pools, prefab, spec, position, dir);
            }
        }

        /// <summary>Fires <paramref name="count"/> projectiles evenly distributed on a circle.</summary>
        public static void FireRing(PoolService pools, PooledObject prefab, in ProjectileSpec spec, Vector2 position,
            int count, float phaseDegrees)
        {
            if (count <= 0) return;
            float step = 360f / count;
            for (int i = 0; i < count; i++)
            {
                Vector2 dir = Rotate(Vector2.up, phaseDegrees + step * i);
                Fire(pools, prefab, spec, position, dir);
            }
        }

        public static Vector2 Rotate(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
    }
}
