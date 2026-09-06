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
        /// <summary>Biomech faction starter (plan §6).</summary>
        Symbiont = 5,
        /// <summary>Cyber faction starter with companion drone (plan §6).</summary>
        Nexus = 6,
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
        /// <summary>Biomech: living, seeking spores. Unlocked with the Symbiont.</summary>
        Spores = 7,
    }

    /// <summary>Playable factions (plan §6). Each reacts differently to Risk and Overdrive.</summary>
    public enum FactionId
    {
        Federation = 0,
        Biomech = 1,
        Cyber = 2,
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
        public float HighestRiskMultiplier = 1f;
        public int Grazes;
        public int OverdriveActivations;
        public float SecondsInDanger;
        public float ElapsedSeconds;
        public bool UsedRevive;
        public bool Completed;
        public int BossesDefeatedMask;
        public StageRank Rank;
    }

    public struct RunRewards
    {
        public int Credits;
        public int Xp;
        public int Components;
        // Breakdown for the results screen (plan §11.5)
        public int BaseCredits;
        public int RankCredits;
        public int RiskCredits;
        public int FirstClearCredits;
        public int DifficultyCredits;
        public int ObjectiveCredits;
    }

    /// <summary>Rewards, unlock costs and pilot level rules (plan §11, §13). Values in docs/BALANCING.md.</summary>
    public static class ProgressionRules
    {
        public const int ShipCount = 7;
        public const int WeaponCount = 8;

        private static readonly int[] ShipCost = { 0, 2500, 4000, 6000, 0, 3500, 5000 };
        private static readonly int[] WeaponCost = { 0, 1500, 3000, 2500, 4500, 5000, 7000, 3500 };
        private static readonly int[] RankCreditBonus = { 0, 50, 120, 220, 350, 500, 700 }; // D..SSS

        /// <summary>Risk bonus is capped so it never dominates the economy (plan §11.5 "bônus de risco limitado").</summary>
        public const int RiskCreditCap = 150;
        public const int EndlessCreditsPerWave = 12;
        public const int EndlessCreditsCap = 600;

        public static int ShipCreditCost(ShipId ship) => ShipCost[(int)ship];
        public static int WeaponCreditCost(WeaponId weapon) => WeaponCost[(int)weapon];

        /// <summary>Nova-X is legendary: it unlocks by finishing the campaign, never by credits.</summary>
        public static bool ShipRequiresCampaign(ShipId ship) => ship == ShipId.NovaX;

        /// <summary>
        /// Credits are no longer score / 10 (plan §11.4). They come from a stage base value plus bounded bonuses,
        /// so tuning the score never inflates the economy.
        /// </summary>
        public static RunRewards ComputeRewards(RunStats stats, int stageBaseCredits, bool firstClear)
        {
            if (stats == null) return default;
            var r = new RunRewards();

            if (stats.Mode == GameModeId.Campaign)
            {
                float completion = stats.Completed ? 1f : 0.4f;
                r.BaseCredits = (int)Math.Round(Math.Max(0, stageBaseCredits) * completion);
                r.RankCredits = stats.Completed ? RankCreditBonus[(int)stats.Rank] : 0;
                r.RiskCredits = Math.Min(RiskCreditCap, (int)(stats.SecondsInDanger * 2f));
                r.FirstClearCredits = stats.Completed && firstClear ? Math.Max(0, stageBaseCredits) : 0;
            }
            else if (stats.Mode == GameModeId.BossRush)
            {
                r.BaseCredits = stats.BossKills * 120;
                r.RiskCredits = Math.Min(RiskCreditCap, (int)(stats.SecondsInDanger * 2f));
                r.FirstClearCredits = stats.Completed && firstClear ? 400 : 0;
            }
            else
            {
                r.BaseCredits = Math.Min(EndlessCreditsCap, stats.WavesSurvived * EndlessCreditsPerWave);
                r.RiskCredits = Math.Min(RiskCreditCap / 2, (int)(stats.SecondsInDanger));
            }
            if (stats.UsedRevive) r.RankCredits /= 2;

            r.Credits = r.BaseCredits + r.RankCredits + r.RiskCredits + r.FirstClearCredits + r.DifficultyCredits + r.ObjectiveCredits;
            r.Xp = stats.Kills * 5 + stats.EliteKills * 20 + stats.BossKills * 200 + (stats.Completed ? 150 : 0) + stats.Grazes * 2;
            r.Components = Math.Max(0, stats.ComponentsCollected);
            return r;
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
            // Faction starters bring their signature weapon (plan §6: arma primária própria).
            if (ship == ShipId.Symbiont) save.UnlockWeapon((int)WeaponId.Spores);
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

        /// <summary>Records campaign completion of a stage, best score/rank, and unlocks Nova-X after the final stage.</summary>
        public static void RecordCampaignStage(SaveData save, int stageIndex, int stageCount, int score, StageRank rank = StageRank.D)
        {
            if (save == null) return;
            save.MarkStageCompleted(stageIndex);
            save.unlockedStage = StageProgression.UnlockAfterCompletion(save.unlockedStage, stageIndex, stageCount);
            if (stageIndex >= 0 && stageIndex < save.stageBestScores.Length && score > save.stageBestScores[stageIndex])
                save.stageBestScores[stageIndex] = score;
            if (save.stageBestRanks != null && stageIndex >= 0 && stageIndex < save.stageBestRanks.Length && (int)rank > save.stageBestRanks[stageIndex])
                save.stageBestRanks[stageIndex] = (int)rank;
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
