using System;
using Starfall.Audio;
using Starfall.Logic;
using Starfall.Pooling;
using UnityEngine;

namespace Starfall.Combat
{
    /// <summary>One projectile of a firing pattern: horizontal offset from the muzzle and angle from straight up.</summary>
    [Serializable]
    public struct ShotSpec
    {
        public float OffsetX;
        public float Angle;

        public ShotSpec(float offsetX, float angle)
        {
            OffsetX = offsetX;
            Angle = angle;
        }
    }

    /// <summary>Firing pattern of one weapon level.</summary>
    [Serializable]
    public struct WeaponLevel
    {
        public ShotSpec[] Shots;
        [Tooltip("Damage multiplier of this level (1 = base).")]
        public float DamageMultiplier;
    }

    /// <summary>
    /// Data for a player weapon (GDD §4: Laser, Double Laser, Plasma, Spread Shot, Railgun, Missiles, Energy Cannon).
    /// A single controller reads these fields; every weapon is an asset. Values in docs/BALANCING.md.
    /// </summary>
    [CreateAssetMenu(menuName = "Starfall/Weapons/Weapon Definition", fileName = "Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [Header("Identity")]
        public WeaponId Id = WeaponId.Laser;
        public string DisplayName = "Laser";
        [TextArea] public string Description = "Reliable energy beam.";
        [Tooltip("Credits to unlock in the Hangar (0 = starter).")]
        [Min(0)] public int Cost = 0;

        [Header("Projectile")]
        public PooledObject ProjectilePrefab;
        public Sprite ProjectileSprite;
        [Min(0.01f)] public float FireInterval = 0.14f;
        [Min(0f)] public float Damage = 4f;
        [Min(0.1f)] public float ProjectileSpeed = 18f;
        [Min(0.1f)] public float ProjectileLifetime = 2.5f;
        [Min(0.05f)] public float ProjectileScale = 1f;
        public Color ProjectileColor = new Color(0.35f, 0.9f, 1f, 1f);
        public DamageType DamageType = DamageType.Laser;
        public SfxId FireSfx = SfxId.Laser;

        [Header("Behaviour")]
        [Tooltip("Railgun: extra targets each projectile passes through.")]
        [Min(0)] public int Pierce = 0;
        [Tooltip("Missiles: projectiles steer towards the nearest enemy.")]
        public bool Homing = false;
        [Min(0f)] public float HomingTurnRate = 180f;
        [Tooltip("Energy Cannon: seconds to fully charge a shot (0 = no charge).")]
        [Min(0f)] public float ChargeSeconds = 0f;
        [Tooltip("Energy Cannon: damage multiplier at full charge.")]
        [Min(1f)] public float ChargeDamageMultiplier = 4f;
        [Tooltip("Energy Cannon: projectile scale multiplier at full charge.")]
        [Min(1f)] public float ChargeScaleMultiplier = 2.5f;
        [Tooltip("Missiles: area damage radius on impact (0 = none).")]
        [Min(0f)] public float SplashRadius = 0f;

        [Header("Levels (index 0 = level 1)")]
        public WeaponLevel[] Levels = new WeaponLevel[0];

        public int MaxLevel => Levels != null && Levels.Length > 0 ? Levels.Length : 1;

        public WeaponLevel GetLevel(int level)
        {
            if (Levels == null || Levels.Length == 0)
                return new WeaponLevel { Shots = new[] { new ShotSpec(0f, 0f) }, DamageMultiplier = 1f };
            int i = Mathf.Clamp(level - 1, 0, Levels.Length - 1);
            var l = Levels[i];
            if (l.Shots == null || l.Shots.Length == 0) l.Shots = new[] { new ShotSpec(0f, 0f) };
            if (l.DamageMultiplier <= 0f) l.DamageMultiplier = 1f;
            return l;
        }
    }
}
