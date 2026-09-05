using System;
using Starfall.Logic;

namespace Starfall.Core
{
    /// <summary>
    /// Volatile data carried between scenes for the current run (never persisted).
    /// Kept static on purpose: it is plain data with no behaviour. See docs/ARCHITECTURE.md.
    /// </summary>
    public static class GameSession
    {
        public static GameModeId Mode = GameModeId.Campaign;
        public static int CurrentStageIndex;
        /// <summary>Score accumulated in previous stages of this run.</summary>
        public static int CarriedScore;
        /// <summary>Lives carried into the next stage. -1 means "use config default".</summary>
        public static int CarriedLives = -1;
        /// <summary>Seed for procedural modes (Survival uses a random one, Daily uses the date).</summary>
        public static int Seed;
        /// <summary>Accumulated stats of the whole run (all stages).</summary>
        public static RunStats Run = new RunStats();

        public static bool IsEndless => Mode == GameModeId.Survival || Mode == GameModeId.DailyChallenge;

        public static void StartNewRun(int stageIndex) => StartNewRun(GameModeId.Campaign, stageIndex, Environment.TickCount);

        public static void StartNewRun(GameModeId mode, int stageIndex, int seed)
        {
            Mode = mode;
            CurrentStageIndex = stageIndex;
            CarriedScore = 0;
            CarriedLives = -1;
            Seed = seed;
            Run = new RunStats { Mode = mode, StageIndex = stageIndex };
        }

        public static void StartDaily(DateTime date)
        {
            StartNewRun(GameModeId.DailyChallenge, 0, ProceduralWaves.SeedFromDate(date));
        }

        public static void PrepareNextStage(int score, int lives)
        {
            CurrentStageIndex++;
            CarriedScore = score;
            CarriedLives = lives;
            Run.StageIndex = CurrentStageIndex;
        }
    }
}
