// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;

namespace Starfall.Logic
{
    /// <summary>
    /// Score + combo multiplier rules. Combo grows with kills without taking damage and resets on damage.
    /// Kills are also scaled by the Risk Zone multiplier (STAR RISK plan §9.2): base x combo x risk.
    /// </summary>
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
        public int Grazes { get; private set; }
        public int GrazePoints { get; private set; }
        public float HighestRiskMultiplier { get; private set; } = 1f;

        public event Action<int> ScoreChanged;
        public event Action<int> MultiplierChanged;

        public ScoreModel(int maxMultiplier = 10, int killsPerStep = 4)
        {
            MaxMultiplier = Math.Max(1, maxMultiplier);
            KillsPerStep = Math.Max(1, killsPerStep);
        }

        /// <summary>Registers a kill and returns the points awarded (base x combo x risk).</summary>
        public int RegisterKill(int baseScore, float riskMultiplier = 1f)
        {
            EnemiesDestroyed++;
            riskMultiplier = Math.Max(1f, riskMultiplier);
            if (riskMultiplier > HighestRiskMultiplier) HighestRiskMultiplier = riskMultiplier;
            int awarded = (int)Math.Round(Math.Max(0, baseScore) * Multiplier * riskMultiplier);
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

        /// <summary>Graze bonus: additive, scaled by risk only (never by combo).</summary>
        public int RegisterGraze(float riskMultiplier)
        {
            Grazes++;
            int points = GrazeRules.Points(riskMultiplier);
            GrazePoints += points;
            Score += points;
            ScoreChanged?.Invoke(Score);
            return points;
        }

        /// <summary>Adds points without affecting the multiplier (stage bonus, boss parts, objectives).</summary>
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
            Grazes = 0;
            GrazePoints = 0;
            HighestRiskMultiplier = 1f;
            ScoreChanged?.Invoke(Score);
            MultiplierChanged?.Invoke(Multiplier);
        }
    }
}
