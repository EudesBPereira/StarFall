using System.Collections;
using Starfall.Audio;
using Starfall.Combat;
using Starfall.Logic;
using Starfall.Save;
using Starfall.UI;
using Starfall.VFX;
using UnityEngine;

namespace Starfall.Core
{
    /// <summary>
    /// Gameplay state machine: Briefing -> Playing <-> Paused, PlayerDown -> Playing | GameOver, Victory.
    /// Owns lives and the stage lifecycle; delegates rules to models and services.
    /// </summary>
    public sealed class GameFlowController : MonoBehaviour
    {
        [SerializeField] internal GameplayContext ctx;
        [SerializeField] internal HudPresenter hud;
        [SerializeField] internal BriefingPanel briefingPanel;
        [SerializeField] internal PausePanel pausePanel;
        [SerializeField] internal VictoryPanel victoryPanel;
        [SerializeField] internal GameOverPanel gameOverPanel;
        [SerializeField] internal SettingsPanel settingsPanel;
        [SerializeField] internal StageBackground background;
        [SerializeField] internal GameObject touchControls;

        private LivesModel _lives;
        private GameState _state = GameState.None;
        private Waves.StageDefinition _stage;
        private Coroutine _transition;

        public GameState State => _state;
        public LivesModel Lives => _lives;

        private void Start()
        {
            if (ctx == null) ctx = GameplayContext.Current;
            var config = ctx.Config;
            _stage = ctx.CurrentStage;

            int startLives = GameSession.CarriedLives >= 0 ? GameSession.CarriedLives : config.StartingLives;
            _lives = new LivesModel(startLives, config.MaxLives);
            _lives.Changed += OnLivesChanged;

            PrewarmPools(config);
            if (background != null) background.Apply(_stage);
            ctx.Player.Initialize(ctx);
            if (hud != null) hud.Bind(ctx, _lives);

            WirePanels();
            GameSignals.PlayerDied += OnPlayerDied;
            GameSignals.StageCompleted += OnStageCompleted;

            if (_stage != null) AudioManager.Instance.PlayMusic(_stage.Music);
            ShowBriefing();
        }

        private void OnDestroy()
        {
            GameSignals.PlayerDied -= OnPlayerDied;
            GameSignals.StageCompleted -= OnStageCompleted;
            if (_lives != null) _lives.Changed -= OnLivesChanged;
            Time.timeScale = 1f;
        }

        private void PrewarmPools(GameConfig config)
        {
            var spawner = ctx.Spawner;
            if (spawner == null) return;
            if (spawner.defaultEnemyPrefab != null) ctx.Pools.Prewarm(spawner.defaultEnemyPrefab, config.PrewarmEnemies);
            if (spawner.enemyProjectilePrefab != null) ctx.Pools.Prewarm(spawner.enemyProjectilePrefab, config.PrewarmEnemyProjectiles);
            var weapon = ctx.Player.Definition != null ? ctx.Player.Definition.Weapon : null;
            if (weapon != null && weapon.ProjectilePrefab != null) ctx.Pools.Prewarm(weapon.ProjectilePrefab, config.PrewarmPlayerProjectiles);
            if (ctx.Vfx != null && ctx.Vfx.explosionPrefab != null) ctx.Pools.Prewarm(ctx.Vfx.explosionPrefab, config.PrewarmExplosions);
        }

        private void WirePanels()
        {
            if (briefingPanel != null && briefingPanel.launchButton != null) briefingPanel.launchButton.onClick.AddListener(StartStage);
            if (pausePanel != null)
            {
                if (pausePanel.continueButton != null) pausePanel.continueButton.onClick.AddListener(Resume);
                if (pausePanel.settingsButton != null) pausePanel.settingsButton.onClick.AddListener(OpenSettingsFromPause);
                if (pausePanel.restartButton != null) pausePanel.restartButton.onClick.AddListener(RestartStage);
                if (pausePanel.menuButton != null) pausePanel.menuButton.onClick.AddListener(ReturnToMenu);
            }
            if (victoryPanel != null)
            {
                if (victoryPanel.nextButton != null) victoryPanel.nextButton.onClick.AddListener(NextStage);
                if (victoryPanel.menuButton != null) victoryPanel.menuButton.onClick.AddListener(ReturnToMenu);
            }
            if (gameOverPanel != null)
            {
                if (gameOverPanel.restartButton != null) gameOverPanel.restartButton.onClick.AddListener(RestartStage);
                if (gameOverPanel.menuButton != null) gameOverPanel.menuButton.onClick.AddListener(ReturnToMenu);
            }
            if (settingsPanel != null) settingsPanel.Closed += OnSettingsClosed;

            HideAllPanels();
        }

        private void HideAllPanels()
        {
            if (briefingPanel != null) briefingPanel.Hide();
            if (pausePanel != null) pausePanel.Hide();
            if (victoryPanel != null) victoryPanel.Hide();
            if (gameOverPanel != null) gameOverPanel.Hide();
            if (settingsPanel != null) settingsPanel.Hide();
        }

        private void Update()
        {
            var input = ctx.Input;
            if (input == null) return;
            switch (_state)
            {
                case GameState.Briefing:
                    if (input.ConfirmPressed) StartStage();
                    break;
                case GameState.Playing:
                    if (input.PausePressed) Pause();
                    break;
                case GameState.Paused:
                    if (input.PausePressed && (settingsPanel == null || !settingsPanel.IsVisible)) Resume();
                    break;
            }
        }

        // ---- States --------------------------------------------------------------------------------

