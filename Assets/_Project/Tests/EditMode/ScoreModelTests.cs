using NUnit.Framework;
using Starfall.Logic;

namespace Starfall.Tests.EditMode
{
    public class ScoreModelTests
    {
        [Test]
        public void Kill_AwardsBaseTimesMultiplier()
        {
            var score = new ScoreModel(maxMultiplier: 10, killsPerStep: 1);

            int first = score.RegisterKill(100);   // x1 -> 100, multiplier becomes 2
            int second = score.RegisterKill(100);  // x2 -> 200

            Assert.That(first, Is.EqualTo(100));
            Assert.That(second, Is.EqualTo(200));
            Assert.That(score.Score, Is.EqualTo(300));
        }

        [Test]
        public void Multiplier_IsCappedAtMax()
        {
            var score = new ScoreModel(maxMultiplier: 10, killsPerStep: 1);

            for (int i = 0; i < 50; i++) score.RegisterKill(100);

            Assert.That(score.Multiplier, Is.EqualTo(10));
            Assert.That(score.HighestMultiplier, Is.EqualTo(10));
        }

        [Test]
        public void Multiplier_RequiresKillsPerStep()
        {
            var score = new ScoreModel(maxMultiplier: 10, killsPerStep: 4);

            score.RegisterKill(100);
            score.RegisterKill(100);
            score.RegisterKill(100);
            Assert.That(score.Multiplier, Is.EqualTo(1));

            score.RegisterKill(100);
            Assert.That(score.Multiplier, Is.EqualTo(2));
        }

        [Test]
        public void Damage_ResetsMultiplierToOne()
        {
            var score = new ScoreModel(10, 1);
            for (int i = 0; i < 5; i++) score.RegisterKill(100);
            Assert.That(score.Multiplier, Is.GreaterThan(1));

            score.RegisterPlayerDamaged();

            Assert.That(score.Multiplier, Is.EqualTo(1));
            Assert.That(score.KillStreak, Is.EqualTo(0));
            Assert.That(score.DamageTakenCount, Is.EqualTo(1));
        }

        [Test]
        public void Events_FireOnChange()
        {
            var score = new ScoreModel(10, 1);
            int scoreEvents = 0, multEvents = 0;
            score.ScoreChanged += _ => scoreEvents++;
            score.MultiplierChanged += _ => multEvents++;

            score.RegisterKill(100);
            score.RegisterPlayerDamaged();

            Assert.That(scoreEvents, Is.EqualTo(1));
            Assert.That(multEvents, Is.EqualTo(2));
        }

        [Test]
        public void Bonus_DoesNotTouchMultiplier()
        {
            var score = new ScoreModel(10, 1);
            score.AddBonus(500);
            Assert.That(score.Score, Is.EqualTo(500));
            Assert.That(score.Multiplier, Is.EqualTo(1));
            Assert.That(score.EnemiesDestroyed, Is.EqualTo(0));
        }
    }
}
