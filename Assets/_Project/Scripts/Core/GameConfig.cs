using Starfall.Waves;
using UnityEngine;

namespace Starfall.Core
{
    /// <summary>
    /// Global tunables for the campaign. Values documented in docs/BALANCING.md.
    /// One instance lives at Assets/_Project/ScriptableObjects/Config/GameConfig.asset.
    /// </summary>
    [CreateAssetMenu(menuName = "Starfall/Config/Game Config", fileName = "GameConfig")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Scenes")]
        public string BootScene = "Boot";
        public string MainMenuScene = "MainMenu";
        public string GameplayScene = "Gameplay";

        [Header("Campaign")]
        public StageDefinition[] Stages = new StageDefinition[0];
        [Min(1)] public int StartingLives = 3;
        [Min(1)] public int MaxLives = 9;

        [Header("Player")]
        [Min(0f)] public float RespawnDelay = 1.5f;
        [Min(0f)] public float RespawnInvulnerability = 2.5f;
        [Min(0f)] public float HitInvulnerability = 1.0f;
        [Tooltip("Fraction of the visible height (from the bottom) where the ship spawns.")]
        [Range(0f, 1f)] public float SpawnHeightFraction = 0.15f;

        [Header("Play area")]
        [Tooltip("Visible world width in units. Orthographic size is derived from this and the aspect ratio.")]
        [Min(4f)] public float PlayAreaWidth = 10f;
        [Min(0f)] public float PlayerEdgePadding = 0.45f;
        [Tooltip("Objects further than this from the visible area are despawned.")]
        [Min(0.5f)] public float DespawnMargin = 2.5f;

        [Header("Scoring")]
        [Min(1)] public int MaxMultiplier = 10;
        [Min(1)] public int KillsPerMultiplierStep = 4;

        [Header("Ultimate")]
        [Min(1f)] public float UltimateEnergyMax = 100f;
        [Min(0f)] public float UltimateEliteDamage = 150f;
        public bool UltimateClearsEnemyProjectiles = true;

        [Header("Pooling")]
        [Min(0)] public int PrewarmPlayerProjectiles = 48;
        [Min(0)] public int PrewarmEnemyProjectiles = 96;
        [Min(0)] public int PrewarmEnemies = 24;
        [Min(0)] public int PrewarmExplosions = 24;

        public int StageCount => Stages != null ? Stages.Length : 0;

        public StageDefinition GetStage(int index)
        {
            if (Stages == null || Stages.Length == 0) return null;
            index = Mathf.Clamp(index, 0, Stages.Length - 1);
            return Stages[index];
        }
    }
}
