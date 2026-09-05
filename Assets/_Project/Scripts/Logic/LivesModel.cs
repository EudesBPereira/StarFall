// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;

namespace Starfall.Logic
{
    /// <summary>Remaining lives for the current campaign.</summary>
    public sealed class LivesModel
    {
        public int Lives { get; private set; }
        public int MaxLives { get; }
        public event Action<int> Changed;

        public LivesModel(int startingLives, int maxLives = 9)
        {
            MaxLives = Math.Max(1, maxLives);
            Lives = Math.Clamp(startingLives, 0, MaxLives);
        }

        /// <summary>Consumes one life. Returns true when the player can still respawn.</summary>
        public bool LoseLife()
        {
            if (Lives <= 0) return false;
            Lives--;
            Changed?.Invoke(Lives);
            return Lives > 0;
        }

        public void AddLife(int amount = 1)
        {
            if (amount <= 0) return;
            Lives = Math.Min(MaxLives, Lives + amount);
            Changed?.Invoke(Lives);
        }

        public void Set(int lives)
        {
            Lives = Math.Clamp(lives, 0, MaxLives);
            Changed?.Invoke(Lives);
        }
    }
}
