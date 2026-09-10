using System;
using System.Collections.Generic;
using Starfall.Logic;
using UnityEngine;

namespace Starfall.Save
{
    /// <summary>
    /// Static facade over <see cref="SaveRepository"/>. Static because the save must survive scene loads and
    /// has no per-scene state; the repository itself is fully testable (see SaveRepositoryTests).
    /// </summary>
    public static class SaveService
    {
        private static SaveRepository _repository;
        private static int _stageCount = 5;

        public static bool IsInitialized => _repository != null;
        public static SaveRepository Repository => _repository;
        public static SaveData Data => _repository != null ? _repository.Data : null;
        public static int StageCount => _stageCount;

        public static void EnsureInitialized(int stageCount)
        {
            if (_repository != null) return;
            _stageCount = Mathf.Max(1, stageCount);
            _repository = new SaveRepository(new FileSaveStorage(), new JsonUtilitySaveSerializer(), _stageCount);
            _repository.Load();
            if (_repository.LastLoadWasCorrupt)
                Debug.LogWarning("[Starfall] Save file was missing or corrupt. Defaults restored.");
            ApplyLanguage();
        }

        /// <summary>Registers the translation tables and selects the saved language (or the device language).</summary>
        public static void ApplyLanguage()
        {
            Loc.Register(Language.Portuguese, PortugueseStrings.Table);
            var data = Data;
            var language = data != null && data.language >= 0 ? (Language)data.language : Loc.FromSystem(Application.systemLanguage.ToString());
            Loc.Set(language);
        }

        /// <summary>Replaces the repository (tests / tools).</summary>
        public static void Override(SaveRepository repository)
        {
            _repository = repository;
        }

        public static void Save()
        {
            if (_repository == null) return;
            if (!_repository.Save())
                Debug.LogWarning("[Starfall] Could not write save file.");
        }

        public static bool HasProgress => Data != null && (Data.unlockedStage > 0 || Data.highScore > 0);

        public static bool RecordScore(int score)
        {
            if (_repository == null) return false;
            bool record = _repository.TrySetHighScore(score);
            Save();
            return record;
        }

        /// <summary>Adds a leaderboard row for the finished run. Returns the rank (0-based) or -1.</summary>
        public static int RecordLeaderboard(RunStats run, int shipId)
        {
            if (Data == null || run == null) return -1;
            var entry = new LeaderboardEntry
            {
                score = run.Score,
                mode = (int)run.Mode,
                stage = run.StageIndex,
                ship = shipId,
                wave = run.WavesSurvived,
                date = DateTime.Now.ToString("yyyy-MM-dd"),
                rank = (int)run.Rank,
                seed = Core.GameSession.Seed,
                balance = SaveData.BalanceVersion,
                revive = run.UsedRevive,
            };
            int rank = Leaderboard.Insert(Data.leaderboard, entry);
            switch (run.Mode)
            {
                case GameModeId.Survival:
                    Data.survivalBestWave = Mathf.Max(Data.survivalBestWave, run.WavesSurvived);
                    break;
                case GameModeId.BossRush:
                    Data.bossRushBestScore = Mathf.Max(Data.bossRushBestScore, run.Score);
                    break;
                case GameModeId.DailyChallenge:
                {
                    string today = DateTime.Now.ToString("yyyy-MM-dd");
                    if (Data.dailyDate != today) { Data.dailyDate = today; Data.dailyBestScore = 0; }
                    Data.dailyBestScore = Mathf.Max(Data.dailyBestScore, run.Score);
                    break;
                }
            }
            return rank;
        }

        public static void ResetProgress()
        {
            _repository?.ResetProgress();
        }

        public static List<LeaderboardEntry> LeaderboardFor(GameModeId mode, List<LeaderboardEntry> buffer)
        {
            buffer.Clear();
            if (Data == null) return buffer;
            foreach (var e in Data.leaderboard) if (e.mode == (int)mode) buffer.Add(e);
            return buffer;
        }
    }
}
