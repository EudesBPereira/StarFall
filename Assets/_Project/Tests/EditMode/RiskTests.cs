using System.Collections.Generic;
using NUnit.Framework;
using Starfall.Logic;

namespace Starfall.Tests.EditMode
{
    public class RiskModelTests
    {
        private static RiskModel Settle(RiskModel m, float threat, float seconds)
        {
            for (float t = 0f; t < seconds; t += 0.1f) m.Tick(threat, 0.1f);
            return m;
        }

        [Test]
        public void StartsSafe_WithMultiplierOne()
        {
            var m = new RiskModel();
            Assert.That(m.State, Is.EqualTo(RiskState.Safe));
            Assert.That(m.Multiplier, Is.EqualTo(1f));
        }

        [Test]
        public void Threat_RaisesThroughEveryState()
        {
            var m = new RiskModel();
            Settle(m, 0.3f, 2f);
            Assert.That(m.State, Is.EqualTo(RiskState.Alert));
            Assert.That(m.Multiplier, Is.EqualTo(2f));
            Settle(m, 0.8f, 2f);
            Assert.That(m.State, Is.EqualTo(RiskState.Danger));
            Assert.That(m.Multiplier, Is.EqualTo(3f));
            Settle(m, 1.5f, 2f);
            Assert.That(m.State, Is.EqualTo(RiskState.Extreme));
            Assert.That(m.Multiplier, Is.EqualTo(5f));
        }

        [Test]
        public void Level_FallsSlowerThanItRises()
        {
            var m = new RiskModel();
            Settle(m, 1.5f, 1f);
            float peak = m.Level;
            m.Tick(0f, 0.5f);
            float afterHalfSecond = m.Level;
            Assert.That(peak - afterHalfSecond, Is.LessThan(peak * 0.6f), "hysteresis: the level should not collapse immediately");
        }

        [Test]
        public void State_DoesNotDropBeforeMinimumHold()
        {
            var m = new RiskModel();
            Settle(m, 1.5f, 2f);
            Assert.That(m.State, Is.EqualTo(RiskState.Extreme));
            m.Tick(0f, 0.1f); // below threshold now, but the state must hold for a moment
            Assert.That(m.State, Is.EqualTo(RiskState.Extreme));
        }

        [Test]
        public void StateChanged_FiresOncePerTransition()
        {
            var m = new RiskModel();
            var transitions = new List<(RiskState, RiskState)>();
            m.StateChanged += (a, b) => transitions.Add((a, b));
            Settle(m, 1.5f, 3f);
            Assert.That(transitions.Count, Is.EqualTo(3));
            Assert.That(transitions[0], Is.EqualTo((RiskState.Safe, RiskState.Alert)));
            Assert.That(transitions[2], Is.EqualTo((RiskState.Danger, RiskState.Extreme)));
        }

        [Test]
        public void Overdrive_ScalesMultiplierUpToCap()
        {
            var m = new RiskModel();
            Settle(m, 1.5f, 2f);
            m.OverdriveActive = true;
            Assert.That(m.Multiplier, Is.EqualTo(8f));
            m.Reset();
            Settle(m, 0.3f, 2f);
            m.OverdriveActive = true;
            Assert.That(m.Multiplier, Is.EqualTo(3.2f).Within(0.001f));
        }

        [Test]
        public void TimeInDanger_AccumulatesOnlyInDangerOrAbove()
        {
            var m = new RiskModel();
            Settle(m, 0.3f, 2f);
            Assert.That(m.TimeInDanger, Is.EqualTo(0f));
            Settle(m, 0.8f, 2f);
            Assert.That(m.TimeInDanger, Is.GreaterThan(1f));
        }

        [Test]
        public void ThreatContribution_IsZeroOutsideRadius_AndGrowsWithCloseness()
        {
            Assert.That(RiskModel.ThreatContribution(5f, 4f, 1f), Is.EqualTo(0f));
            Assert.That(RiskModel.ThreatContribution(0f, 4f, 1f), Is.EqualTo(1f));
            Assert.That(RiskModel.ThreatContribution(2f, 4f, 1f), Is.EqualTo(0.25f).Within(0.001f));
            Assert.That(RiskModel.ThreatContribution(1f, 4f, 2f), Is.GreaterThan(RiskModel.ThreatContribution(1f, 4f, 1f)));
        }

        [Test]
        public void Reset_ReturnsToSafe()
        {
            var m = new RiskModel();
            Settle(m, 1.5f, 2f);
            m.Reset();
            Assert.That(m.State, Is.EqualTo(RiskState.Safe));
            Assert.That(m.Level, Is.EqualTo(0f));
        }
    }

    public class OverdriveModelTests
    {
        [Test]
        public void ChargesInDanger_FasterInExtreme()
        {
            var a = new OverdriveModel();
            var b = new OverdriveModel();
            a.Tick(RiskState.Danger, 1f);
            b.Tick(RiskState.Extreme, 1f);
            Assert.That(a.Meter, Is.GreaterThan(0f));
            Assert.That(b.Meter, Is.GreaterThan(a.Meter));
        }

