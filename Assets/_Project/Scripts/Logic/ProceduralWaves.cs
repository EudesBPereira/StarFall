// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;
using System.Collections.Generic;

namespace Starfall.Logic
{
    /// <summary>Enemy archetypes known to the procedural generator (mapped to definitions by the runtime).</summary>
    public enum ProceduralEnemy
    {
        Drone = 0,
        Interceptor = 1,
        Bomber = 2,
        Kamikaze = 3,
        Shield = 4,
        Supply = 5,
        Elite = 6,
        Turret = 7,
    }

    public struct ProceduralSpawn
    {
        public ProceduralEnemy Enemy;
        public int Count;
        public float Interval;
        public float DelayBefore;
        /// <summary>Pattern index in [0, PatternCount).</summary>
        public int Pattern;
    }

    /// <summary>
    /// Deterministic wave generator for Survival and Daily Challenge. Same seed + wave index = same wave.
    /// Difficulty ramps with the wave index: more enemies, harder archetypes, shorter intervals.
    /// </summary>
    public static class ProceduralWaves
    {
        public const int PatternCount = 8;

        public static int SeedFromDate(DateTime date) => date.Year * 10000 + date.Month * 100 + date.Day;

        public static List<ProceduralSpawn> Generate(int seed, int waveIndex, List<ProceduralSpawn> buffer = null)
        {
            var list = buffer ?? new List<ProceduralSpawn>(6);
            list.Clear();
            var rng = new Random(unchecked(seed * 397 + waveIndex * 7919));
            int wave = Math.Max(0, waveIndex);

            int budget = 6 + wave * 2 + Math.Min(wave / 3, 12);
            float speedFactor = 1f / (1f + wave * 0.04f);
            int entries = 2 + Math.Min(wave / 4, 3);

            for (int e = 0; e < entries && budget > 0; e++)
            {
                var enemy = PickEnemy(rng, wave);
                int cost = Cost(enemy);
                int maxCount = Math.Max(1, budget / cost);
                int count = Math.Min(maxCount, 2 + rng.Next(1, 4 + wave / 5));
                if (enemy == ProceduralEnemy.Elite || enemy == ProceduralEnemy.Turret) count = Math.Min(count, 1 + wave / 8);
                if (enemy == ProceduralEnemy.Supply) count = 1;
                budget -= count * cost;
                list.Add(new ProceduralSpawn
                {
                    Enemy = enemy,
                    Count = Math.Max(1, count),
                    Interval = Math.Max(0.18f, (0.35f + (float)rng.NextDouble() * 0.5f) * speedFactor),
                    DelayBefore = e == 0 ? 0f : 0.6f + (float)rng.NextDouble() * 1.2f,
                    Pattern = rng.Next(PatternCount),
                });
            }

            // Every 5th wave drops a supply unit so the run stays winnable.
            if (wave > 0 && wave % 5 == 0)
                list.Add(new ProceduralSpawn { Enemy = ProceduralEnemy.Supply, Count = 1, Interval = 0f, DelayBefore = 1f, Pattern = 1 });
            return list;
        }

        /// <summary>Enemy stat multiplier for a wave (applied by the runtime to hull/damage).</summary>
        public static float DifficultyMultiplier(int waveIndex) => 1f + Math.Max(0, waveIndex) * 0.06f;

        private static ProceduralEnemy PickEnemy(Random rng, int wave)
        {
            int roll = rng.Next(100);
            if (wave < 2) return roll < 70 ? ProceduralEnemy.Drone : ProceduralEnemy.Interceptor;
            if (wave < 5)
            {
                if (roll < 40) return ProceduralEnemy.Drone;
                if (roll < 70) return ProceduralEnemy.Interceptor;
                if (roll < 85) return ProceduralEnemy.Kamikaze;
                return ProceduralEnemy.Bomber;
            }
            if (wave < 10)
            {
                if (roll < 25) return ProceduralEnemy.Drone;
                if (roll < 45) return ProceduralEnemy.Interceptor;
                if (roll < 60) return ProceduralEnemy.Kamikaze;
                if (roll < 75) return ProceduralEnemy.Bomber;
                if (roll < 90) return ProceduralEnemy.Shield;
                return ProceduralEnemy.Elite;
            }
            if (roll < 15) return ProceduralEnemy.Drone;
            if (roll < 35) return ProceduralEnemy.Interceptor;
            if (roll < 50) return ProceduralEnemy.Kamikaze;
            if (roll < 62) return ProceduralEnemy.Bomber;
            if (roll < 76) return ProceduralEnemy.Shield;
            if (roll < 90) return ProceduralEnemy.Elite;
            return ProceduralEnemy.Turret;
        }

        private static int Cost(ProceduralEnemy enemy)
        {
            switch (enemy)
            {
                case ProceduralEnemy.Drone: return 1;
                case ProceduralEnemy.Interceptor: return 2;
                case ProceduralEnemy.Kamikaze: return 2;
                case ProceduralEnemy.Bomber: return 3;
                case ProceduralEnemy.Shield: return 3;
                case ProceduralEnemy.Supply: return 1;
                case ProceduralEnemy.Elite: return 5;
                case ProceduralEnemy.Turret: return 4;
                default: return 1;
            }
        }
    }
}