        private void SetState(GameState next)
        {
            if (_state == next) return;
            var previous = _state;
            _state = next;
            if (touchControls != null) touchControls.SetActive(next == GameState.Playing);
            GameSignals.RaiseGameStateChanged(previous, next);
        }

        private void ShowBriefing()
        {
            SetState(GameState.Briefing);
            Time.timeScale = 0f;
            int number = GameSession.CurrentStageIndex + 1;
            string hint = Application.isMobilePlatform ? "TAP TO LAUNCH" : "PRESS SPACE / START TO LAUNCH";
            if (briefingPanel != null) briefingPanel.Show(_stage, number, hint);
            else StartStage();
        }

        private void StartStage()
        {
            if (_state != GameState.Briefing) return;
            if (briefingPanel != null) briefingPanel.Hide();
            Time.timeScale = 1f;
            SpawnPlayer();
            SetState(GameState.Playing);
            ctx.StageDirector.Begin(_stage, ctx);
        }

        private void SpawnPlayer()
        {
            var config = ctx.Config;
            var pos = ctx.PlayArea.PlayerSpawnPosition(config.SpawnHeightFraction);
            ctx.Player.SpawnAt(pos, config.RespawnInvulnerability);
            ctx.Player.SetControlEnabled(true);
            GameSignals.RaisePlayerRespawned();
        }

        private void OnLivesChanged(int lives) => GameSignals.RaiseLivesChanged(lives);

        private void OnPlayerDied()
        {
            if (_state != GameState.Playing) return;
            SetState(GameState.PlayerDown);
            bool canRespawn = _lives.LoseLife();
            if (_transition != null) StopCoroutine(_transition);
            _transition = StartCoroutine(canRespawn ? RespawnRoutine() : GameOverRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(ctx.Config.RespawnDelay);
            if (_state != GameState.PlayerDown) yield break;
            Projectile.ClearEnemyProjectiles();
            SpawnPlayer();
            SetState(GameState.Playing);
            _transition = null;
        }

        private IEnumerator GameOverRoutine()
        {
            yield return new WaitForSeconds(1.4f);
            if (_state != GameState.PlayerDown) yield break;
            SetState(GameState.GameOver);
            ctx.StageDirector.Stop();
            int total = ctx.Score.RunTotal;
            bool record = SaveService.RecordScore(total);
            Time.timeScale = 0f;
            AudioManager.Instance.StopMusic(1f);
            if (gameOverPanel != null) gameOverPanel.Show(total, record);
            _transition = null;
        }

        private void OnStageCompleted()
        {
            if (_state != GameState.Playing && _state != GameState.PlayerDown) return;
            if (_transition != null) StopCoroutine(_transition);
            _transition = StartCoroutine(VictoryRoutine());
        }

        private IEnumerator VictoryRoutine()
        {
            SetState(GameState.Victory);
            ctx.Player.SetControlEnabled(false);
            ctx.Player.Health.Invulnerable = true;
            Projectile.ClearEnemyProjectiles();
            ctx.Enemies.KillAllCommon(DamageSource.Environment);
            GameSignals.RaiseStageMessage("SECTOR CLEARED", 2f);
            yield return new WaitForSeconds(2f);

            var score = ctx.Score.Model;
            if (_stage != null) ctx.Score.AddBonus(_stage.CompletionBonus);
            int stageIndex = GameSession.CurrentStageIndex;
            int stageCount = ctx.Config.StageCount;
            SaveService.RecordStageCompleted(stageIndex);
            int total = ctx.Score.RunTotal;
            bool record = SaveService.RecordScore(total);
            bool hasNext = StageProgression.HasNextStage(stageIndex, stageCount);

            Time.timeScale = 0f;
            if (victoryPanel != null)
                victoryPanel.Show(hasNext ? "SECTOR CLEARED" : "THE SWARM IS DEFEATED", total, score.HighestMultiplier,
                    score.EnemiesDestroyed, score.DamageTakenCount, _lives.Lives, record, hasNext);
            _transition = null;
        }

        // ---- Pause -----------------------------------------------------------------------------------

        public void Pause()
        {
            if (_state != GameState.Playing) return;
            SetState(GameState.Paused);
            Time.timeScale = 0f;
            ctx.Input.ResetTransient();
            if (pausePanel != null) pausePanel.Show();
        }

        public void Resume()
        {
            if (_state != GameState.Paused) return;
            if (settingsPanel != null) settingsPanel.Hide();
            if (pausePanel != null) pausePanel.Hide();
            Time.timeScale = 1f;
            ctx.Input.ResetTransient();
            SetState(GameState.Playing);
        }

        private void OpenSettingsFromPause()
        {
            if (pausePanel != null) pausePanel.Hide();
            if (settingsPanel != null) settingsPanel.Show();
        }

        private void OnSettingsClosed()
        {
            var data = SaveService.Data;
            if (data != null && ctx.Input != null) ctx.Input.AutoFire = data.autoFire;
            if (_state == GameState.Paused && pausePanel != null) pausePanel.Show();
        }

        // ---- Navigation ------------------------------------------------------------------------------

        public void RestartStage()
        {
            SaveService.Save();
            SceneLoader.ReloadCurrent();
        }

        public void ReturnToMenu()
        {
            SaveService.Save();
            SceneLoader.LoadMainMenu(ctx.Config);
        }

        public void NextStage()
        {
            GameSession.PrepareNextStage(ctx.Score.RunTotal, _lives.Lives);
            SceneLoader.LoadGameplay(ctx.Config);
        }
    }
}
