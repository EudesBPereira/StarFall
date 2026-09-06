using System.Collections.Generic;
using NUnit.Framework;
using Starfall.Logic;

namespace Starfall.Tests.EditMode
{
    public class UpgradeTreeTests
    {
        [Test]
        public void Purchase_SpendsCreditsAndRaisesLevel()
        {
            var save = SaveData.CreateDefault();
            save.credits = 10000;

            int cost = UpgradeCatalog.CreditCost(UpgradeNode.WeaponDamage, 0);
            Assert.That(UpgradeCatalog.Purchase(save, UpgradeNode.WeaponDamage), Is.True);
            Assert.That(save.credits, Is.EqualTo(10000 - cost));
            Assert.That(save.GetUpgradeLevel((int)UpgradeNode.WeaponDamage), Is.EqualTo(1));
        }

        [Test]
        public void Purchase_FailsWithoutCreditsOrComponents()
        {
            var save = SaveData.CreateDefault();
            save.credits = 10;
            Assert.That(UpgradeCatalog.Purchase(save, UpgradeNode.HullMax), Is.False);

            save.credits = 1000000;
            save.upgradeLevels[(int)UpgradeNode.HullMax] = 3; // level 4 needs components
            save.components = 0;
            Assert.That(UpgradeCatalog.ComponentCost(UpgradeNode.HullMax, 3), Is.GreaterThan(0));
            Assert.That(UpgradeCatalog.Purchase(save, UpgradeNode.HullMax), Is.False);
            save.components = 5;
            Assert.That(UpgradeCatalog.Purchase(save, UpgradeNode.HullMax), Is.True);
        }

        [Test]
        public void Purchase_StopsAtMaxLevel()
        {
            var save = SaveData.CreateDefault();
            save.credits = 1000000;
            save.components = 100;
            for (int i = 0; i < 10; i++) UpgradeCatalog.Purchase(save, UpgradeNode.MobilitySpeed);
            Assert.That(save.GetUpgradeLevel((int)UpgradeNode.MobilitySpeed), Is.EqualTo(UpgradeCatalog.MaxLevel));
        }

