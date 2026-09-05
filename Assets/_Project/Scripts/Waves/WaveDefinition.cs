using System;
using Starfall.Enemies;
using UnityEngine;

namespace Starfall.Waves
{
    public enum SpawnPattern
    {
        TopRandom = 0,
        TopCenter = 1,
        TopLeft = 2,
        TopRight = 3,
        /// <summary>Entries spread evenly across the width.</summary>
        TopLine = 4,
        /// <summary>V formation: center first, then alternating outwards and lower.</summary>
        TopVee = 5,
        /// <summary>Fixed normalized X given by PatternValue.</summary>
        TopFixed = 6,
        /// <summary>Alternates left / right columns.</summary>
        TopAlternate = 7,
    }

    [Serializable]
    public sealed class SpawnEntry
    {
        public EnemyDefinition Enemy;
        [Min(1)] public int Count = 3;
        [Tooltip("Seconds between each spawn of this entry.")]
        [Min(0f)] public float Interval = 0.6f;
        [Tooltip("Seconds to wait before the first spawn of this entry.")]
        [Min(0f)] public float DelayBefore = 0f;
        public SpawnPattern Pattern = SpawnPattern.TopRandom;
        [Tooltip("Pattern parameter (TopFixed: normalized X in [0,1]).")]
        [Range(0f, 1f)] public float PatternValue = 0.5f;
    }

    /// <summary>One wave = a list of spawn entries executed in order.</summary>
    [CreateAssetMenu(menuName = "Starfall/Waves/Wave Definition", fileName = "Wave")]
    public sealed class WaveDefinition : ScriptableObject
    {
        public string Label = "Wave";
        public SpawnEntry[] Entries = new SpawnEntry[0];
        [Tooltip("Wait for every blocking enemy to die (or leave) before the next stage event.")]
        public bool WaitForClear = true;
        [Tooltip("Safety timeout: the wave ends after this many seconds even if enemies remain.")]
        [Min(1f)] public float MaxDuration = 40f;
    }
}
