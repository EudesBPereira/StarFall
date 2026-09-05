using NUnit.Framework;
using Starfall.Logic;

namespace Starfall.Tests.EditMode
{
    public class LivesModelTests
    {
        [Test]
        public void LoseLife_ReturnsTrueWhileLivesRemain()
        {
            var lives = new LivesModel(3);

            Assert.That(lives.LoseLife(), Is.True);   // 2 left
            Assert.That(lives.LoseLife(), Is.True);   // 1 left
            Assert.That(lives.LoseLife(), Is.False);  // 0 left -> game over
            Assert.That(lives.Lives, Is.EqualTo(0));
            Assert.That(lives.LoseLife(), Is.False);
            Assert.That(lives.Lives, Is.EqualTo(0));
        }

        [Test]
        public void AddLife_ClampsToMax()
        {
            var lives = new LivesModel(3, maxLives: 5);
            lives.AddLife(10);
            Assert.That(lives.Lives, Is.EqualTo(5));
        }
    }

    public class EnergyModelTests
    {
        [Test]
        public void Ultimate_OnlyWhenFull()
        {
            var energy = new EnergyModel(100f);

            energy.Add(99f);
            Assert.That(energy.TryConsumeAll(), Is.False);
            Assert.That(energy.Current, Is.EqualTo(99f));

            energy.Add(1f);
            Assert.That(energy.IsFull, Is.True);
            Assert.That(energy.TryConsumeAll(), Is.True);
            Assert.That(energy.Current, Is.EqualTo(0f));
        }

        [Test]
        public void Add_ClampsAndFiresBecameFullOnce()
        {
            var energy = new EnergyModel(50f);
            int full = 0;
            energy.BecameFull += () => full++;

            energy.Add(40f);
            energy.Add(40f);
            energy.Add(40f);

            Assert.That(energy.Current, Is.EqualTo(50f));
            Assert.That(full, Is.EqualTo(1));
        }
    }
}
