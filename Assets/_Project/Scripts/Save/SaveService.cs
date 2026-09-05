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
        private static int _stageCount = 3;

        public static bool IsInitialized => _repository != null;
        public static SaveRepository Repository => _repository;
        public static SaveData Data => _repository != null ? _repository.Data : null;

        public static void EnsureInitialized(int stageCount)
        {
            if (_repository != null) return;
            _stageCount = Mathf.Max(1, stageCount);
            _repository = new SaveRepository(new FileSaveStorage(), new JsonUtilitySaveSerializer(), _stageCount);
            _repository.Load();
            if (_repository.LastLoadWasCorrupt)
                Debug.LogWarning("[Starfall] Save file was missing or corrupt. Defaults restored.");
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

        public static void RecordStageCompleted(int stageIndex)
        {
            if (Data == null) return;
            Data.unlockedStage = StageProgression.UnlockAfterCompletion(Data.unlockedStage, stageIndex, _stageCount);
            Save();
        }

        public static bool RecordScore(int score)
        {
            if (_repository == null) return false;
            bool record = _repository.TrySetHighScore(score);
            Save();
            return record;
        }

        public static void ResetProgress()
        {
            _repository?.ResetProgress();
        }
    }
}
