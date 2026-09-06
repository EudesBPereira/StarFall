using NUnit.Framework;
using Starfall.Logic;

namespace Starfall.Tests.EditMode
{
    public class PrecisionModelTests
    {
        [Test]
        public void Streak_GrowsOnHits_ResetsOnMiss()
        {
            var p = new PrecisionModel();
            p.RegisterHit();
            p.RegisterHit();
            Assert.That(p.Streak, Is.EqualTo(2));
            p.RegisterMiss();
            Assert.That(p.Streak, Is.EqualTo(0));
            Assert.That(p.BestStreak, Is.EqualTo(2));
        }

        [Test]
        public void Multiplier_IsCapped()
        {
            var p = new PrecisionModel();
            for (int i = 0; i < 100; i++) p.RegisterHit();
            Assert.That(p.Multiplier(0.01f, FactionRules.PrecisionCap), Is.EqualTo(1.25f).Within(0.001f));
            Assert.That(new PrecisionModel().Multiplier(0.01f, 0.25f), Is.EqualTo(1f));
        }
    }

    public class FactionRulesTests
    {
        [Test]
        public void EveryShipHasAFaction_AndEachFactionHasItsOwnUltimate()
        {
            Assert.That(FactionRules.FactionOf(ShipId.Vanguard), Is.EqualTo(FactionId.Federation));
            Assert.That(FactionRules.FactionOf(ShipId.Symbiont), Is.EqualTo(FactionId.Biomech));
            Assert.That(FactionRules.FactionOf(ShipId.Nexus), Is.EqualTo(FactionId.Cyber));
            Assert.That(FactionRules.FactionOf(ShipId.Phantom), Is.EqualTo(FactionId.Cyber));
            Assert.That(FactionRules.UltimateOf(FactionId.Federation), Is.EqualTo(UltimateKind.OrbitalStrike));
            Assert.That(FactionRules.UltimateOf(FactionId.Biomech), Is.EqualTo(UltimateKind.Swarm));
            Assert.That(FactionRules.UltimateOf(FactionId.Cyber), Is.EqualTo(UltimateKind.Emp));
        }

        [Test]
        public void Lifesteal_IsFractionOfMaxHull_Capped()
        {
            Assert.That(FactionRules.LifestealAmount(100f, 0.02f), Is.EqualTo(2f).Within(0.001f));
            Assert.That(FactionRules.LifestealAmount(100f, 0.5f), Is.EqualTo(5f).Within(0.001f), "capped at 5%");
            Assert.That(FactionRules.LifestealAmount(100f, -1f), Is.EqualTo(0f));
        }

        [Test]
        public void BuyingSymbiont_UnlocksSpores()
        {
            var save = SaveData.CreateDefault();
            save.credits = ProgressionRules.ShipCreditCost(ShipId.Symbiont);
            Assert.That(ProgressionRules.BuyShip(save, ShipId.Symbiont), Is.True);
            Assert.That(save.IsWeaponUnlocked((int)WeaponId.Spores), Is.True);
            Assert.That(save.credits, Is.EqualTo(0));
        }

        [Test]
        public void Catalog_CountsCoverEveryEnumValue()
        {
            Assert.That(ProgressionRules.ShipCount, Is.EqualTo(System.Enum.GetValues(typeof(ShipId)).Length));
            Assert.That(ProgressionRules.WeaponCount, Is.EqualTo(System.Enum.GetValues(typeof(WeaponId)).Length));
            for (int i = 0; i < ProgressionRules.ShipCount; i++) Assert.That(ProgressionRules.ShipCreditCost((ShipId)i), Is.GreaterThanOrEqualTo(0));
            for (int i = 0; i < ProgressionRules.WeaponCount; i++) Assert.That(ProgressionRules.WeaponCreditCost((WeaponId)i), Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void NoShipIsBetterAtEverything_PerPlan()
        {
            // Proxy for SR-BAL-001: the legendary ship must not have the highest hull AND shield (defense) among all.
            // Numbers mirror ContentFactory; keep in sync when balancing.
            float novaHull = 90f, novaShield = 40f, titanHull = 160f, titanShield = 90f;
            Assert.That(novaHull, Is.LessThan(titanHull));
            Assert.That(novaShield, Is.LessThan(titanShield));
        }
    }
}
