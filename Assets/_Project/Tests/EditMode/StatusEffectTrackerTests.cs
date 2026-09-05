using System.Collections.Generic;
using NUnit.Framework;
using Starfall.Logic;

namespace Starfall.Tests.EditMode
{
    public class StatusEffectTrackerTests
    {
        [Test]
        public void Effect_ExpiresAfterDuration()
        {
            var tracker = new StatusEffectTracker();
            var expired = new List<PowerUpKind>();
            tracker.Expired += k => expired.Add(k);

            tracker.Apply(PowerUpKind.SpeedBoost, 2f);
            tracker.Tick(1f);
            Assert.That(tracker.IsActive(PowerUpKind.SpeedBoost), Is.True);
            Assert.That(expired, Is.Empty);

            tracker.Tick(1.01f);
            Assert.That(tracker.IsActive(PowerUpKind.SpeedBoost), Is.False);
            Assert.That(expired, Is.EqualTo(new[] { PowerUpKind.SpeedBoost }));
        }

        [Test]
        public void Reapply_RenewsDurationWithoutStacking()
        {
            var tracker = new StatusEffectTracker();
            tracker.Apply(PowerUpKind.DamageBoost, 5f);
            tracker.Tick(4f);

            tracker.Apply(PowerUpKind.DamageBoost, 5f);

            Assert.That(tracker.Remaining(PowerUpKind.DamageBoost), Is.EqualTo(5f));
            Assert.That(tracker.ActiveCount, Is.EqualTo(1));
        }

        [Test]
        public void Reapply_WithShorterDurationKeepsLonger()
        {
            var tracker = new StatusEffectTracker();
            tracker.Apply(PowerUpKind.Invincibility, 8f);
            tracker.Apply(PowerUpKind.Invincibility, 3f);
            Assert.That(tracker.Remaining(PowerUpKind.Invincibility), Is.EqualTo(8f));
        }

        [Test]
        public void ClearAll_FiresExpiredForEach()
        {
            var tracker = new StatusEffectTracker();
            int expired = 0;
            tracker.Expired += _ => expired++;
            tracker.Apply(PowerUpKind.SpeedBoost, 3f);
            tracker.Apply(PowerUpKind.DamageBoost, 3f);

            tracker.ClearAll();

            Assert.That(expired, Is.EqualTo(2));
            Assert.That(tracker.ActiveCount, Is.EqualTo(0));
        }

        [Test]
        public void Longest_ReturnsEffectWithMostTime()
        {
            var tracker = new StatusEffectTracker();
            tracker.Apply(PowerUpKind.SpeedBoost, 3f);
            tracker.Apply(PowerUpKind.DamageBoost, 6f);

            var kind = tracker.Longest(out float remaining);

            Assert.That(kind, Is.EqualTo(PowerUpKind.DamageBoost));
            Assert.That(remaining, Is.EqualTo(6f));
        }
    }
}
