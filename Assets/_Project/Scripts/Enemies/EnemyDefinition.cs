using System;
using Starfall.Pooling;
using Starfall.PowerUps;
using UnityEngine;

namespace Starfall.Enemies
{
    public enum MovementKind
    {
        StraightDown = 0,
        Weave = 1,
        Chase = 2,
        HoverStrafe = 3,
        LateralPatrol = 4,
        /// <summary>Leviathan: wide horizontal sweeps with vertical undulation.</summary>
        Serpentine = 5,
        /// <summary>Reaper Wing: quick dashes to random positions with short pauses.</summary>
        Dash = 6,
        /// <summary>Turrets: descend to the hold line and stay there.</summary>
        Hold = 7,
    }

    public enum AttackKind
    {
        None = 0,
        Forward = 1,
        Aimed = 2,
        Ring = 3,
        /// <summary>Continuous stream of shots sweeping left/right.</summary>
        Stream = 4,
        /// <summary>Slow web projectile that slows the player on hit.</summary>
        Web = 5,
    }

    [Serializable]
    public struct MovementParams
    {
        [Min(0f)] public float Speed;
        [Tooltip("Weave/patrol horizontal amplitude in world units.")]
        [Min(0f)] public float Amplitude;
        [Tooltip("Weave/patrol frequency (radians per second).")]
        [Min(0f)] public float Frequency;
        [Tooltip("HoverStrafe/Hold: distance from the top of the screen (fraction of height) where the enemy holds.")]
        [Range(0f, 1f)] public float HoldHeight;
        [Tooltip("HoverStrafe: seconds holding position before leaving (Hold: ignored).")]
        [Min(0f)] public float HoldDuration;
        [Tooltip("HoverStrafe: horizontal speed while holding. Serpentine: vertical amplitude. Dash: pause seconds.")]
        [Min(0f)] public float StrafeSpeed;
        [Tooltip("Chase: turn rate in degrees per second.")]
        [Min(0f)] public float TurnRate;

        public static MovementParams Default => new MovementParams { Speed = 2.5f, Amplitude = 1.5f, Frequency = 2f, HoldHeight = 0.25f, HoldDuration = 6f, StrafeSpeed = 2f, TurnRate = 90f };
    }

    [Serializable]
    public struct AttackParams
    {
        [Min(0.05f)] public float Interval;
        [Min(0f)] public float InitialDelay;
        [Min(1)] public int Count;
        [Min(0f)] public float SpreadAngle;
        [Min(1)] public int BurstCount;
        [Min(0.02f)] public float BurstInterval;
        [Min(0.1f)] public float ProjectileSpeed;
        [Min(0f)] public float Damage;
        [Min(0.05f)] public float ProjectileScale;
        [Min(0.1f)] public float Lifetime;
        public Color Color;
        public bool OnlyWhenOnScreen;
        [Tooltip("Splash radius on impact (0 = none). Bomber bombs use this.")]
        [Min(0f)] public float SplashRadius;
        [Tooltip("Web: seconds the player stays slowed after a hit.")]
        [Min(0f)] public float SlowSeconds;

        public static AttackParams Default => new AttackParams
        {
            Interval = 2f, InitialDelay = 0.8f, Count = 1, SpreadAngle = 0f, BurstCount = 1, BurstInterval = 0.12f,
            ProjectileSpeed = 6f, Damage = 10f, ProjectileScale = 1f, Lifetime = 5f, Color = new Color(1f, 0.4f, 0.3f), OnlyWhenOnScreen = true,
            SplashRadius = 0f, SlowSeconds = 0f,
        };
    }

    /// <summary>
    /// Data-driven enemy archetype. Movement and attack behaviours are selected by enum and parameterized here,
    /// so new enemies are new assets, not new classes. Values in docs/BALANCING.md.
    /// </summary>
    [CreateAssetMenu(menuName = "Starfall/Enemies/Enemy Definition", fileName = "Enemy")]
    public class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string DisplayName = "Enemy";
        [Tooltip("Optional prefab override. When empty the spawner uses its default enemy prefab.")]
        public PooledObject Prefab;
        public Sprite Sprite;
        public Color Tint = Color.white;
        [Min(0.05f)] public float Scale = 1f;
        [Min(0.05f)] public float ColliderRadius = 0.4f;

        [Header("Durability")]
        [Min(1f)] public float MaxHull = 10f;
        [Min(0f)] public float MaxShield = 0f;
        public Color ShieldColor = new Color(0.4f, 0.9f, 1f, 0.6f);

        [Header("Contact")]
        [Min(0f)] public float ContactDamage = 20f;
        [Tooltip("Kamikaze: the enemy dies after hitting the player.")]
        public bool SelfDestructOnContact = false;

        [Header("Rewards")]
        [Min(0)] public int ScoreValue = 100;
        [Min(0f)] public float EnergyOnKill = 6f;
        [Tooltip("Upgrade components granted when destroyed by the player (elites, bosses).")]
        [Min(0)] public int ComponentReward = 0;
        public DropTable DropTable;

        [Header("Classification")]
        [Tooltip("Elites take reduced Ultimate damage instead of dying instantly.")]
        public bool IsElite = false;
        [Tooltip("Obstacles (asteroids) do not block wave completion and give no energy.")]
        public bool IsObstacle = false;
        public bool ImmuneToUltimate = false;

        [Header("Behaviour")]
        public MovementKind Movement = MovementKind.StraightDown;
        public MovementParams MovementSettings = MovementParams.Default;
        public AttackKind Attack = AttackKind.None;
        public AttackParams AttackSettings = AttackParams.Default;
        [Tooltip("Optional projectile prefab override for this enemy's attacks.")]
        public PooledObject ProjectilePrefab;

        [Header("Lifetime")]
        [Tooltip("Seconds before off-screen checks despawn the enemy (lets it enter from above).")]
        [Min(0f)] public float MinLifetime = 1.5f;

        [Header("Risk Zone")]
        [Tooltip("How much this enemy contributes to the player Risk Zone when close (1 = standard, kamikaze > 1).")]
        [Min(0f)] public float RiskWeight = 1f;

        [Header("Feedback")]
        [Min(0.1f)] public float ExplosionScale = 1f;
        public Color ExplosionColor = new Color(1f, 0.6f, 0.25f);
        public bool HasThruster = true;
        public Color ThrusterColor = new Color(1f, 0.5f, 0.2f, 0.9f);

        public virtual bool IsBoss => false;
    }
}
