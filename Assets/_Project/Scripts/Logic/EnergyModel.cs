// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;

namespace Starfall.Logic
{
    /// <summary>Ultimate energy bar. Can only be consumed when full.</summary>
    public sealed class EnergyModel
    {
        public float Max { get; }
        public float Current { get; private set; }
        public bool IsFull => Current >= Max;
        public float Fraction => Max <= 0f ? 0f : Current / Max;

        public event Action<float, float> Changed; // current, max
        public event Action BecameFull;

        public EnergyModel(float max)
        {
            Max = Math.Max(1f, max);
        }

        public void Add(float amount)
        {
            if (amount <= 0f || IsFull) return;
            Current = Math.Min(Max, Current + amount);
            Changed?.Invoke(Current, Max);
            if (IsFull) BecameFull?.Invoke();
        }

        /// <summary>Consumes the whole bar. Returns false when not full.</summary>
        public bool TryConsumeAll()
        {
            if (!IsFull) return false;
            Current = 0f;
            Changed?.Invoke(Current, Max);
            return true;
        }

        public void Reset()
        {
            Current = 0f;
            Changed?.Invoke(Current, Max);
        }
    }
}
