using System.Collections.Generic;
using Starfall.Logic;

namespace Starfall.Player
{
    /// <summary>Temporary build chosen during the current stage (plan §2). Reset when a stage starts.</summary>
    public sealed class BuildState
    {
        private readonly List<BuildModId> _chosen = new List<BuildModId>(12);

        public IReadOnlyList<BuildModId> Chosen => _chosen;
        public BuildModifiers Modifiers { get; private set; } = BuildModifiers.Identity;
        public int DraftsTaken { get; private set; }

        public void Reset()
        {
            _chosen.Clear();
            DraftsTaken = 0;
            Modifiers = BuildModifiers.Identity;
        }

        public List<BuildModId> NextDraft(int seed, int stageIndex, List<BuildModId> buffer = null)
        {
            return BuildMods.Draft(seed, stageIndex, DraftsTaken, _chosen, BuildMods.DraftSize, buffer);
        }

        public void Choose(BuildModId id)
        {
            _chosen.Add(id);
            DraftsTaken++;
            Modifiers = BuildMods.Compute(_chosen);
        }

        /// <summary>The player declined (or the pool was empty): the draft still counts so seeds advance.</summary>
        public void Skip()
        {
            DraftsTaken++;
        }
    }
}
