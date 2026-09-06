using NUnit.Framework;
using Starfall.Logic;

namespace Starfall.Tests.EditMode
{
    public class StageResultRulesTests
    {
        private static readonly RankThresholds Thresholds = RankThresholds.FromTarget(20000);

        [Test]
        public void Thresholds_AreMonotonic()
        {
            Assert.That(Thresholds.C, Is.LessThan(Thresholds.B));
            Assert.That(Thresholds.B, Is.LessThan(Thresholds.A));
            Assert.That(Thresholds.A, Is.LessThan(Thresholds.S));
            Assert.That(Thresholds.S, Is.LessThan(Thresholds.SS));
            Assert.That(Thresholds.SS, Is.LessThan(Thresholds.SSS));
        }

        [Test]
        public void Rank_FollowsThresholds()
        {
            Assert.That(StageResultRules.RankFor(0, Thresholds), Is.EqualTo(StageRank.D));
            Assert.That(StageResultRules.RankFor(Thresholds.C, Thresholds), Is.EqualTo(StageRank.C));
            Assert.That(StageResultRules.RankFor(Thresholds.S, Thresholds), Is.EqualTo(StageRank.S));
            Assert.That(StageResultRules.RankFor(Thresholds.SSS + 1, Thresholds), Is.EqualTo(StageRank.SSS));
        }

        [Test]
        public void TimeBonus_OnlyUnderPar_AndCapped()
        {
            Assert.That(StageResultRules.TimeBonus(200f, 180f), Is.EqualTo(0));
            Assert.That(StageResultRules.TimeBonus(150f, 180f), Is.EqualTo(1200));
            Assert.That(StageResultRules.TimeBonus(0f, 100000f), Is.EqualTo(StageResultRules.TimeBonusCap));
        }

        [Test]
        public void Compose_AddsBonusesOnlyWhenCompleted()
        {
            var score = new ScoreModel();
            score.RegisterKill(100, 3f); // 300
            score.RegisterGraze(1f);     // +50
            var win = StageResultRules.Compose(score, true, 0, 100f, 180f, 1000, Thresholds);
            var loss = StageResultRules.Compose(score, false, 0, 100f, 180f, 1000, Thresholds);

            Assert.That(win.KillPoints, Is.EqualTo(300));
            Assert.That(win.GrazePoints, Is.EqualTo(50));
            Assert.That(win.NoDamageBonus, Is.EqualTo(StageResultRules.NoDamageBonusPoints));
            Assert.That(win.TimeBonus, Is.EqualTo(3200));
            Assert.That(win.ObjectiveBonus, Is.EqualTo(1000));
            Assert.That(win.Total, Is.EqualTo(300 + 50 + 5000 + 3200 + 1000));
            Assert.That(loss.Total, Is.EqualTo(350));
            Assert.That(loss.Rank, Is.EqualTo(StageRank.D));
        }

        [Test]
        public void Compose_NoDamageBonusRequiresZeroHits()
        {
            var score = new ScoreModel();
            var r = StageResultRules.Compose(score, true, 1, 100f, 180f, 1000, Thresholds);
            Assert.That(r.NoDamageBonus, Is.EqualTo(0));
        }
    }

    public class EconomySeparationTests
    {
        [Test]
        public void Credits_DoNotScaleWithScore()
        {
            var low = new RunStats { Mode = GameModeId.Campaign, Score = 1000, Completed = true, Rank = StageRank.B };
            var high = new RunStats { Mode = GameModeId.Campaign, Score = 1000000, Completed = true, Rank = StageRank.B };
            var a = ProgressionRules.ComputeRewards(low, 300, false);
            var b = ProgressionRules.ComputeRewards(high, 300, false);
            Assert.That(a.Credits, Is.EqualTo(b.Credits), "score must not convert into credits (plan §11.4)");
        }

