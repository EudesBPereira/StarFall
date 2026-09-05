using NUnit.Framework;
using Starfall.Logic;

namespace Starfall.Tests.EditMode
{
    public class CriticalHitTests
    {
        [Test]
        public void Critical_MultipliesWhenRollUnderChance()
        {
            float dmg = DamageInfo.ApplyCritical(10f, 0.25f, 2f, 0.1f, out bool crit);
            Assert.That(crit, Is.True);
            Assert.That(dmg, Is.EqualTo(20f));
        }

        [Test]
        public void Critical_KeepsDamageWhenRollAboveChance()
        {
            float dmg = DamageInfo.ApplyCritical(10f, 0.25f, 2f, 0.9f, out bool crit);
            Assert.That(crit, Is.False);
            Assert.That(dmg, Is.EqualTo(10f));
        }

        [Test]
        public void Critical_ZeroChanceNeverCrits()
        {
            DamageInfo.ApplyCritical(10f, 0f, 5f, 0f, out bool crit);
            Assert.That(crit, Is.False);
        }
    }

    public class LoadoutTests
    {
        [Test]
        public void Modifiers_ScaleShipStats()
        {
            var levels = new int[UpgradeCatalog.NodeCount];
            levels[(int)UpgradeNode.HullMax] = 5;      // +50%
            levels[(int)UpgradeNode.ShieldCapacity] = 5; // +60%
            var mods = UpgradeCatalog.Compute(levels);
            Assert.That(100f * mods.HullMultiplier, Is.EqualTo(150f).Within(0.01f));
            Assert.That(50f * mods.ShieldMultiplier, Is.EqualTo(80f).Within(0.01f));
        }

        [Test]
        public void Identity_LeavesStatsUnchanged()
        {
            var m = LoadoutModifiers.Identity;
            Assert.That(m.DamageMultiplier, Is.EqualTo(1f));
            Assert.That(m.ShieldRegenPerSecondBonus, Is.EqualTo(0f));
        }
    }

    public class StatusEffectDebuffTests
    {
        [Test]
        public void SlowedEffect_ExpiresLikeAnyOther()
        {
            var tracker = new StatusEffectTracker();
            tracker.Apply(PowerUpKind.Slowed, 2f);
            Assert.That(tracker.IsActive(PowerUpKind.Slowed), Is.True);
            tracker.Tick(1.5f);
            Assert.That(tracker.IsActive(PowerUpKind.Slowed), Is.True);
            tracker.Tick(0.6f);
            Assert.That(tracker.IsActive(PowerUpKind.Slowed), Is.False);
        }

        [Test]
        public void Longest_ReturnsEffectWithMostTimeLeft()
        {
            var tracker = new StatusEffectTracker();
            tracker.Apply(PowerUpKind.SpeedBoost, 3f);
            tracker.Apply(PowerUpKind.DamageBoost, 8f);
            var longest = tracker.Longest(out float remaining);
            Assert.That(longest, Is.EqualTo(PowerUpKind.DamageBoost));
            Assert.That(remaining, Is.EqualTo(8f).Within(0.001f));
        }
    }

    public class GameModeRulesTests
    {
        [Test]
        public void DailySeed_IsSameForSameDay_DifferentAcrossDays()
        {
            int a = ProceduralWaves.SeedFromDate(new System.DateTime(2026, 9, 5));
            int b = ProceduralWaves.SeedFromDate(new System.DateTime(2026, 9, 5));
            int c = ProceduralWaves.SeedFromDate(new System.DateTime(2026, 9, 6));
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a, Is.Not.EqualTo(c));
        }

        [Test]
        public void SurvivalWaves_AlwaysProduceSpawns()
        {
            for (int wave = 0; wave < 30; wave++)
            {
                var spawns = ProceduralWaves.Generate(99, wave);
                Assert.That(spawns.Count, Is.GreaterThan(0), $"wave {wave}");
                foreach (var s in spawns)
                {
                    Assert.That(s.Count, Is.GreaterThan(0));
                    Assert.That(s.Interval, Is.GreaterThan(0f).Or.EqualTo(0f));
                    Assert.That(s.Pattern, Is.InRange(0, ProceduralWaves.PatternCount - 1));
                }
            }
        }

        [Test]
        public void SupplyDrone_AppearsOnEveryFifthWave()
        {
            bool found = false;
            foreach (var s in ProceduralWaves.Generate(5, 5))
                if (s.Enemy == ProceduralEnemy.Supply) found = true;
            Assert.That(found, Is.True);
        }
    }
}
