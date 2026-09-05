using System;
using UnityEngine;

namespace Starfall.PowerUps
{
    /// <summary>Weighted power-up drop list with a global drop chance.</summary>
    [CreateAssetMenu(menuName = "Starfall/Power-Ups/Drop Table", fileName = "DropTable")]
    public sealed class DropTable : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public PowerUpDefinition PowerUp;
            [Min(0f)] public float Weight;
        }

        [Range(0f, 1f)] public float DropChance = 0.12f;
        public Entry[] Entries = new Entry[0];

        /// <summary>Returns a power-up or null when nothing drops.</summary>
        public PowerUpDefinition Roll(float chanceMultiplier = 1f)
        {
            if (Entries == null || Entries.Length == 0) return null;
            if (UnityEngine.Random.value > DropChance * chanceMultiplier) return null;
            return RollGuaranteed();
        }

        public PowerUpDefinition RollGuaranteed()
        {
            float total = 0f;
            for (int i = 0; i < Entries.Length; i++)
                if (Entries[i].PowerUp != null) total += Mathf.Max(0f, Entries[i].Weight > 0f ? Entries[i].Weight : Entries[i].PowerUp.Weight);
            if (total <= 0f) return null;

            float r = UnityEngine.Random.value * total;
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].PowerUp == null) continue;
                float w = Entries[i].Weight > 0f ? Entries[i].Weight : Entries[i].PowerUp.Weight;
                r -= w;
                if (r <= 0f) return Entries[i].PowerUp;
            }
            return Entries[Entries.Length - 1].PowerUp;
        }
    }
}
