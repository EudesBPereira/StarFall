using System;
using Starfall.Enemies;
using UnityEngine;

namespace Starfall.Bosses
{
    public enum BossAttackKind
    {
        Ring = 0,
        Aimed = 1,
        Forward = 2,
        SideCannons = 3,
        Missiles = 4,
        FrontLaser = 5,
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
    }

    /// <summary>Boss archetype: an enemy with a scripted entrance and health-driven phases.</summary>
    [CreateAssetMenu(menuName = "Starfall/Bosses/Boss Definition", fileName = "Boss")]
    public sealed class BossDefinition : EnemyDefinition
    {
        [Header("Boss")]
        public string Title = "BOSS";
        [Min(0.1f)] public float EntranceDuration = 2.5f;
        [Tooltip("Distance from the top of the screen (fraction of height) where the boss settles.")]
        [Range(0.05f, 0.6f)] public float EntranceHeight = 0.2f;
        public BossPhase[] Phases = new BossPhase[0];
        [Min(1)] public int DeathExplosions = 8;
        [Min(0.1f)] public float DeathSequenceSeconds = 1.6f;

        public override bool IsBoss => true;
    }
}