        [Test]
        public void Compute_AppliesBonusesPerLevel()
        {
            var levels = new int[UpgradeCatalog.NodeCount];
            levels[(int)UpgradeNode.WeaponDamage] = 5;
            levels[(int)UpgradeNode.ShieldRegen] = 2;
            var m = UpgradeCatalog.Compute(levels);
            Assert.That(m.DamageMultiplier, Is.EqualTo(1.4f).Within(0.001f));
            Assert.That(m.ShieldRegenPerSecondBonus, Is.EqualTo(2f).Within(0.001f));
            Assert.That(m.SpeedMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void CostsGrowWithLevel()
        {
            Assert.That(UpgradeCatalog.CreditCost(UpgradeNode.SpecialPower, 1), Is.GreaterThan(UpgradeCatalog.CreditCost(UpgradeNode.SpecialPower, 0)));
            Assert.That(UpgradeCatalog.CreditCost(UpgradeNode.SpecialPower, UpgradeCatalog.MaxLevel), Is.EqualTo(0));
        }
    }

    public class ProgressionRulesTests
    {
        [Test]
        public void Rewards_UseStageBaseNotScore()
        {
            var win = new RunStats { Mode = GameModeId.Campaign, Score = 10000, Kills = 40, BossKills = 1, Completed = true, ComponentsCollected = 2, Rank = StageRank.A };
            var loss = new RunStats { Mode = GameModeId.Campaign, Score = 10000, Kills = 40, Completed = false };
            var rw = ProgressionRules.ComputeRewards(win, 300, false);
            var rl = ProgressionRules.ComputeRewards(loss, 300, false);
            Assert.That(rw.Credits, Is.GreaterThan(rl.Credits));
            Assert.That(rw.Xp, Is.EqualTo(40 * 5 + 200 + 150));
            Assert.That(rw.Components, Is.EqualTo(2));
            Assert.That(rl.Credits, Is.EqualTo(120), "40% of the base on a loss");
        }

        [Test]
        public void PilotLevel_FollowsSquareCurve()
        {
            Assert.That(ProgressionRules.PilotLevel(0), Is.EqualTo(1));
            Assert.That(ProgressionRules.PilotLevel(99), Is.EqualTo(1));
            Assert.That(ProgressionRules.PilotLevel(100), Is.EqualTo(2));
            Assert.That(ProgressionRules.PilotLevel(400), Is.EqualTo(3));
            Assert.That(ProgressionRules.XpForLevel(3), Is.EqualTo(400));
        }

        [Test]
        public void BuyShip_RespectsCreditsAndLegendaryRule()
        {
            var save = SaveData.CreateDefault();
            save.credits = 2000;
            Assert.That(ProgressionRules.BuyShip(save, ShipId.Falcon), Is.False);
            save.credits = 2500;
            Assert.That(ProgressionRules.BuyShip(save, ShipId.Falcon), Is.True);
            Assert.That(save.IsShipUnlocked((int)ShipId.Falcon), Is.True);
            Assert.That(save.credits, Is.EqualTo(0));

            save.credits = 999999;
            Assert.That(ProgressionRules.BuyShip(save, ShipId.NovaX), Is.False, "Nova-X only unlocks by finishing the campaign");
            ProgressionRules.RecordCampaignStage(save, 4, 5, 1000);
            Assert.That(save.IsShipUnlocked((int)ShipId.NovaX), Is.True);
        }

        [Test]
        public void BuyWeapon_Works()
        {
            var save = SaveData.CreateDefault();
            save.credits = 1500;
            Assert.That(ProgressionRules.BuyWeapon(save, WeaponId.DoubleLaser), Is.True);
            Assert.That(ProgressionRules.BuyWeapon(save, WeaponId.DoubleLaser), Is.False, "already owned");
            Assert.That(save.IsWeaponUnlocked((int)WeaponId.Laser), Is.True, "starter weapon");
        }

        [Test]
        public void RecordCampaignStage_TracksBestScoresAndUnlocks()
        {
            var save = SaveData.CreateDefault();
            ProgressionRules.RecordCampaignStage(save, 0, 5, 5000);
            ProgressionRules.RecordCampaignStage(save, 0, 5, 3000);
            Assert.That(save.stageBestScores[0], Is.EqualTo(5000));
            Assert.That(save.unlockedStage, Is.EqualTo(1));
            Assert.That(save.IsStageCompleted(0), Is.True);
            Assert.That(save.IsStageCompleted(1), Is.False);
        }
    }

    public class AchievementTests
    {
        [Test]
        public void FirstKill_And_Survivor()
        {
            var save = SaveData.CreateDefault();
            var run = new RunStats { Mode = GameModeId.Campaign, Kills = 3, Completed = true, LivesLost = 0 };
            var earned = AchievementRules.Evaluate(save, run, 5, 4);
            Assert.That(earned, Has.Member(AchievementId.FirstKill));
            Assert.That(earned, Has.Member(AchievementId.Survivor));
            Assert.That(AchievementRules.Evaluate(save, run, 5, 4), Is.Empty, "never granted twice");
        }

        [Test]
        public void BossHunter_NeedsEveryBoss()
        {
            var save = SaveData.CreateDefault();
            save.MarkBossDefeated(0);
            save.MarkBossDefeated(1);
            Assert.That(AchievementRules.Evaluate(save, null, 5, 4), Has.No.Member(AchievementId.BossHunter));
            save.MarkBossDefeated(2);
            save.MarkBossDefeated(3);
            Assert.That(AchievementRules.Evaluate(save, null, 5, 4), Has.Member(AchievementId.BossHunter));
        }

        [Test]
        public void GalacticLegend_RequiresEverything()
        {
            var save = SaveData.CreateDefault();
            save.credits = int.MaxValue / 2;
            save.components = 1000;
            for (int i = 0; i < 5; i++) ProgressionRules.RecordCampaignStage(save, i, 5, 100);
            for (int i = 0; i < 4; i++) save.MarkBossDefeated(i);
            for (int s = 1; s < ProgressionRules.ShipCount; s++) ProgressionRules.BuyShip(save, (ShipId)s);
            for (int w = 1; w < ProgressionRules.WeaponCount; w++) ProgressionRules.BuyWeapon(save, (WeaponId)w);
            for (int n = 0; n < UpgradeCatalog.NodeCount; n++)
                while (UpgradeCatalog.Purchase(save, (UpgradeNode)n)) { }
            var run = new RunStats { Mode = GameModeId.Campaign, Kills = 1, Completed = true, LivesLost = 0 };

            var earned = AchievementRules.Evaluate(save, run, 5, 4);

            Assert.That(earned, Has.Member(AchievementId.Collector));
            Assert.That(earned, Has.Member(AchievementId.GalacticLegend));
        }
    }

    public class ProceduralWavesTests
    {
        [Test]
        public void SameSeedSameWave_IsDeterministic()
        {
            var a = ProceduralWaves.Generate(1234, 7);
            var b = ProceduralWaves.Generate(1234, 7);
            Assert.That(a.Count, Is.EqualTo(b.Count));
            for (int i = 0; i < a.Count; i++)
            {
                Assert.That(a[i].Enemy, Is.EqualTo(b[i].Enemy));
                Assert.That(a[i].Count, Is.EqualTo(b[i].Count));
                Assert.That(a[i].Pattern, Is.EqualTo(b[i].Pattern));
            }
        }

        [Test]
        public void Difficulty_RampsUp()
        {
            int Total(int wave) { int n = 0; foreach (var s in ProceduralWaves.Generate(42, wave)) n += s.Count; return n; }
            Assert.That(Total(20), Is.GreaterThan(Total(0)));
            Assert.That(ProceduralWaves.DifficultyMultiplier(10), Is.GreaterThan(ProceduralWaves.DifficultyMultiplier(0)));
        }

        [Test]
        public void EarlyWaves_OnlyBasicEnemies()
        {
            foreach (var s in ProceduralWaves.Generate(7, 0))
                Assert.That(s.Enemy, Is.EqualTo(ProceduralEnemy.Drone).Or.EqualTo(ProceduralEnemy.Interceptor));
        }

        [Test]
        public void DateSeed_IsStable()
        {
            Assert.That(ProceduralWaves.SeedFromDate(new System.DateTime(2026, 9, 5)), Is.EqualTo(20260905));
        }
    }

    public class LeaderboardTests
    {
        [Test]
        public void Insert_SortsAndTrims()
        {
            var list = new List<LeaderboardEntry>();
            for (int i = 0; i < 12; i++) Leaderboard.Insert(list, new LeaderboardEntry { score = i * 100 });
            Assert.That(list.Count, Is.EqualTo(SaveData.LeaderboardSize));
            Assert.That(list[0].score, Is.EqualTo(1100));
            Assert.That(list[9].score, Is.EqualTo(200));
            Assert.That(Leaderboard.Insert(list, new LeaderboardEntry { score = 1 }), Is.EqualTo(-1));
            Assert.That(Leaderboard.Insert(list, new LeaderboardEntry { score = 5000 }), Is.EqualTo(0));
        }
    }

    public class SaveMigrationTests
    {
        [Test]
        public void V1_DefaultsAreFilledOnMigration()
        {
            var old = new SaveData { version = 1, unlockedStage = 2, highScore = 900, upgradeLevels = null, stageBestScores = null, leaderboard = null };
            var data = SaveDataValidator.SanitizeAndMigrate(old, 5);
            Assert.That(data.version, Is.EqualTo(SaveData.CurrentVersion));
            Assert.That(data.unlockedStage, Is.EqualTo(2));
            Assert.That(data.upgradeLevels.Length, Is.EqualTo(SaveData.UpgradeNodeCount));
            Assert.That(data.IsShipUnlocked(0), Is.True);
            Assert.That(data.IsWeaponUnlocked(0), Is.True);
            Assert.That(data.leaderboard, Is.Not.Null);
        }

        [Test]
        public void SelectedShip_FallsBackWhenLocked()
        {
            var data = SaveDataValidator.SanitizeAndMigrate(new SaveData { selectedShip = 3, selectedWeapon = 4 }, 5);
            Assert.That(data.selectedShip, Is.EqualTo(0));
            Assert.That(data.selectedWeapon, Is.EqualTo(0));
        }
    }
}
