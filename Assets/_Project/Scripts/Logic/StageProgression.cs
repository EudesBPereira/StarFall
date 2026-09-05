// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;

namespace Starfall.Logic
{
    /// <summary>Stage unlock rules.</summary>
    public static class StageProgression
    {
        /// <summary>Returns the new highest unlocked stage index after completing <paramref name="completedIndex"/>.</summary>
        public static int UnlockAfterCompletion(int currentUnlocked, int completedIndex, int stageCount)
        {
            if (stageCount <= 0) return 0;
            int next = Math.Min(completedIndex + 1, stageCount - 1);
            return Math.Clamp(Math.Max(currentUnlocked, next), 0, stageCount - 1);
        }

        public static bool HasNextStage(int completedIndex, int stageCount) => completedIndex + 1 < stageCount;

        public static int ClampStageIndex(int index, int stageCount) =>
            stageCount <= 0 ? 0 : Math.Clamp(index, 0, stageCount - 1);
    }
}
