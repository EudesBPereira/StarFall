using System.Collections.Generic;
using Starfall.Logic;
using Starfall.Save;

namespace Starfall.Core
{
    /// <summary>Evaluates achievements against the save and announces new ones through <see cref="GameSignals"/>.</summary>
    public static class AchievementService
    {
        private static readonly List<AchievementId> Buffer = new List<AchievementId>(5);

        /// <summary>Checks every rule; call after runs, purchases and upgrades. Returns how many were unlocked.</summary>
        public static int Evaluate(RunStats run, int stageCount, int bossCount)
        {
            var save = SaveService.Data;
            if (save == null) return 0;
            AchievementRules.Evaluate(save, run, stageCount, bossCount, Buffer);
            for (int i = 0; i < Buffer.Count; i++) GameSignals.RaiseAchievementUnlocked(Buffer[i]);
            if (Buffer.Count > 0) SaveService.Save();
            return Buffer.Count;
        }
    }
}
