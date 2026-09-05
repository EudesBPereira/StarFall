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
    /// Owns lives and the stage lifecycle for every mode; delegates rules to models and services.
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
        private bool _runRecorded;

        public GameState State => _state;
        public LivesModel Lives => _lives;

        private void Start()
        {
            if (ctx == null) ctx = GameplayContext.Current;
            var config = ctx.Config;
            _stage = GameSession.IsEndless || GameSession.Mode == GameModeId.BossRush
                ? (config.SurvivalLook != null ? config.SurvivalLook : ctx.CurrentStage)
                : ctx.CurrentStage;

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

            if (_stage != null)
            {
                AudioManager.Instance.PlayMusic(GameSession.IsEndless ? MusicId.Survival : _stage.Music);
                AudioManager.Instance.PlayAmbient(_stage.Ambient);
            }
            ShowBriefing();
        }

        private void OnDestroy()
        {
            GameSignals.PlayerDied -= OnPlayerDied;
            GameSignals.StageCompleted -= OnStageCompleted;
            if (_lives != null) _lives.Changed -= OnLivesChanged;
            if (AudioManager.Instance != null) AudioManager.Instance.StopAmbient();
            Time.timeScale = 1f;
        }

        private void PrewarmPools(GameConfig config)
        {
            var spawner = ctx.Spawner;
            if (spawner == null) return;
            if (spawner.defaultEnemyPrefab != null) ctx.Pools.Prewarm(spawner.defaultEnemyPrefab, config.PrewarmEnemies);
            if (spawner.enemyProjectilePrefab != null) ctx.Pools.Prewarm(spawner.enemyProjectilePrefab, config.PrewarmEnemyProjectiles);
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
            string hint = Application.isMobilePlatform ? "TAP TO LAUNCH" : "PRESS SPACE / START TO LAUNCH";
            if (briefingPanel != null) briefingPanel.Show(_stage, GameSession.Mode, GameSession.CurrentStageIndex + 1, hint, ctx.Player.Loadout);
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

            var run = FinalizeRun(false);
            var rewards = ProgressionRules.ComputeRewards(run, _stage != null ? _stage.CompletionBonus : 0);
            RecordRun(run, rewards, out bool record, out int rank);

            Time.timeScale = 0f;
            AudioManager.Instance.StopMusic(1f);
            if (gameOverPanel != null) gameOverPanel.Show(run, rewards, record, rank);
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
            GameSignals.RaiseStageMessage(GameSession.Mode == GameModeId.BossRush ? "ALL BOSSES DESTROYED" : "SECTOR CLEARED", 2f);
            yield return new WaitForSeconds(2f);

            if (_stage != null) ctx.Score.AddBonus(_stage.CompletionBonus);
            var run = FinalizeRun(true);
            var rewards = ProgressionRules.ComputeRewards(run, _stage != null ? _stage.CompletionBonus : 0);
            RecordRun(run, rewards, out bool record, out int rank);

            int stageIndex = GameSession.CurrentStageIndex;
            int stageCount = ctx.Config.StageCount;
            bool hasNext = GameSession.Mode == GameModeId.Campaign && StageProgression.HasNextStage(stageIndex, stageCount);

            Time.timeScale = 0f;
            if (victoryPanel != null)
            {
                string title = GameSession.Mode == GameModeId.BossRush ? "BOSS RUSH COMPLETE"
                    : hasNext ? "SECTOR CLEARED" : "THE SWARM IS DEFEATED";
                victoryPanel.Show(title, run, ctx.Score.Model, _lives.Lives, rewards, record, rank, hasNext);
            }
            _transition = null;
        }

        /// <summary>Copies score/multiplier into the run stats and marks completion.</summary>
        private RunStats FinalizeRun(bool completed)
        {
            var run = GameSession.Run;
            run.Score = ctx.Score.RunTotal;
            run.HighestMultiplier = Mathf.Max(run.HighestMultiplier, ctx.Score.Model.HighestMultiplier);
            run.Completed = completed;
            run.Mode = GameSession.Mode;
            run.StageIndex = GameSession.CurrentStageIndex;
            run.WavesSurvived = Mathf.Max(run.WavesSurvived, ctx.StageDirector.Wave);
            return run;
        }

        /// <summary>Applies rewards, campaign progress, leaderboard, stats and achievements to the save (once per stage).</summary>
        private void RecordRun(RunStats run, in RunRewards rewards, out bool record, out int rank)
        {
            record = false;
            rank = -1;
            if (_runRecorded) return;
            _runRecorded = true;
            var save = SaveService.Data;
            if (save == null) return;

            ProgressionRules.ApplyRewards(save, rewards);
            save.totalKills += run.Kills;
            save.bossesDefeatedMask |= run.BossesDefeatedMask;
            if (run.Mode == GameModeId.Campaign && run.Completed)
                ProgressionRules.RecordCampaignStage(save, run.StageIndex, ctx.Config.StageCount, ctx.Score.Model.Score);

            record = SaveService.RecordScore(run.Score);
            // Endless / boss rush rank on game over or completion; campaign only when the run ends.
            bool finalStage = run.Mode != GameModeId.Campaign || !run.Completed || !StageProgression.HasNextStage(run.StageIndex, ctx.Config.StageCount);
            if (finalStage) rank = SaveService.RecordLeaderboard(run, ctx.Player.Definition != null ? (int)ctx.Player.Definition.Id : 0);

            SaveService.Save();
            AchievementService.Evaluate(run, ctx.Config.StageCount, ctx.Config.BossCount);
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
            GameSession.StartNewRun(GameSession.Mode, GameSession.CurrentStageIndex, GameSession.Mode == GameModeId.DailyChallenge ? GameSession.Seed : System.Environment.TickCount);
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
