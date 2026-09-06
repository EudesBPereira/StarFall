// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;

namespace Starfall.Logic
{
    /// <summary>
    /// Deterministic choices for "partially random" stages (plan §2, §7.5): the same run seed always produces the
    /// same variant picks, so a run is reproducible and a leaderboard row can carry its seed.
    /// </summary>
    public static class StageVariation
    {
        /// <summary>Stable hash of (seed, stage, event) so events do not share the same random stream.</summary>
        public static int Hash(int seed, int stageIndex, int eventIndex)
        {
            unchecked
            {
                int h = seed * 486187739;
                h ^= (stageIndex + 1) * 1000003;
                h ^= (eventIndex + 1) * 8191;
                h ^= h >> 13;
                h *= 1274126177;
                h ^= h >> 16;
                return h;
            }
        }

        /// <summary>Picks an index in [0, variantCount) for an event; -1 when there is nothing to pick.</summary>
        public static int PickVariant(int seed, int stageIndex, int eventIndex, int variantCount)
        {
            if (variantCount <= 0) return -1;
            if (variantCount == 1) return 0;
            var rng = new Random(Hash(seed, stageIndex, eventIndex));
            return rng.Next(variantCount);
        }

        /// <summary>Whether an optional event (random encounter) fires for this run.</summary>
        public static bool Roll(int seed, int stageIndex, int eventIndex, float chance)
        {
            if (chance >= 1f) return true;
            if (chance <= 0f) return false;
            var rng = new Random(Hash(seed, stageIndex, eventIndex) ^ 0x5bd1e995);
            return rng.NextDouble() < chance;
        }

        /// <summary>Normalized value in [0,1) for hazards (band position, spawn side...).</summary>
        public static float Value(int seed, int stageIndex, int eventIndex, int salt = 0)
        {
            var rng = new Random(Hash(seed, stageIndex, eventIndex) + salt * 7919);
            return (float)rng.NextDouble();
        }
    }
}
