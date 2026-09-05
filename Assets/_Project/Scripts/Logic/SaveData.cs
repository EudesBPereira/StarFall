// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;
using System.Collections.Generic;

namespace Starfall.Logic
{
    /// <summary>One local leaderboard row. Fields are public for JsonUtility.</summary>
    [Serializable]
    public sealed class LeaderboardEntry
    {
        public int score;
        public int mode;      // GameModeId
        public int stage;     // stage index reached / completed (campaign)
        public int ship;      // ShipId
        public int wave;      // waves survived (endless modes)
        public string date = "";
    }

    /// <summary>
    /// Local save payload (version 2). No personal data. Fields are public for JsonUtility compatibility.
    /// Missing fields in older files keep their default values, which is how v1 -> v2 migration works.
    /// </summary>
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentVersion = 2;
        public const int UpgradeNodeCount = 10;
        public const int StageSlots = 5;
        public const int LeaderboardSize = 10;

        public int version = CurrentVersion;

        // ---- Campaign progress ----
        public int unlockedStage = 0;
        public int highScore = 0;
        public int[] stageBestScores = new int[StageSlots];
        public int stagesCompletedMask = 0;
        public int bossesDefeatedMask = 0;

        // ---- Economy / progression ----
        public int credits = 0;
        public int xp = 0;
        public int components = 0;
        public int[] upgradeLevels = new int[UpgradeNodeCount];
        public int unlockedShipsMask = 1;      // bit 0 = Vanguard
        public int unlockedWeaponsMask = 1;    // bit 0 = Laser
        public int selectedShip = 0;
        public int selectedWeapon = 0;

        // ---- Stats & achievements ----
        public int totalKills = 0;
        public int totalRuns = 0;
        public int achievementsMask = 0;
        public List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();
        public int survivalBestWave = 0;
        public int bossRushBestScore = 0;
        public string dailyDate = "";
        public int dailyBestScore = 0;

        // ---- Preferences ----
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;
        public bool autoFire = true;
        public float touchSensitivity = 1.4f;

        public static SaveData CreateDefault() => new SaveData();

        public bool IsShipUnlocked(int shipId) => (unlockedShipsMask & (1 << shipId)) != 0;
        public bool IsWeaponUnlocked(int weaponId) => (unlockedWeaponsMask & (1 << weaponId)) != 0;
        public bool HasAchievement(int id) => (achievementsMask & (1 << id)) != 0;
        public bool IsStageCompleted(int stageIndex) => (stagesCompletedMask & (1 << stageIndex)) != 0;
        public bool IsBossDefeated(int bossId) => (bossesDefeatedMask & (1 << bossId)) != 0;

        public void UnlockShip(int shipId) => unlockedShipsMask |= 1 << shipId;
        public void UnlockWeapon(int weaponId) => unlockedWeaponsMask |= 1 << weaponId;
        public void GrantAchievement(int id) => achievementsMask |= 1 << id;
        public void MarkStageCompleted(int stageIndex) => stagesCompletedMask |= 1 << stageIndex;
        public void MarkBossDefeated(int bossId) => bossesDefeatedMask |= 1 << bossId;

        public int GetUpgradeLevel(int node) =>
            upgradeLevels != null && node >= 0 && node < upgradeLevels.Length ? upgradeLevels[node] : 0;
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

            // v0/v1 -> v2: new fields already hold defaults; make sure the starter ship/weapon are owned.
            if (data.version < 2)
            {
                data.unlockedShipsMask |= 1;
                data.unlockedWeaponsMask |= 1;
                data.version = 2;
            }
            if (data.version > SaveData.CurrentVersion)
                data.version = SaveData.CurrentVersion;

            int stages = Math.Max(1, stageCount);
            data.unlockedStage = StageProgression.ClampStageIndex(data.unlockedStage, stages);
            data.highScore = Math.Max(0, data.highScore);
            data.credits = Math.Max(0, data.credits);
            data.xp = Math.Max(0, data.xp);
            data.components = Math.Max(0, data.components);
            data.totalKills = Math.Max(0, data.totalKills);
            data.totalRuns = Math.Max(0, data.totalRuns);
            data.survivalBestWave = Math.Max(0, data.survivalBestWave);
            data.bossRushBestScore = Math.Max(0, data.bossRushBestScore);
            data.dailyBestScore = Math.Max(0, data.dailyBestScore);
            data.unlockedShipsMask |= 1;
            data.unlockedWeaponsMask |= 1;

            if (data.upgradeLevels == null || data.upgradeLevels.Length != SaveData.UpgradeNodeCount)
            {
                var levels = new int[SaveData.UpgradeNodeCount];
                if (data.upgradeLevels != null)
                    Array.Copy(data.upgradeLevels, levels, Math.Min(levels.Length, data.upgradeLevels.Length));
                data.upgradeLevels = levels;
            }
            for (int i = 0; i < data.upgradeLevels.Length; i++)
                data.upgradeLevels[i] = Math.Clamp(data.upgradeLevels[i], 0, UpgradeCatalog.MaxLevel);

            if (data.stageBestScores == null || data.stageBestScores.Length != SaveData.StageSlots)
            {
                var best = new int[SaveData.StageSlots];
                if (data.stageBestScores != null)
                    Array.Copy(data.stageBestScores, best, Math.Min(best.Length, data.stageBestScores.Length));
                data.stageBestScores = best;
            }

            if (!data.IsShipUnlocked(data.selectedShip)) data.selectedShip = 0;
            if (!data.IsWeaponUnlocked(data.selectedWeapon)) data.selectedWeapon = 0;

            if (data.leaderboard == null) data.leaderboard = new List<LeaderboardEntry>();
            data.leaderboard.RemoveAll(e => e == null || e.score < 0);
            Leaderboard.SortAndTrim(data.leaderboard);
            if (data.dailyDate == null) data.dailyDate = "";

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

        /// <summary>Resets progress (campaign, economy, stats) but keeps preferences.</summary>
        public void ResetProgress()
        {
            var fresh = SaveData.CreateDefault();
            fresh.masterVolume = Data.masterVolume;
            fresh.musicVolume = Data.musicVolume;
            fresh.sfxVolume = Data.sfxVolume;
            fresh.autoFire = Data.autoFire;
            fresh.touchSensitivity = Data.touchSensitivity;
            Data = fresh;
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

    /// <summary>Local ranking helpers (top N, sorted by score).</summary>
    public static class Leaderboard
    {
        /// <summary>Inserts an entry and returns its 0-based rank, or -1 when it did not make the list.</summary>
        public static int Insert(List<LeaderboardEntry> list, LeaderboardEntry entry, int maxSize = SaveData.LeaderboardSize)
        {
            if (list == null || entry == null) return -1;
            list.Add(entry);
            SortAndTrim(list, maxSize);
            return list.IndexOf(entry);
        }

        public static void SortAndTrim(List<LeaderboardEntry> list, int maxSize = SaveData.LeaderboardSize)
        {
            if (list == null) return;
            list.Sort((a, b) => b.score.CompareTo(a.score));
            if (list.Count > maxSize) list.RemoveRange(maxSize, list.Count - maxSize);
        }

        public static int CountForMode(List<LeaderboardEntry> list, int mode)
        {
            int n = 0;
            if (list == null) return 0;
            for (int i = 0; i < list.Count; i++) if (list[i].mode == mode) n++;
            return n;
        }
    }
}
