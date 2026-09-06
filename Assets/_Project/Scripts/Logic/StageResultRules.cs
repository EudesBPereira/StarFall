// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;

namespace Starfall.Logic
{
    /// <summary>Stage ranks (plan §9.3). Order matters: higher = better.</summary>
    public enum StageRank
    {
        D = 0,
        C = 1,
        B = 2,
        A = 3,
        S = 4,
        SS = 5,
        SSS = 6,
    }

    /// <summary>Per-stage rank thresholds (score needed for C, B, A, S, SS, SSS). D is anything below C.</summary>
    public struct RankThresholds
    {
        public int C, B, A, S, SS, SSS;

        /// <summary>Derives thresholds from an expected "good run" score so every stage has its own curve.</summary>
        public static RankThresholds FromTarget(int targetScore)
        {
            targetScore = Math.Max(1000, targetScore);
            return new RankThresholds
            {
                C = (int)(targetScore * 0.25f),
                B = (int)(targetScore * 0.45f),
                A = (int)(targetScore * 0.7f),
                S = targetScore,
                SS = (int)(targetScore * 1.4f),
                SSS = (int)(targetScore * 2f),
            };
        }
    }

    /// <summary>Everything that composes the final stage score (plan §9.2) and its rank.</summary>
    public struct StageResult
    {
        public int KillPoints;        // base x combo x risk, already accumulated in ScoreModel
        public int GrazePoints;
        public int PartPoints;        // boss parts (reserved: bosses have no parts yet)
        public int ObjectiveBonus;    // stage completion bonus
        public int TimeBonus;
        public int NoDamageBonus;
        public int DifficultyBonus;   // reserved: difficulty levels not implemented yet
        public bool Completed;
        public bool UsedRevive;
        public StageRank Rank;

        public int Total => KillPoints + GrazePoints + PartPoints + ObjectiveBonus + TimeBonus + NoDamageBonus + DifficultyBonus;
    }

    /// <summary>Score composition, time/no-damage bonuses and rank thresholds. Values in docs/BALANCING.md.</summary>
    public static class StageResultRules
    {
        public const int NoDamageBonusPoints = 5000;
        /// <summary>Points per second under par (capped).</summary>
        public const int TimeBonusPerSecond = 40;
        public const int TimeBonusCap = 6000;

        public static int TimeBonus(float elapsedSeconds, float parSeconds)
        {
            if (parSeconds <= 0f || elapsedSeconds >= parSeconds) return 0;
            return Math.Min(TimeBonusCap, (int)((parSeconds - elapsedSeconds) * TimeBonusPerSecond));
        }

        public static StageRank RankFor(int score, in RankThresholds t)
        {
            if (score >= t.SSS) return StageRank.SSS;
            if (score >= t.SS) return StageRank.SS;
            if (score >= t.S) return StageRank.S;
            if (score >= t.A) return StageRank.A;
            if (score >= t.B) return StageRank.B;
            if (score >= t.C) return StageRank.C;
            return StageRank.D;
        }

        /// <summary>Builds the result. Ranks are only awarded to completed stages; a failed run is D.</summary>
        public static StageResult Compose(ScoreModel score, bool completed, int hitsTaken, float elapsedSeconds, float parSeconds,
            int completionBonus, in RankThresholds thresholds, bool usedRevive = false)
        {
            var r = new StageResult
            {
                KillPoints = Math.Max(0, score.Score - score.GrazePoints - score.PartPoints),
                GrazePoints = score.GrazePoints,
                PartPoints = score.PartPoints,
                ObjectiveBonus = completed ? Math.Max(0, completionBonus) : 0,
                TimeBonus = completed ? TimeBonus(elapsedSeconds, parSeconds) : 0,
                NoDamageBonus = completed && hitsTaken == 0 ? NoDamageBonusPoints : 0,
                DifficultyBonus = 0,
                Completed = completed,
                UsedRevive = usedRevive,
            };
            r.Rank = completed ? RankFor(r.Total, thresholds) : StageRank.D;
            return r;
        }

        public static string RankLabel(StageRank rank) => rank.ToString();
    }
}
