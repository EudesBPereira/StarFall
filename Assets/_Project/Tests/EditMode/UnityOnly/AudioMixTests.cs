using NUnit.Framework;
using Starfall.Audio;

namespace Starfall.Tests.EditMode
{
    public sealed class AudioMixTests
    {
        [Test]
        public void EverySfxHasAGainInRange()
        {
            foreach (SfxId id in System.Enum.GetValues(typeof(SfxId)))
            {
                float v = AudioMix.SfxVolume(id);
                Assert.That(v, Is.GreaterThan(0f).And.LessThanOrEqualTo(1f), id.ToString());
            }
        }

        [Test]
        public void FrequentSoundsSitUnderMusicAndKeyEventsPunchThrough()
        {
            Assert.That(AudioMix.SfxVolume(SfxId.Laser), Is.LessThan(0.4f));
            Assert.That(AudioMix.SfxVolume(SfxId.EnemyShot), Is.LessThan(0.4f));
            Assert.That(AudioMix.SfxVolume(SfxId.ExplosionLarge), Is.GreaterThan(0.7f));
            Assert.That(AudioMix.SfxVolume(SfxId.PlayerHit), Is.GreaterThan(0.7f));
            Assert.That(AudioMix.MusicVolume(MusicId.Menu), Is.EqualTo(1f));
        }
    }
}
