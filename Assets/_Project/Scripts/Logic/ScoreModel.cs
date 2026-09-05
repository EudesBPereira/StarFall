// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;

namespace Starfall.Logic
{
    /// <summary>Score + multiplier rules. Multiplier grows with kills without taking damage and resets on damage.</summary>
    public sealed class ScoreModel
    {
        public int Score { get; private set; }
        public int Multiplier { get; private set; } = 1;
        public int MaxMultiplier { get; }
        /// <summary>Kills needed (without taking damage) to raise the multiplier by one.</summary>
        public int KillsPerStep { get; }
        public int KillStreak { get; private set; }
        public int HighestMultiplier { get; private set; } = 1;
        public int EnemiesDestroyed { get; private set; }
        public int DamageTakenCount { get; private set; }

        public event Action<int> ScoreChanged;
        public event Action<int> MultiplierChanged;

        public ScoreModel(int maxMultiplier = 10, int killsPerStep = 4)
        {
            MaxMultiplier = Math.Max(1, maxMultiplier);
            KillsPerStep = Math.Max(1, killsPerStep);
        }

        /// <summary>Registers a kill and returns the points awarded (base * multiplier).</summary>
        public int RegisterKill(int baseScore)
        {
            EnemiesDestroyed++;
            int awarded = Math.Max(0, baseScore) * Multiplier;
            if (awarded > 0)
            {
                Score += awarded;
                ScoreChanged?.Invoke(Score);
            }

            KillStreak++;
            if (KillStreak >= KillsPerStep && Multiplier < MaxMultiplier)
            {
                KillStreak = 0;
                Multiplier++;
                if (Multiplier > HighestMultiplier) HighestMultiplier = Multiplier;
                MultiplierChanged?.Invoke(Multiplier);
            }
            return awarded;
        }

        /// <summary>Adds points without affecting the multiplier (stage bonus, etc).</summary>
        public void AddBonus(int points)
        {
            if (points <= 0) return;
            Score += points;
            ScoreChanged?.Invoke(Score);
        }

        public void RegisterPlayerDamaged()
        {
            DamageTakenCount++;
            KillStreak = 0;
            if (Multiplier != 1)
            {
                Multiplier = 1;
                MultiplierChanged?.Invoke(Multiplier);
            }
        }

        public void Reset()
        {
            Score = 0;
            Multiplier = 1;
            KillStreak = 0;
            HighestMultiplier = 1;
            EnemiesDestroyed = 0;
            DamageTakenCount = 0;
            ScoreChanged?.Invoke(Score);
            MultiplierChanged?.Invoke(Multiplier);
        }
    }
}
