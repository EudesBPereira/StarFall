using Starfall.Logic;
using UnityEngine;

namespace Starfall.Combat
{
    /// <summary>Everything a projectile needs at launch time. Built by weapons, enemy attacks and bosses.</summary>
    public struct ProjectileSpec
    {
        public float Speed;
        public float Damage;
        public float Lifetime;
        public float Scale;
        public Color Color;
        public Faction Faction;
        public DamageSource Source;
        public DamageType Type;
        public bool Homing;
        public float TurnRateDegrees;
        public Transform HomingTarget;
        /// <summary>Extra targets the projectile may pass through before despawning (0 = none).</summary>
        public int Pierce;

        public static ProjectileSpec Player(float damage, float speed, float lifetime, Color color, float scale = 1f)
        {
            return new ProjectileSpec
            {
                Speed = speed,
                Damage = damage,
                Lifetime = lifetime,
                Scale = scale,
                Color = color,
                Faction = Faction.Player,
                Source = DamageSource.Player,
                Type = DamageType.Laser,
            };
        }

        public static ProjectileSpec Enemy(float damage, float speed, float lifetime, Color color, float scale = 1f, bool boss = false)
        {
            return new ProjectileSpec
            {
                Speed = speed,
                Damage = damage,
                Lifetime = lifetime,
                Scale = scale,
                Color = color,
                Faction = Faction.Enemy,
                Source = boss ? DamageSource.Boss : DamageSource.Enemy,
                Type = DamageType.Normal,
            };
        }
    }
}
