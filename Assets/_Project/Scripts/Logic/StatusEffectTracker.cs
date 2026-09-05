// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;
using System.Collections.Generic;

namespace Starfall.Logic
{
    /// <summary>
    /// Tracks timed effects by kind. Re-applying an active effect renews its duration
    /// (never stacks magnitude). See docs/DECISIONS.md.
    /// </summary>
    public sealed class StatusEffectTracker
    {
        private readonly Dictionary<PowerUpKind, float> _remaining = new Dictionary<PowerUpKind, float>();
        private readonly List<PowerUpKind> _keysBuffer = new List<PowerUpKind>(8);
        private readonly List<PowerUpKind> _expiredBuffer = new List<PowerUpKind>(8);

        public event Action<PowerUpKind> Applied;
        public event Action<PowerUpKind> Expired;

        public int ActiveCount => _remaining.Count;

        public bool IsActive(PowerUpKind kind) => _remaining.ContainsKey(kind);

        public float Remaining(PowerUpKind kind) => _remaining.TryGetValue(kind, out var t) ? t : 0f;

        /// <summary>Applies or renews an effect. Duration becomes max(current, duration).</summary>
        public void Apply(PowerUpKind kind, float duration)
        {
            if (duration <= 0f) return;
            if (_remaining.TryGetValue(kind, out var current))
                _remaining[kind] = Math.Max(current, duration);
            else
                _remaining[kind] = duration;
            Applied?.Invoke(kind);
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f || _remaining.Count == 0) return;
            _keysBuffer.Clear();
            _expiredBuffer.Clear();
            foreach (var kv in _remaining) _keysBuffer.Add(kv.Key);
            for (int i = 0; i < _keysBuffer.Count; i++)
            {
                var kind = _keysBuffer[i];
                float t = _remaining[kind] - deltaTime;
                if (t <= 0f)
                    _expiredBuffer.Add(kind);
                else
                    _remaining[kind] = t;
            }
            for (int i = 0; i < _expiredBuffer.Count; i++)
            {
                _remaining.Remove(_expiredBuffer[i]);
                Expired?.Invoke(_expiredBuffer[i]);
            }
        }

        /// <summary>Removes every active effect, firing Expired for each one.</summary>
        public void ClearAll()
        {
            if (_remaining.Count == 0) return;
            _keysBuffer.Clear();
            foreach (var kv in _remaining) _keysBuffer.Add(kv.Key);
            _remaining.Clear();
            for (int i = 0; i < _keysBuffer.Count; i++) Expired?.Invoke(_keysBuffer[i]);
        }

        /// <summary>Returns the active effect with the longest remaining time, or null.</summary>
        public PowerUpKind? Longest(out float remaining)
        {
            PowerUpKind? best = null;
            remaining = 0f;
            foreach (var kv in _remaining)
            {
                if (kv.Value > remaining)
                {
                    remaining = kv.Value;
                    best = kv.Key;
                }
            }
            return best;
        }
    }
}