        [Test]
        public void DrainsWhenSafe_HoldsOnAlert()
        {
            var m = new OverdriveModel();
            m.Tick(RiskState.Danger, 2f);
            float charged = m.Meter;
            m.Tick(RiskState.Alert, 2f);
            Assert.That(m.Meter, Is.EqualTo(charged));
            m.Tick(RiskState.Safe, 1f);
            Assert.That(m.Meter, Is.LessThan(charged));
        }

        [Test]
        public void ActivatesWhenFull_ThenDrainsAndEnds()
        {
            var m = new OverdriveModel();
            bool activated = false, ended = false;
            m.Activated += () => activated = true;
            m.Ended += () => ended = true;
            for (int i = 0; i < 30 && !m.IsActive; i++) m.Tick(RiskState.Extreme, 1f);
            Assert.That(activated, Is.True);
            Assert.That(m.IsActive, Is.True);
            Assert.That(m.FireRateMultiplier, Is.GreaterThan(1f));
            m.Tick(RiskState.Extreme, 100f);
            Assert.That(ended, Is.True);
            Assert.That(m.IsActive, Is.False);
            Assert.That(m.Meter, Is.EqualTo(0f), "meter starts empty after an activation");
        }

        [Test]
        public void GrazeAndKills_AddCharge()
        {
            var m = new OverdriveModel();
            m.OnGraze();
            float afterGraze = m.Meter;
            m.OnCloseKill();
            m.OnComboKill();
            m.OnBossPartDestroyed();
            Assert.That(afterGraze, Is.GreaterThan(0f));
            Assert.That(m.Meter, Is.GreaterThan(afterGraze));
        }

        [Test]
        public void Damage_LosesMeterWhenCharging_AndTimeWhenActive()
        {
            var m = new OverdriveModel();
            m.Tick(RiskState.Extreme, 3f);
            float before = m.Meter;
            m.OnDamaged();
            Assert.That(m.Meter, Is.LessThan(before));

            for (int i = 0; i < 30 && !m.IsActive; i++) m.Tick(RiskState.Extreme, 1f);
            float remaining = m.RemainingSeconds;
            m.OnDamaged();
            Assert.That(m.RemainingSeconds, Is.LessThan(remaining));
        }

        [Test]
        public void Death_EndsAndEmpties()
        {
            var m = new OverdriveModel();
            for (int i = 0; i < 30 && !m.IsActive; i++) m.Tick(RiskState.Extreme, 1f);
            m.OnDeath();
            Assert.That(m.IsActive, Is.False);
            Assert.That(m.Meter, Is.EqualTo(0f));
        }

        [Test]
        public void NeverExceedsMax_WhileCharging()
        {
            var settings = OverdriveSettings.Default;
            settings.Max = 10f;
            settings.ExtremeGainPerSecond = 3f;
            var m = new OverdriveModel(settings);
            m.Tick(RiskState.Extreme, 1f);
            m.Tick(RiskState.Extreme, 1f);
            Assert.That(m.Meter, Is.LessThanOrEqualTo(10f));
        }
    }

    public class GrazeRulesTests
    {
        [Test]
        public void Ring_IsOutsideHitboxAndInsideMargin()
        {
            Assert.That(GrazeRules.IsInGrazeRing(0.2f, 0.28f, 0.1f), Is.False, "inside hitbox = hit, not graze");
            Assert.That(GrazeRules.IsInGrazeRing(0.5f, 0.28f, 0.1f), Is.True);
            Assert.That(GrazeRules.IsInGrazeRing(1.5f, 0.28f, 0.1f), Is.False, "too far");
        }

        [Test]
        public void CanAward_RejectsInvulnerableRepeatedAndSlow()
        {
            Assert.That(GrazeRules.CanAward(false, false, 6f), Is.True);
            Assert.That(GrazeRules.CanAward(true, false, 6f), Is.False, "invulnerable");
            Assert.That(GrazeRules.CanAward(false, true, 6f), Is.False, "already grazed");
            Assert.That(GrazeRules.CanAward(false, false, 1f), Is.False, "too slow");
        }

        [Test]
        public void Points_ScaleWithRisk()
        {
            Assert.That(GrazeRules.Points(1f), Is.EqualTo(50));
            Assert.That(GrazeRules.Points(5f), Is.EqualTo(250));
            Assert.That(GrazeRules.Points(0.5f), Is.EqualTo(50), "never below base");
        }
    }

    public class ScoreRiskIntegrationTests
    {
        [Test]
        public void Kill_MultipliesByComboAndRisk()
        {
            var s = new ScoreModel(10, 4);
            int awarded = s.RegisterKill(100, 3f);
            Assert.That(awarded, Is.EqualTo(300));
            for (int i = 0; i < 3; i++) s.RegisterKill(100, 1f); // reach combo x2
            Assert.That(s.Multiplier, Is.EqualTo(2));
            Assert.That(s.RegisterKill(100, 5f), Is.EqualTo(1000));
            Assert.That(s.HighestRiskMultiplier, Is.EqualTo(5f));
        }

        [Test]
        public void Graze_AddsPointsWithoutTouchingCombo()
        {
            var s = new ScoreModel(10, 4);
            s.RegisterGraze(2f);
            Assert.That(s.Score, Is.EqualTo(100));
            Assert.That(s.Grazes, Is.EqualTo(1));
            Assert.That(s.Multiplier, Is.EqualTo(1));
            Assert.That(s.KillStreak, Is.EqualTo(0));
        }
    }
}
