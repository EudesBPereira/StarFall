using System;
using NUnit.Framework;
using Starfall.Logic;

namespace Starfall.Tests.EditMode
{
    public class SaveRepositoryTests
    {
        private sealed class MemoryStorage : ISaveStorage
        {
            public string Content;
            public bool ThrowOnRead;
            public bool Exists() => Content != null;
            public string Read()
            {
                if (ThrowOnRead) throw new InvalidOperationException("disk error");
                return Content;
            }
            public void Write(string content) => Content = content;
            public void Delete() => Content = null;
        }

        /// <summary>Minimal serializer: "unlocked|highScore|master" or throws.</summary>
        private sealed class FakeSerializer : ISaveSerializer
        {
            public string Serialize(SaveData d) => $"{d.version}|{d.unlockedStage}|{d.highScore}|{d.masterVolume}";
            public SaveData Deserialize(string content)
            {
                var parts = content.Split('|');
                if (parts.Length != 4) throw new FormatException();
                return new SaveData
                {
                    version = int.Parse(parts[0]),
                    unlockedStage = int.Parse(parts[1]),
                    highScore = int.Parse(parts[2]),
                    masterVolume = float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture),
                };
            }
        }

        [Test]
        public void Load_WithoutSave_ReturnsDefaults()
        {
            var repo = new SaveRepository(new MemoryStorage(), new FakeSerializer(), 3);
            var data = repo.Load();

            Assert.That(data.unlockedStage, Is.EqualTo(0));
            Assert.That(data.highScore, Is.EqualTo(0));
            Assert.That(data.version, Is.EqualTo(SaveData.CurrentVersion));
            Assert.That(repo.LastLoadWasCorrupt, Is.False);
        }

        [Test]
        public void Load_WithCorruptContent_FallsBackToDefaults()
        {
            var storage = new MemoryStorage { Content = "garbage" };
            var repo = new SaveRepository(storage, new FakeSerializer(), 3);

            var data = repo.Load();

            Assert.That(data, Is.Not.Null);
            Assert.That(repo.LastLoadWasCorrupt, Is.True);
            Assert.That(data.unlockedStage, Is.EqualTo(0));
        }

        [Test]
        public void Load_WithStorageException_DoesNotThrow()
        {
            var storage = new MemoryStorage { Content = "1|0|0|1", ThrowOnRead = true };
            var repo = new SaveRepository(storage, new FakeSerializer(), 3);

            Assert.DoesNotThrow(() => repo.Load());
            Assert.That(repo.LastLoadWasCorrupt, Is.True);
        }

        [Test]
        public void Load_ClampsOutOfRangeValues()
        {
            var storage = new MemoryStorage { Content = "1|99|-5|7" };
            var repo = new SaveRepository(storage, new FakeSerializer(), 3);

            var data = repo.Load();

            Assert.That(data.unlockedStage, Is.EqualTo(2));
            Assert.That(data.highScore, Is.EqualTo(0));
            Assert.That(data.masterVolume, Is.EqualTo(1f));
        }

        [Test]
        public void Load_MigratesOldVersion()
        {
            var storage = new MemoryStorage { Content = "0|1|500|1" };
            var repo = new SaveRepository(storage, new FakeSerializer(), 3);

            var data = repo.Load();

            Assert.That(data.version, Is.EqualTo(SaveData.CurrentVersion));
            Assert.That(data.unlockedStage, Is.EqualTo(1));
            Assert.That(data.highScore, Is.EqualTo(500));
        }

        [Test]
        public void Save_RoundTrips()
        {
            var storage = new MemoryStorage();
            var repo = new SaveRepository(storage, new FakeSerializer(), 3);
            repo.Load();
            repo.Data.unlockedStage = 2;
            repo.TrySetHighScore(12345);

            Assert.That(repo.Save(), Is.True);

            var repo2 = new SaveRepository(storage, new FakeSerializer(), 3);
            var data = repo2.Load();
            Assert.That(data.unlockedStage, Is.EqualTo(2));
            Assert.That(data.highScore, Is.EqualTo(12345));
        }

        [Test]
        public void TrySetHighScore_OnlyIncreases()
        {
            var repo = new SaveRepository(new MemoryStorage(), new FakeSerializer(), 3);
            repo.Load();

            Assert.That(repo.TrySetHighScore(100), Is.True);
            Assert.That(repo.TrySetHighScore(50), Is.False);
            Assert.That(repo.Data.highScore, Is.EqualTo(100));
        }

        [Test]
        public void ResetProgress_KeepsPreferences()
        {
            var repo = new SaveRepository(new MemoryStorage(), new FakeSerializer(), 3);
            repo.Load();
            repo.Data.unlockedStage = 2;
            repo.Data.highScore = 999;
            repo.Data.masterVolume = 0.3f;

            repo.ResetProgress();

            Assert.That(repo.Data.unlockedStage, Is.EqualTo(0));
            Assert.That(repo.Data.highScore, Is.EqualTo(0));
            Assert.That(repo.Data.masterVolume, Is.EqualTo(0.3f));
        }
    }

    public class StageProgressionTests
    {
        [Test]
        public void CompletingStage_UnlocksNext()
        {
            Assert.That(StageProgression.UnlockAfterCompletion(0, 0, 3), Is.EqualTo(1));
            Assert.That(StageProgression.UnlockAfterCompletion(1, 1, 3), Is.EqualTo(2));
        }

        [Test]
        public void CompletingLastStage_StaysAtLast()
        {
            Assert.That(StageProgression.UnlockAfterCompletion(2, 2, 3), Is.EqualTo(2));
            Assert.That(StageProgression.HasNextStage(2, 3), Is.False);
            Assert.That(StageProgression.HasNextStage(1, 3), Is.True);
        }

        [Test]
        public void ReplayingEarlyStage_NeverRegresses()
        {
            Assert.That(StageProgression.UnlockAfterCompletion(2, 0, 3), Is.EqualTo(2));
        }
    }
}