        [Test]
        public void Credits_ComeFromBaseRankRiskAndFirstClear()
        {
            var stats = new RunStats { Mode = GameModeId.Campaign, Completed = true, Rank = StageRank.S, SecondsInDanger = 30f };
            var r = ProgressionRules.ComputeRewards(stats, 300, true);
            Assert.That(r.BaseCredits, Is.EqualTo(300));
            Assert.That(r.RankCredits, Is.EqualTo(350));
            Assert.That(r.RiskCredits, Is.EqualTo(60));
            Assert.That(r.FirstClearCredits, Is.EqualTo(300));
            Assert.That(r.Credits, Is.EqualTo(300 + 350 + 60 + 300));
        }

        [Test]
        public void RiskBonus_IsCapped()
        {
            var stats = new RunStats { Mode = GameModeId.Campaign, Completed = true, SecondsInDanger = 100000f };
            var r = ProgressionRules.ComputeRewards(stats, 300, false);
            Assert.That(r.RiskCredits, Is.EqualTo(ProgressionRules.RiskCreditCap));
        }

        [Test]
        public void Loss_PaysPartialBase_NoRankOrFirstClear()
        {
            var stats = new RunStats { Mode = GameModeId.Campaign, Completed = false, Rank = StageRank.S };
            var r = ProgressionRules.ComputeRewards(stats, 300, true);
            Assert.That(r.BaseCredits, Is.EqualTo(120));
            Assert.That(r.RankCredits, Is.EqualTo(0));
            Assert.That(r.FirstClearCredits, Is.EqualTo(0));
        }

        [Test]
        public void Revive_HalvesRankCredits()
        {
            var clean = new RunStats { Mode = GameModeId.Campaign, Completed = true, Rank = StageRank.SSS };
            var revived = new RunStats { Mode = GameModeId.Campaign, Completed = true, Rank = StageRank.SSS, UsedRevive = true };
            Assert.That(ProgressionRules.ComputeRewards(revived, 0, false).RankCredits,
                Is.EqualTo(ProgressionRules.ComputeRewards(clean, 0, false).RankCredits / 2));
        }

        [Test]
        public void Endless_PaysPerWave_Capped()
        {
            var s = new RunStats { Mode = GameModeId.Survival, WavesSurvived = 10 };
            Assert.That(ProgressionRules.ComputeRewards(s, 0, false).BaseCredits, Is.EqualTo(120));
            s.WavesSurvived = 1000;
            Assert.That(ProgressionRules.ComputeRewards(s, 0, false).BaseCredits, Is.EqualTo(ProgressionRules.EndlessCreditsCap));
        }

        [Test]
        public void Progression_SimulatedCampaign_UnlocksFirstShipWithinReasonableRuns()
        {
            // Rough simulation (plan SR-ECO-001): clearing stages 1..5 once each at rank A with first-clear bonuses.
            var save = SaveData.CreateDefault();
            int[] baseCredits = { 300, 380, 460, 560, 700 };
            for (int i = 0; i < 5; i++)
            {
                var stats = new RunStats { Mode = GameModeId.Campaign, StageIndex = i, Completed = true, Rank = StageRank.A, SecondsInDanger = 40f, Kills = 40 };
                var rewards = ProgressionRules.ComputeRewards(stats, baseCredits[i], true);
                ProgressionRules.ApplyRewards(save, rewards);
            }
            Assert.That(save.credits, Is.GreaterThanOrEqualTo(ProgressionRules.ShipCreditCost(ShipId.Falcon)), "one full campaign should afford the first ship");
            Assert.That(save.credits, Is.LessThan(ProgressionRules.ShipCreditCost(ShipId.Falcon) + ProgressionRules.ShipCreditCost(ShipId.Titan) + ProgressionRules.ShipCreditCost(ShipId.Phantom)),
                "but not everything at once");
        }

        [Test]
        public void RecordCampaignStage_KeepsBestRank()
        {
            var save = SaveData.CreateDefault();
            ProgressionRules.RecordCampaignStage(save, 0, 5, 1000, StageRank.S);
            ProgressionRules.RecordCampaignStage(save, 0, 5, 500, StageRank.B);
            Assert.That(save.stageBestRanks[0], Is.EqualTo((int)StageRank.S));
        }
    }
}
