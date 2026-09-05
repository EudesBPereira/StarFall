// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;

namespace Starfall.Logic
{
    /// <summary>
    /// Local save payload. No personal data. Fields are public for JsonUtility compatibility.
    /// </summary>
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public int unlockedStage = 0;
        public int highScore = 0;
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;
        public bool autoFire = true;
        public float touchSensitivity = 1.4f;

        public static SaveData CreateDefault() => new SaveData();

        public SaveData Clone()
        {
            return (SaveData)MemberwiseClone();
        }
    }

    /// <summary>Abstraction over the physical storage so the repository can be unit tested.</summary>
    public interface ISaveStorage
    {
        bool Exists();
        string Read();
        void Write(string content);
        void Delete();
    }

    /// <summary>Abstraction over the serializer (JsonUtility in Unity, anything in tests).</summary>
    public interface ISaveSerializer
    {
        string Serialize(SaveData data);
        /// <summary>Must throw or return null on invalid content.</summary>
        SaveData Deserialize(string content);
    }

    /// <summary>Clamps values into valid ranges and upgrades older versions.</summary>
    public static class SaveDataValidator
    {
        public static SaveData SanitizeAndMigrate(SaveData data, int stageCount)
        {
            if (data == null) return SaveData.CreateDefault();

            // Migration hook: versions below CurrentVersion are upgraded step by step.
            if (data.version < 1)
            {
                // v0 -> v1: nothing to translate; fields keep defaults.
                data.version = 1;
            }
            if (data.version > SaveData.CurrentVersion)
            {
                // Save from a newer build: keep what we understand, downgrade version tag.
                data.version = SaveData.CurrentVersion;
            }

            data.unlockedStage = StageProgression.ClampStageIndex(data.unlockedStage, Math.Max(1, stageCount));
            data.highScore = Math.Max(0, data.highScore);
            data.masterVolume = Clamp01(data.masterVolume);
            data.musicVolume = Clamp01(data.musicVolume);
            data.sfxVolume = Clamp01(data.sfxVolume);
            data.touchSensitivity = Math.Clamp(data.touchSensitivity, 0.5f, 3f);
            return data;
        }

        private static float Clamp01(float v)
        {
            if (float.IsNaN(v)) return 1f;
            return Math.Clamp(v, 0f, 1f);
        }
    }

    /// <summary>
    /// Loads / saves the local save. Never throws on load: invalid data falls back to defaults.
    /// </summary>
    public sealed class SaveRepository
    {
        private readonly ISaveStorage _storage;
        private readonly ISaveSerializer _serializer;
        private readonly int _stageCount;

        public SaveData Data { get; private set; }
        public bool LastLoadWasCorrupt { get; private set; }

        public SaveRepository(ISaveStorage storage, ISaveSerializer serializer, int stageCount)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _stageCount = Math.Max(1, stageCount);
            Data = SaveData.CreateDefault();
        }

        public SaveData Load()
        {
            LastLoadWasCorrupt = false;
            SaveData loaded = null;
            try
            {
                if (_storage.Exists())
                {
                    string content = _storage.Read();
                    if (!string.IsNullOrWhiteSpace(content))
                        loaded = _serializer.Deserialize(content);
                    if (loaded == null) LastLoadWasCorrupt = true;
                }
            }
            catch (Exception)
            {
                LastLoadWasCorrupt = true;
                loaded = null;
            }

            Data = SaveDataValidator.SanitizeAndMigrate(loaded ?? SaveData.CreateDefault(), _stageCount);
            return Data;
        }

        public bool Save()
        {
            try
            {
                Data = SaveDataValidator.SanitizeAndMigrate(Data, _stageCount);
                _storage.Write(_serializer.Serialize(Data));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Resets progress (stage + high score) but keeps preferences.</summary>
        public void ResetProgress()
        {
            Data.unlockedStage = 0;
            Data.highScore = 0;
            Save();
        }

        public void ResetEverything()
        {
            Data = SaveData.CreateDefault();
            try { _storage.Delete(); } catch (Exception) { /* ignore */ }
        }

        /// <summary>Returns true when the score is a new record (and stores it).</summary>
        public bool TrySetHighScore(int score)
        {
            if (score <= Data.highScore) return false;
            Data.highScore = score;
            return true;
        }
    }
}
