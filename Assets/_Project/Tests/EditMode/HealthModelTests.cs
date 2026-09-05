using NUnit.Framework;
using Starfall.Logic;

namespace Starfall.Tests.EditMode
{
    public class HealthModelTests
    {
        [Test]
        public void Damage_HitsShieldBeforeHull()
        {
            var health = new HealthModel(maxHull: 100f, maxShield: 50f);

            var result = health.TakeDamage(new DamageInfo(30f, DamageSource.Enemy));

            Assert.That(result.Applied, Is.True);
            Assert.That(result.ShieldDamage, Is.EqualTo(30f));
            Assert.That(result.HullDamage, Is.EqualTo(0f));
            Assert.That(health.Shield, Is.EqualTo(20f));
            Assert.That(health.Hull, Is.EqualTo(100f));
        }

        [Test]
        public void Damage_OverflowsFromShieldIntoHull()
        {
            var health = new HealthModel(100f, 20f);

            var result = health.TakeDamage(new DamageInfo(35f, DamageSource.Enemy));

            Assert.That(result.ShieldDamage, Is.EqualTo(20f));
            Assert.That(result.HullDamage, Is.EqualTo(15f));
            Assert.That(result.ShieldBroken, Is.True);
            Assert.That(health.Shield, Is.EqualTo(0f));
            Assert.That(health.Hull, Is.EqualTo(85f));
        }

        [Test]
        public void Damage_IgnoreShieldHitsHullDirectly()
        {
            var health = new HealthModel(100f, 50f);

            health.TakeDamage(new DamageInfo(10f, DamageSource.Enemy, ignoreShield: true));

            Assert.That(health.Shield, Is.EqualTo(50f));
            Assert.That(health.Hull, Is.EqualTo(90f));
        }

        [Test]
        public void Invulnerable_BlocksDamage()
        {
            var health = new HealthModel(100f, 0f) { Invulnerable = true };

            var result = health.TakeDamage(new DamageInfo(40f, DamageSource.Enemy));

            Assert.That(result.Applied, Is.False);
            Assert.That(result.BlockedByInvulnerability, Is.True);
            Assert.That(health.Hull, Is.EqualTo(100f));
        }

        [Test]
        public void Invulnerable_CanBeBypassed()
        {
            var health = new HealthModel(100f, 0f) { Invulnerable = true };

            health.TakeDamage(new DamageInfo(40f, DamageSource.Ultimate, ignoreInvulnerability: true));

            Assert.That(health.Hull, Is.EqualTo(60f));
        }

        [Test]
        public void Death_FiresExactlyOnce()
        {
            var health = new HealthModel(10f, 0f);
            int deaths = 0;
            health.Died += _ => deaths++;

            var first = health.TakeDamage(new DamageInfo(10f, DamageSource.Player));
            var second = health.TakeDamage(new DamageInfo(10f, DamageSource.Player));
            health.Kill(DamageSource.Player);

            Assert.That(first.Killed, Is.True);
            Assert.That(second.Applied, Is.False);
            Assert.That(deaths, Is.EqualTo(1));
            Assert.That(health.IsAlive, Is.False);
        }

        [Test]
        public void Kill_BypassesShieldAndInvulnerability()
        {
            var health = new HealthModel(10f, 100f) { Invulnerable = true };
            int deaths = 0;
            health.Died += _ => deaths++;

            health.Kill(DamageSource.Environment);

            Assert.That(deaths, Is.EqualTo(1));
            Assert.That(health.IsAlive, Is.False);
        }

        [Test]
        public void Restore_ClampsToMax()
        {
            var health = new HealthModel(100f, 50f);
            health.TakeDamage(new DamageInfo(70f, DamageSource.Enemy));

            health.RestoreShield(1000f);
            health.RestoreHull(1000f);

            Assert.That(health.Shield, Is.EqualTo(50f));
            Assert.That(health.Hull, Is.EqualTo(100f));
        }

        [Test]
        public void ResetToFull_RevivesEntity()
        {
            var health = new HealthModel(10f, 5f);
            health.Kill(DamageSource.Enemy);

            health.ResetToFull();

            Assert.That(health.IsAlive, Is.True);
            Assert.That(health.Hull, Is.EqualTo(10f));
            Assert.That(health.Shield, Is.EqualTo(5f));
        }
    }
}
