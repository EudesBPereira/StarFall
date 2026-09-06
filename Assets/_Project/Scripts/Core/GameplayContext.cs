using Starfall.Audio;
using Starfall.Enemies;
using Starfall.Input;
using Starfall.Player;
using Starfall.Pooling;
using Starfall.Save;
using Starfall.Scoring;
using Starfall.VFX;
using Starfall.Waves;
using UnityEngine;

namespace Starfall.Core
{
    /// <summary>
    /// Scene-scoped composition root for the gameplay scene. Holds references to the scene services so
    /// spawned objects (enemies, projectiles, pickups) resolve them once at spawn time instead of
    /// searching the hierarchy. <see cref="Current"/> is only valid while the gameplay scene is loaded.
    /// </summary>
    [DefaultExecutionOrder(-300)]
    public sealed class GameplayContext : MonoBehaviour
    {
        public static GameplayContext Current { get; private set; }

        [SerializeField] internal GameConfig config;
        [SerializeField] internal AudioLibrary audioLibrary;
        [SerializeField] internal PlayArea playArea;
        [SerializeField] internal PoolService pools;
        [SerializeField] internal EnemyRegistry enemies;
        [SerializeField] internal EnemySpawner spawner;
        [SerializeField] internal PlayerShip player;
        [SerializeField] internal GameInputReader input;
        [SerializeField] internal ScoreService score;
        [SerializeField] internal StageDirector stageDirector;
        [SerializeField] internal VfxSpawner vfx;
        [SerializeField] internal CameraShake cameraShake;
        [SerializeField] internal RunTracker runTracker;
        [SerializeField] internal Waves.HazardController hazards;

        public GameConfig Config => config;
        public PlayArea PlayArea => playArea;
        public PoolService Pools => pools;
        public EnemyRegistry Enemies => enemies;
        public EnemySpawner Spawner => spawner;
        public PlayerShip Player => player;
        public GameInputReader Input => input;
        public ScoreService Score => score;
        public StageDirector StageDirector => stageDirector;
        public VfxSpawner Vfx => vfx;
        public CameraShake CameraShake => cameraShake;
        public RunTracker RunTracker => runTracker;
        public Waves.HazardController Hazards => hazards;

        public StageDefinition CurrentStage => config != null ? config.GetStage(GameSession.CurrentStageIndex) : null;

        private void Awake()
        {
            if (Current != null && Current != this)
            {
                Debug.LogWarning("[Starfall] Duplicate GameplayContext, destroying the newest one.");
                Destroy(gameObject);
                return;
            }
            Current = this;

            // Allow opening the Gameplay scene directly in the editor without passing through Boot.
            SaveService.EnsureInitialized(config != null ? config.StageCount : 5);
            AudioManager.Ensure(audioLibrary);

            if (playArea != null && config != null)
                playArea.Configure(config.PlayAreaWidth, config.DespawnMargin);
        }

        private void OnDestroy()
        {
            if (Current == this) Current = null;
        }
    }
}
