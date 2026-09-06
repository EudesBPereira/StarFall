// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;

namespace Starfall.Logic
{
    /// <summary>Graze rules (STAR RISK plan §5.3): a projectile passing close to the hitbox without hitting.</summary>
    public static class GrazeRules
    {
        /// <summary>Extra radius around the hitbox that counts as a graze (world units).</summary>
        public const float DefaultMargin = 0.42f;
        /// <summary>Points per graze (added, not multiplied by combo; multiplied by risk state).</summary>
        public const int BasePoints = 50;
        /// <summary>Projectiles slower than this never graze (they would hover next to the ship).</summary>
        public const float MinProjectileSpeed = 2.5f;

        public static float GrazeRadius(float hitboxRadius, float margin = DefaultMargin) => Math.Max(0f, hitboxRadius) + Math.Max(0f, margin);

        /// <summary>True when the projectile is inside the graze ring but outside the hitbox.</summary>
        public static bool IsInGrazeRing(float distance, float hitboxRadius, float projectileRadius, float margin = DefaultMargin)
        {
            float inner = hitboxRadius + projectileRadius;
            float outer = GrazeRadius(hitboxRadius, margin) + projectileRadius;
            return distance > inner && distance <= outer;
        }

        /// <summary>
        /// Whether a graze can be awarded. Invulnerable (respawn / power-up) ships never earn grazes, so
        /// invulnerability cannot be farmed, and slow projectiles are excluded.
        /// </summary>
        public static bool CanAward(bool playerInvulnerable, bool alreadyGrazed, float projectileSpeed)
        {
            return !playerInvulnerable && !alreadyGrazed && projectileSpeed >= MinProjectileSpeed;
        }

        public static int Points(float riskMultiplier) => (int)Math.Round(BasePoints * Math.Max(1f, riskMultiplier));
    }
}
