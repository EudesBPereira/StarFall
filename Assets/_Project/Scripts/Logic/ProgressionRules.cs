// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;

namespace Starfall.Logic
{
    public enum GameModeId
    {
        Campaign = 0,
        Survival = 1,
        BossRush = 2,
        DailyChallenge = 3,
    }

    public enum ShipId
    {
        Vanguard = 0,
        Falcon = 1,
        Titan = 2,
        Phantom = 3,
        NovaX = 4,
    }

    public enum WeaponId
    {
        Laser = 0,
        DoubleLaser = 1,
        Plasma = 2,
        SpreadShot = 3,
        Railgun = 4,
        Missiles = 5,
        EnergyCannon = 6,
    }

    /// <summary>Numbers collected during one run and consumed by rewards/achievements.</summary>
    public sealed class RunStats
    {
        public GameModeId Mode;
        public int StageIndex;
        public int Score;
        public int Kills;
        public int EliteKills;
        public int BossKills;
        public int ComponentsCollected;
        public int LivesLost;
        public int HitsTaken;
        public int WavesSurvived;
        public int HighestMultiplier = 1;
        public bool Completed;
        public int BossesDefeatedMask;
    }

    public struct RunRewards
    {
        public int Credits;
        public int Xp;
        public int Components;
    }

    /// <summary>Rewards, unlock costs and pilot level rules (GDD §13, §15, §4). Values in docs/BALANCING.md.</summary>
    public static class ProgressionRules
    {
        public const int ShipCount = 5;
        public const int WeaponCount = 7;

        private static readonly int[] ShipCost = { 0, 2500, 4000, 6000, 0 };
        private static readonly int[] WeaponCost = { 0, 1500, 3000, 2500, 4500, 5000, 7000 };

        public static int ShipCreditCost(ShipId ship) => ShipCost[(int)ship];
        public static int WeaponCreditCost(WeaponId weapon) => WeaponCost[(int)weapon];

        /// <summary>Nova-X is legendary: it unlocks by finishing the campaign, never by credits.</summary>
        public static bool ShipRequiresCampaign(ShipId ship) => ship == ShipId.NovaX;

        public static RunRewards ComputeRewards(RunStats stats, int stageCompletionBonus)
        {
            if (stats == null) return default;
            float completionFactor = stats.Completed ? 1f : 0.5f;
            int credits = (int)Math.Round((stats.Score / 10f + (stats.Completed ? stageCompletionBonus / 5f : 0f)) * completionFactor);
            int xp = stats.Kills * 5 + stats.EliteKills * 20 + stats.BossKills * 200 + (stats.Completed ? 150 : 0);
            return new RunRewards { Credits = Math.Max(0, credits), Xp = Math.Max(0, xp), Components = Math.Max(0, stats.ComponentsCollected) };
        }

        public static void ApplyRewards(SaveData save, in RunRewards rewards)
        {
            if (save == null) return;
            save.credits += rewards.Credits;
            save.xp += rewards.Xp;
            save.components += rewards.Components;
            save.totalRuns++;
        }

        /// <summary>Pilot level: level n needs 100 * n^2 total XP.</summary>
        public static int PilotLevel(int xp) => (int)Math.Floor(Math.Sqrt(Math.Max(0, xp) / 100.0)) + 1;

        public static int XpForLevel(int level) => 100 * (level - 1) * (level - 1);

        public static bool CanBuyShip(SaveData save, ShipId ship)
        {
            if (save == null || save.IsShipUnlocked((int)ship) || ShipRequiresCampaign(ship)) return false;
            return save.credits >= ShipCreditCost(ship);
        }

        public static bool BuyShip(SaveData save, ShipId ship)
        {
            if (!CanBuyShip(save, ship)) return false;
            save.credits -= ShipCreditCost(ship);
            save.UnlockShip((int)ship);
            return true;
        }

        public static bool CanBuyWeapon(SaveData save, WeaponId weapon)
        {
            if (save == null || save.IsWeaponUnlocked((int)weapon)) return false;
            return save.credits >= WeaponCreditCost(weapon);
        }

        public static bool BuyWeapon(SaveData save, WeaponId weapon)
        {
            if (!CanBuyWeapon(save, weapon)) return false;
            save.credits -= WeaponCreditCost(weapon);
            save.UnlockWeapon((int)weapon);
            return true;
        }

        /// <summary>Records campaign completion of a stage and unlocks Nova-X after the final stage.</summary>
        public static void RecordCampaignStage(SaveData save, int stageIndex, int stageCount, int score)
        {
            if (save == null) return;
            save.MarkStageCompleted(stageIndex);
            save.unlockedStage = StageProgression.UnlockAfterCompletion(save.unlockedStage, stageIndex, stageCount);
            if (stageIndex >= 0 && stageIndex < save.stageBestScores.Length && score > save.stageBestScores[stageIndex])
                save.stageBestScores[stageIndex] = score;
            if (stageIndex == stageCount - 1) save.UnlockShip((int)ShipId.NovaX);
        }

        public static bool AllStagesCompleted(SaveData save, int stageCount)
        {
            if (save == null) return false;
            for (int i = 0; i < stageCount; i++) if (!save.IsStageCompleted(i)) return false;
            return true;
        }

        public static bool AllShipsUnlocked(SaveData save)
        {
            for (int i = 0; i < ShipCount; i++) if (!save.IsShipUnlocked(i)) return false;
            return true;
        }

        public static bool AllWeaponsUnlocked(SaveData save)
        {
            for (int i = 0; i < WeaponCount; i++) if (!save.IsWeaponUnlocked(i)) return false;
            return true;
        }
    }
}
