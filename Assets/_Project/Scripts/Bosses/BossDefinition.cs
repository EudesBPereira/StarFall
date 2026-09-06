using System;
using Starfall.Enemies;
using UnityEngine;

namespace Starfall.Bosses
{
    /// <summary>Main bosses of the campaign (GDD §11). Index = bit in SaveData.bossesDefeatedMask.</summary>
    public enum BossId
    {
        Destroyer = 0,
        Leviathan = 1,
        HiveQueen = 2,
        OmegaCore = 3,
        IronWarden = 4,
        Bastion = 5,
        CorsairQueen = 6,
        Assembler = 7,
        AetherGuardian = 8,
        RiftWalker = 9,
        /// <summary>Mini-bosses and variants share this id and never count for Boss Hunter.</summary>
        MiniBoss = 10,
    }

    public enum BossAttackKind
    {
        Ring = 0,
        Aimed = 1,
        Forward = 2,
        SideCannons = 3,
        Missiles = 4,
        FrontLaser = 5,
        /// <summary>Continuous sweeping stream (Leviathan).</summary>
        Stream = 6,
        /// <summary>Spawns minions (Hive Queen).</summary>
        Summon = 7,
        /// <summary>Slow energy web that slows the player (Widow).</summary>
        Web = 8,
        /// <summary>Drops slow mines that detonate with splash when they expire (Corsair Queen).</summary>
        Mines = 9,
        /// <summary>Opens rifts at random on-screen points that spawn minions after a telegraph (Rift Walker).</summary>
        RiftSpawn = 10,
    }

    /// <summary>Destructible boss component (plan §7.7: partes destrutíveis, pontos fracos).</summary>
    [Serializable]
    public struct BossPartSpec
    {
        public string Name;
        public Vector2 Offset;
        [Min(0.1f)] public float Radius;
        [Min(1f)] public float Hull;
        [Min(0)] public int Score;
        [Tooltip("Index of the attack (in every phase) that stops when this part dies; -1 = none.")]
        public int DisablesAttack;
        [Tooltip("While any shielding part is alive the core cannot be damaged (weak-point mechanic).")]
        public bool ShieldsCore;
        public Sprite Sprite;
        public Color Tint;
        [Min(0.1f)] public float Scale;
    }

    [Serializable]
    public struct BossAttack
    {
        public BossAttackKind Kind;
        public AttackParams Settings;
        [Tooltip("Missiles: homing turn rate in degrees per second.")]
        [Min(0f)] public float HomingTurnRate;
        [Tooltip("FrontLaser: seconds of visible warning before the beam deals damage.")]
        [Min(0f)] public float TelegraphSeconds;
        [Tooltip("FrontLaser: seconds the beam stays active.")]
        [Min(0f)] public float BeamSeconds;
        [Tooltip("FrontLaser: beam width in world units.")]
        [Min(0.1f)] public float BeamWidth;
        [Tooltip("FrontLaser: damage per second while touching the beam.")]
        [Min(0f)] public float BeamDamagePerSecond;
        [Tooltip("SideCannons/Missiles: horizontal offset of the muzzles.")]
        [Min(0f)] public float MuzzleOffset;
        [Tooltip("Summon: enemy to spawn (Count = how many).")]
        public EnemyDefinition SummonEnemy;
        [Tooltip("Summon: maximum minions alive before the boss stops summoning.")]
        [Min(0)] public int SummonCap;
    }

    [Serializable]
    public struct BossPhase
    {
        public string Name;
        [Tooltip("Phase becomes active when hull fraction is at or below this value (first phase should be 1).")]
        [Range(0f, 1f)] public float HealthThreshold;
        public MovementKind Movement;
        public MovementParams MovementSettings;
        public BossAttack[] Attacks;
        [Header("Transformation (optional)")]
        public Sprite Sprite;
        public Color Tint;
        [Tooltip("0 keeps the current scale.")]
        [Min(0f)] public float Scale;
        [Tooltip("Seconds of invulnerability while transforming.")]
        [Min(0f)] public float TransformSeconds;
    }

    /// <summary>Boss archetype: an enemy with a scripted entrance and health-driven phases.</summary>
    [CreateAssetMenu(menuName = "Starfall/Bosses/Boss Definition", fileName = "Boss")]
    public sealed class BossDefinition : EnemyDefinition
    {
        [Header("Boss")]
        public BossId BossId = BossId.MiniBoss;
        public string Title = "BOSS";
        [Min(0.1f)] public float EntranceDuration = 2.5f;
        [Tooltip("Distance from the top of the screen (fraction of height) where the boss settles.")]
        [Range(0.05f, 0.6f)] public float EntranceHeight = 0.2f;
        public BossPhase[] Phases = new BossPhase[0];
        [Min(1)] public int DeathExplosions = 8;
        [Min(0.1f)] public float DeathSequenceSeconds = 1.6f;

        [Header("Destructible parts")]
        public BossPartSpec[] Parts = new BossPartSpec[0];

        [Header("Body segments (Leviathan)")]
        [Min(0)] public int SegmentCount = 0;
        public Sprite SegmentSprite;
        [Min(0.1f)] public float SegmentSpacing = 0.9f;
        [Min(0.1f)] public float SegmentScale = 0.8f;

        public override bool IsBoss => true;
        public bool IsMainBoss => BossId != BossId.MiniBoss;
    }
}
