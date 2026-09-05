// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System.Collections.Generic;

namespace Starfall.Logic
{
    /// <summary>Local achievements (GDD §22).</summary>
    public enum AchievementId
    {
        FirstKill = 0,
        Survivor = 1,
        Collector = 2,
        BossHunter = 3,
        GalacticLegend = 4,
    }

    public static class AchievementRules
    {
        public const int Count = 5;

        public static string DisplayName(AchievementId id)
        {
            switch (id)
            {
                case AchievementId.FirstKill: return "First Kill";
                case AchievementId.Survivor: return "Survivor";
                case AchievementId.Collector: return "Collector";
                case AchievementId.BossHunter: return "Boss Hunter";
                case AchievementId.GalacticLegend: return "Galactic Legend";
                default: return id.ToString();
            }
        }

        public static string Description(AchievementId id)
        {
            switch (id)
            {
                case AchievementId.FirstKill: return "Destroy your first enemy.";
                case AchievementId.Survivor: return "Finish a stage without losing a life.";
                case AchievementId.Collector: return "Max out every upgrade.";
                case AchievementId.BossHunter: return "Defeat every boss.";
                case AchievementId.GalacticLegend: return "Complete 100%: stages, ships, weapons, upgrades.";
                default: return "";
            }
        }

        /// <summary>
        /// Evaluates every achievement against the save + optional run and returns the newly earned ones
        /// (already granted to the save).
        /// </summary>
        public static List<AchievementId> Evaluate(SaveData save, RunStats run, int stageCount, int bossCount, List<AchievementId> buffer = null)
        {
            var earned = buffer ?? new List<AchievementId>(4);
            earned.Clear();
            if (save == null) return earned;

            Check(save, earned, AchievementId.FirstKill, save.totalKills > 0 || (run != null && run.Kills > 0));
            Check(save, earned, AchievementId.Survivor, run != null && run.Mode == GameModeId.Campaign && run.Completed && run.LivesLost == 0);
            Check(save, earned, AchievementId.Collector, UpgradeCatalog.AllMaxed(save));
            Check(save, earned, AchievementId.BossHunter, bossCount > 0 && AllBossesDefeated(save, bossCount));
            bool complete = ProgressionRules.AllStagesCompleted(save, stageCount) && ProgressionRules.AllShipsUnlocked(save)
                            && ProgressionRules.AllWeaponsUnlocked(save) && UpgradeCatalog.AllMaxed(save)
                            && save.HasAchievement((int)AchievementId.FirstKill) && save.HasAchievement((int)AchievementId.Survivor)
                            && save.HasAchievement((int)AchievementId.BossHunter);
            Check(save, earned, AchievementId.GalacticLegend, complete);
            return earned;
        }

        public static bool AllBossesDefeated(SaveData save, int bossCount)
        {
            for (int i = 0; i < bossCount; i++) if (!save.IsBossDefeated(i)) return false;
            return true;
        }

        private static void Check(SaveData save, List<AchievementId> earned, AchievementId id, bool condition)
        {
            if (!condition || save.HasAchievement((int)id)) return;
            save.GrantAchievement((int)id);
            earned.Add(id);
        }
    }
}
