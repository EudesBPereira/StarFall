using Starfall.Pooling;
using UnityEngine;

namespace Starfall.Combat
{
    /// <summary>Data for the player's main weapon. Values documented in docs/BALANCING.md.</summary>
    [CreateAssetMenu(menuName = "Starfall/Weapons/Weapon Definition", fileName = "Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        public string DisplayName = "Laser";
        public PooledObject ProjectilePrefab;
        [Min(0.01f)] public float FireInterval = 0.14f;
        [Min(0f)] public float Damage = 4f;
        [Min(0.1f)] public float ProjectileSpeed = 18f;
        [Min(0.1f)] public float ProjectileLifetime = 2.5f;
        [Min(0.05f)] public float ProjectileScale = 1f;
        public Color ProjectileColor = new Color(0.35f, 0.9f, 1f, 1f);
        [Tooltip("Horizontal offset between parallel shots at higher levels.")]
        [Min(0f)] public float ParallelSpacing = 0.22f;
        [Tooltip("Angle of the angled shots at higher levels.")]
        [Min(0f)] public float SideShotAngle = 9f;
        [Range(1, 5)] public int MaxLevel = 5;
        [Tooltip("Damage multiplier applied per weapon level above 1 (additive).")]
        [Min(0f)] public float DamagePerLevel = 0.15f;
    }
}
