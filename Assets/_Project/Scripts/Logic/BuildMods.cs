// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;
using System.Collections.Generic;

namespace Starfall.Logic
{
    /// <summary>Temporary in-mission upgrades (plan §2 "builds temporárias"). They last for the current stage only.</summary>
    public enum BuildModId
    {
        Overclock = 0,     // +fire rate
        HeavyRounds = 1,   // +damage
        PiercingTips = 2,  // +1 pierce
        WideSpread = 3,    // +1 angled shot
        ReactivePlating = 4, // +max shield
        NanoRepair = 5,    // lifesteal per kill
        Afterburner = 6,   // +speed
        RiskTuner = 7,     // +overdrive gain
        Magnet = 8,        // pickups attracted
        LuckyCore = 9,     // +crit chance
        FocusLens = 10,    // +projectile speed and range
        EnergyCells = 11,  // +ultimate charge
    }

    /// <summary>Aggregated effect of the chosen mods. All multipliers default to 1, additive values to 0.</summary>
    public struct BuildModifiers
    {
        public float FireRate;
        public float Damage;
        public int Pierce;
        public int ExtraShots;
        public float ShieldBonus;
        public float LifestealPerKill;
        public float Speed;
        public float OverdriveGain;
        public bool Magnet;
        public float CritChance;
        public float ProjectileSpeed;
        public float Range;
        public float UltimateCharge;

        public static BuildModifiers Identity => new BuildModifiers
        {
            FireRate = 1f, Damage = 1f, Pierce = 0, ExtraShots = 0, ShieldBonus = 0f, LifestealPerKill = 0f,
            Speed = 1f, OverdriveGain = 1f, Magnet = false, CritChance = 0f, ProjectileSpeed = 1f, Range = 1f, UltimateCharge = 1f,
        };
    }

    /// <summary>Catalog, stacking rules and seeded drafts for build mods.</summary>
    public static class BuildMods
    {
        public const int Count = 12;
        public const int DraftSize = 3;
        /// <summary>A mod can be taken up to this many times (stacks are capped).</summary>
        public const int MaxStacks = 3;

        public static string DisplayName(BuildModId id)
        {
            switch (id)
            {
                case BuildModId.Overclock: return "Overclock";
                case BuildModId.HeavyRounds: return "Heavy Rounds";
                case BuildModId.PiercingTips: return "Piercing Tips";
                case BuildModId.WideSpread: return "Wide Spread";
                case BuildModId.ReactivePlating: return "Reactive Plating";
                case BuildModId.NanoRepair: return "Nano Repair";
                case BuildModId.Afterburner: return "Afterburner";
                case BuildModId.RiskTuner: return "Risk Tuner";
                case BuildModId.Magnet: return "Magnet";
                case BuildModId.LuckyCore: return "Lucky Core";
                case BuildModId.FocusLens: return "Focus Lens";
                case BuildModId.EnergyCells: return "Energy Cells";
                default: return id.ToString();
            }
        }

        public static string Description(BuildModId id)
        {
            switch (id)
            {
                case BuildModId.Overclock: return "+12% fire rate";
                case BuildModId.HeavyRounds: return "+15% damage";
                case BuildModId.PiercingTips: return "Shots pierce one more enemy";
                case BuildModId.WideSpread: return "+1 angled shot per volley";
                case BuildModId.ReactivePlating: return "+25 max shield, restored now";
                case BuildModId.NanoRepair: return "Kills restore 1% hull";
                case BuildModId.Afterburner: return "+8% speed";
                case BuildModId.RiskTuner: return "+20% Overdrive gain";
                case BuildModId.Magnet: return "Pickups fly to you";
                case BuildModId.LuckyCore: return "+8% critical chance";
                case BuildModId.FocusLens: return "+15% projectile speed and range";
                case BuildModId.EnergyCells: return "+20% Ultimate charge";
                default: return "";
            }
        }

        /// <summary>Magnet cannot stack; everything else stacks up to MaxStacks.</summary>
        public static int StackLimit(BuildModId id) => id == BuildModId.Magnet ? 1 : MaxStacks;

        public static BuildModifiers Compute(IReadOnlyList<BuildModId> chosen)
        {
            var m = BuildModifiers.Identity;
            if (chosen == null) return m;
            for (int i = 0; i < chosen.Count; i++)
            {
                switch (chosen[i])
                {
                    case BuildModId.Overclock: m.FireRate *= 1.12f; break;
                    case BuildModId.HeavyRounds: m.Damage *= 1.15f; break;
                    case BuildModId.PiercingTips: m.Pierce += 1; break;
                    case BuildModId.WideSpread: m.ExtraShots += 1; break;
                    case BuildModId.ReactivePlating: m.ShieldBonus += 25f; break;
                    case BuildModId.NanoRepair: m.LifestealPerKill += 0.01f; break;
                    case BuildModId.Afterburner: m.Speed *= 1.08f; break;
                    case BuildModId.RiskTuner: m.OverdriveGain *= 1.2f; break;
                    case BuildModId.Magnet: m.Magnet = true; break;
                    case BuildModId.LuckyCore: m.CritChance += 0.08f; break;
                    case BuildModId.FocusLens: m.ProjectileSpeed *= 1.15f; m.Range *= 1.15f; break;
                    case BuildModId.EnergyCells: m.UltimateCharge *= 1.2f; break;
                }
            }
            m.LifestealPerKill = Math.Min(m.LifestealPerKill, FactionRules.LifestealCap);
            return m;
        }

        public static int CountOf(IReadOnlyList<BuildModId> chosen, BuildModId id)
        {
            int n = 0;
            if (chosen == null) return 0;
            for (int i = 0; i < chosen.Count; i++) if (chosen[i] == id) n++;
            return n;
        }

        /// <summary>
        /// Seeded draft of distinct offers that still have stacks left. Same seed/stage/draft index = same offer.
        /// Returns fewer than <paramref name="size"/> when the pool is exhausted.
        /// </summary>
        public static List<BuildModId> Draft(int seed, int stageIndex, int draftIndex, IReadOnlyList<BuildModId> chosen, int size = DraftSize, List<BuildModId> buffer = null)
        {
            var result = buffer ?? new List<BuildModId>(size);
            result.Clear();
            var pool = new List<BuildModId>(Count);
            for (int i = 0; i < Count; i++)
            {
                var id = (BuildModId)i;
                if (CountOf(chosen, id) < StackLimit(id)) pool.Add(id);
            }
            var rng = new Random(StageVariation.Hash(seed, stageIndex, 1000 + draftIndex));
            while (result.Count < size && pool.Count > 0)
            {
                int k = rng.Next(pool.Count);
                result.Add(pool[k]);
                pool.RemoveAt(k);
            }
            return result;
        }
    }
}
