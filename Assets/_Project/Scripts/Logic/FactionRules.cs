// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;

namespace Starfall.Logic
{
    /// <summary>Faction Ultimates (plan §6): each faction has its own special.</summary>
    public enum UltimateKind
    {
        /// <summary>Federation: orbital strike that wipes common enemies and damages elites/bosses.</summary>
        OrbitalStrike = 0,
        /// <summary>Biomech: releases a swarm of seeking spores that hunt enemies for a few seconds.</summary>
        Swarm = 1,
        /// <summary>Cyber: EMP that stuns everything, clears bullets, marks all targets and deals moderate damage.</summary>
        Emp = 2,
    }

    /// <summary>Federation passive: consecutive hits without a miss raise damage (capped).</summary>
    public sealed class PrecisionModel
    {
        public int Streak { get; private set; }
        public int BestStreak { get; private set; }

        public void RegisterHit()
        {
            Streak++;
            if (Streak > BestStreak) BestStreak = Streak;
        }

        public void RegisterMiss() => Streak = 0;

        public void Reset() => Streak = 0;

        /// <summary>Damage multiplier for the current streak: 1 + min(cap, streak * perHit).</summary>
        public float Multiplier(float bonusPerHit, float cap) => 1f + Math.Min(Math.Max(0f, cap), Streak * Math.Max(0f, bonusPerHit));
    }

    /// <summary>Numbers shared by the three factions. Values in docs/BALANCING.md.</summary>
    public static class FactionRules
    {
        public const float PrecisionCap = 0.25f;

        // Cyber: marked targets take extra damage for a while.
        public const float MarkSeconds = 4f;
        public const float MarkDamageMultiplier = 1.25f;
        public const float EmpStunSeconds = 3f;
        public const float EmpDamage = 90f;
        public const float EmpMarkSeconds = 6f;

        // Biomech: kills restore a fraction of max hull; the swarm Ultimate launches seekers.
        public const float LifestealCap = 0.05f;
        public const int SwarmSeekers = 10;
        public const float SwarmSeekerDamage = 45f;
        public const float SwarmSeekerLifetime = 6f;

        public static float LifestealAmount(float maxHull, float fractionPerKill)
        {
            fractionPerKill = Math.Clamp(fractionPerKill, 0f, LifestealCap);
            return Math.Max(0f, maxHull) * fractionPerKill;
        }

        public static FactionId FactionOf(ShipId ship)
        {
            switch (ship)
            {
                case ShipId.Symbiont: return FactionId.Biomech;
                case ShipId.Phantom:
                case ShipId.Nexus: return FactionId.Cyber;
                default: return FactionId.Federation;
            }
        }

        public static string FactionName(FactionId faction)
        {
            switch (faction)
            {
                case FactionId.Biomech: return "BIOMECH";
                case FactionId.Cyber: return "CYBER CORP";
                default: return "FEDERATION";
            }
        }

        public static UltimateKind UltimateOf(FactionId faction)
        {
            switch (faction)
            {
                case FactionId.Biomech: return UltimateKind.Swarm;
                case FactionId.Cyber: return UltimateKind.Emp;
                default: return UltimateKind.OrbitalStrike;
            }
        }

        public static string UltimateName(UltimateKind kind)
        {
            switch (kind)
            {
                case UltimateKind.Swarm: return "SPORE SWARM";
                case UltimateKind.Emp: return "EMP BURST";
                default: return "ORBITAL STRIKE";
            }
        }
    }
}
