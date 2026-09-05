using System;
using System.Collections;
using NUnit.Framework;
using Starfall.Core;
using Starfall.Logic;
using Starfall.PowerUps;
using Starfall.Save;
using Starfall.UI;
using Starfall.Waves;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Starfall.Tests.PlayMode
{
    /// <summary>
    /// End-to-end smoke tests over the generated scenes. They exercise the vertical slice acceptance criteria
    /// without any rendering assumptions so they can run in batch mode.
    /// </summary>
    public class VerticalSliceTests
    {
        private sealed class MemoryStorage : ISaveStorage
        {
            private string _content;
            public bool Exists() => _content != null;
            public string Read() => _content;
            public void Write(string content) => _content = content;
            public void Delete() => _content = null;
        }

        private static IEnumerator WaitUntil(Func<bool> condition, float timeoutSeconds, string what)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!condition())
            {
                if (Time.realtimeSinceStartup > deadline)
                    Assert.Fail($"Timed out waiting for: {what}");
                yield return null;
            }
        }

        private static IEnumerator WaitRealtime(float seconds)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < deadline) yield return null;
        }

        [SetUp]
        public void SetUp()
        {
            // Never touch the real save file from tests.
            SaveService.Override(new SaveRepository(new MemoryStorage(), new JsonUtilitySaveSerializer(), 3));
            SaveService.Repository.Load();
            GameSignals.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator Boot_LoadsMainMenu()
        {
            SceneManager.LoadScene("Boot");
            yield return null;
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "MainMenu", 15f, "MainMenu scene");
            yield return null;
            var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuController>();
            Assert.That(menu, Is.Not.Null);
            Assert.That(menu.playButton.gameObject.activeInHierarchy, Is.True);
            Assert.That(menu.missionPanel, Is.Not.Null, "mission select panel");
            Assert.That(menu.hangarPanel, Is.Not.Null, "hangar panel");
            Assert.That(menu.upgradesPanel, Is.Not.Null, "upgrades panel");
            Assert.That(menu.rankingPanel, Is.Not.Null, "ranking panel");
        }

        [UnityTest]
        public IEnumerator Gameplay_VerticalSlice()
        {
            GameSession.StartNewRun(0);
            SceneManager.LoadScene("Gameplay");
            yield return null;
            yield return null;

            var ctx = GameplayContext.Current;
            Assert.That(ctx, Is.Not.Null, "GameplayContext");
            var flow = UnityEngine.Object.FindFirstObjectByType<GameFlowController>();
            Assert.That(flow, Is.Not.Null, "GameFlowController");
            var player = ctx.Player;
            var config = ctx.Config;

            // 1. Briefing shown, time frozen.
            yield return WaitUntil(() => flow.State == GameState.Briefing, 5f, "Briefing state");
            Assert.That(Time.timeScale, Is.EqualTo(0f));

            // 2. Start the stage (confirm intent from the input layer).
            ctx.Input.PressConfirm();
            yield return WaitUntil(() => flow.State == GameState.Playing, 5f, "Playing state");
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(player.IsAlive, Is.True);
            Assert.That(player.ControlEnabled, Is.True);
            Assert.That(ctx.StageDirector.IsRunning, Is.True);

            // 3/4. Ship stays inside the play area even when pushed outside.
            player.transform.position = new Vector3(100f, 100f, 0f);
            yield return null;
            yield return null;
            Assert.That(ctx.PlayArea.IsInside(player.transform.position), Is.True, "player clamped to play area");

            // 5/6/13. Enemies spawn and the stage advances (auto-fire is on, so projectiles are firing).
            yield return WaitUntil(() => ctx.Enemies.Count > 0, 20f, "first enemy spawn");
            Assert.That(ctx.StageDirector.EventIndex, Is.GreaterThanOrEqualTo(0));

            // 7. Damage hits shield before hull (wait for spawn invulnerability to end).
            yield return WaitRealtime(config.RespawnInvulnerability + 0.5f);
            float shieldBefore = player.Health.Shield;
            float hullBefore = player.Health.Hull;
            var result = player.Health.ApplyDamage(new DamageInfo(10f, DamageSource.Enemy), player.transform.position);
            Assert.That(result.Applied, Is.True, "damage applied after invulnerability window");
            Assert.That(player.Health.Shield, Is.EqualTo(shieldBefore - 10f).Within(0.01f));
            Assert.That(player.Health.Hull, Is.EqualTo(hullBefore).Within(0.01f));
            Assert.That(ctx.Score.Model.Multiplier, Is.EqualTo(1));

            // 9. Collect a power-up (laser level goes up).
            var laserUp = FindPowerUp(config, PowerUpKind.LaserLevel);
            Assert.That(laserUp, Is.Not.Null, "LaserLevel power-up in stage data");
            int levelBefore = player.Weapon.Level;
            ctx.Spawner.SpawnPickup(laserUp, player.transform.position);
            yield return WaitUntil(() => player.Weapon.Level == levelBefore + 1, 3f, "laser level up after pickup");

            // 10. Score + multiplier react to kills.
            int scoreBefore = ctx.Score.Model.Score;
            var drone = FirstEnemyDefinition(config);
            var enemy = ctx.Spawner.Spawn(drone, new Vector2(ctx.PlayArea.Center.x, ctx.PlayArea.Center.y));
            Assert.That(enemy, Is.Not.Null);
            enemy.Health.Kill(DamageSource.Player);
            yield return null;
            Assert.That(ctx.Score.Model.Score, Is.EqualTo(scoreBefore + drone.ScoreValue));
            Assert.That(ctx.Score.Model.EnemiesDestroyed, Is.GreaterThanOrEqualTo(1));

            // 11. Ultimate only when full, then consumes the bar.
            Assert.That(player.Ultimate.TryActivate(), Is.False, "not full yet");
            player.Ultimate.AddEnergy(config.UltimateEnergyMax);
            Assert.That(player.Ultimate.IsReady, Is.True);
            Assert.That(player.Ultimate.TryActivate(), Is.True);
            Assert.That(player.Ultimate.Energy.Current, Is.EqualTo(0f));

            // 12. Pause and continue.
            flow.Pause();
            Assert.That(flow.State, Is.EqualTo(GameState.Paused));
            Assert.That(Time.timeScale, Is.EqualTo(0f));
            yield return null;
            flow.Resume();
            Assert.That(flow.State, Is.EqualTo(GameState.Playing));
            Assert.That(Time.timeScale, Is.EqualTo(1f));

            // 8. Lose a life and respawn.
            int livesBefore = flow.Lives.Lives;
            player.Health.Kill(DamageSource.Enemy);
            yield return null;
            Assert.That(flow.State, Is.EqualTo(GameState.PlayerDown));
            Assert.That(flow.Lives.Lives, Is.EqualTo(livesBefore - 1));
            yield return WaitUntil(() => flow.State == GameState.Playing && player.IsAlive, config.RespawnDelay + 5f, "respawn");
            Assert.That(player.Health.Hull, Is.EqualTo(player.Health.MaxHull));

            // 14. Victory when the stage reports completion; progress is saved.
            GameSignals.RaiseStageCompleted();
            yield return WaitUntil(() => flow.State == GameState.Victory, 5f, "Victory state");
            yield return WaitUntil(() => Time.timeScale == 0f, 10f, "victory panel freeze");
            Assert.That(SaveService.Data.unlockedStage, Is.EqualTo(1));
            Assert.That(SaveService.Data.highScore, Is.GreaterThan(0));

            // 15. Back to the menu without errors.
            flow.ReturnToMenu();
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == "MainMenu", 15f, "MainMenu after victory");
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            yield return null;
            var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuController>();
            Assert.That(menu, Is.Not.Null);
            Assert.That(SaveService.Data.unlockedStage, Is.GreaterThan(0), "stage 2 unlocked after victory");
            Assert.That(SaveService.Data.credits, Is.GreaterThan(0), "credits rewarded");
        }

        [UnityTest]
        public IEnumerator Gameplay_GameOverAfterLosingAllLives()
        {
            GameSession.StartNewRun(0);
            SceneManager.LoadScene("Gameplay");
            yield return null;
            yield return null;
            var ctx = GameplayContext.Current;
            var flow = UnityEngine.Object.FindFirstObjectByType<GameFlowController>();
            yield return WaitUntil(() => flow.State == GameState.Briefing, 5f, "Briefing");
            ctx.Input.PressConfirm();
            yield return WaitUntil(() => flow.State == GameState.Playing, 5f, "Playing");

            int lives = flow.Lives.Lives;
            for (int i = 0; i < lives; i++)
            {
                yield return WaitUntil(() => flow.State == GameState.Playing && ctx.Player.IsAlive, 10f, $"alive before death {i + 1}");
                ctx.Player.Health.Kill(DamageSource.Boss);
                yield return null;
            }
            yield return WaitUntil(() => flow.State == GameState.GameOver, 10f, "GameOver state");
            Assert.That(flow.Lives.Lives, Is.EqualTo(0));
            Assert.That(Time.timeScale, Is.EqualTo(0f));
            Assert.That(ctx.StageDirector.IsRunning, Is.False);

            flow.RestartStage();
            yield return WaitUntil(() => GameplayContext.Current != null && GameplayContext.Current != ctx, 15f, "scene reloaded");
            yield return null;
            var newFlow = UnityEngine.Object.FindFirstObjectByType<GameFlowController>();
            yield return WaitUntil(() => newFlow.State == GameState.Briefing, 5f, "Briefing after restart");
            Assert.That(newFlow.Lives.Lives, Is.EqualTo(ctx.Config.StartingLives), "restart resets lives to stage start");
        }

        private static PowerUpDefinition FindPowerUp(GameConfig config, PowerUpKind kind)
        {
            foreach (var stage in config.Stages)
            {
                if (stage == null) continue;
                foreach (var ev in stage.Events)
                {
                    if (ev == null || ev.Wave == null) continue;
                    foreach (var entry in ev.Wave.Entries)
                    {
                        var table = entry != null && entry.Enemy != null ? entry.Enemy.DropTable : null;
                        if (table == null) continue;
                        foreach (var e in table.Entries)
                            if (e.PowerUp != null && e.PowerUp.Kind == kind) return e.PowerUp;
                    }
                }
            }
            return null;
        }

        private static Enemies.EnemyDefinition FirstEnemyDefinition(GameConfig config)
        {
            foreach (var ev in config.Stages[0].Events)
                if (ev != null && ev.Wave != null && ev.Wave.Entries.Length > 0 && ev.Wave.Entries[0].Enemy != null)
                    return ev.Wave.Entries[0].Enemy;
            return null;
        }
    }
}
