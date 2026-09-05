namespace Starfall.Core
{
    /// <summary>
    /// Volatile data carried between scenes for the current run (never persisted).
    /// Kept static on purpose: it is plain data with no behaviour. See docs/ARCHITECTURE.md.
    /// </summary>
    public static class GameSession
    {
        public static int CurrentStageIndex;
        /// <summary>Score accumulated in previous stages of this run.</summary>
        public static int CarriedScore;
        /// <summary>Lives carried into the next stage. -1 means "use config default".</summary>
        public static int CarriedLives = -1;

        public static void StartNewRun(int stageIndex)
        {
            CurrentStageIndex = stageIndex;
            CarriedScore = 0;
            CarriedLives = -1;
        }

        public static void PrepareNextStage(int score, int lives)
        {
            CurrentStageIndex++;
            CarriedScore = score;
            CarriedLives = lives;
        }
    }
}
