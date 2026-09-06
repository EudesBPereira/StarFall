using System;
using System.Collections;
using NUnit.Framework;
using Starfall.Combat;
using Starfall.Core;
using Starfall.Logic;
using Starfall.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Starfall.Tests.PlayMode
{
    /// <summary>End-to-end checks of the STAR RISK core loop (plan §5): Risk Zone, Overdrive and graze in the real scene.</summary>
    public class RiskLoopTests
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
                if (Time.realtimeSinceStartup > deadline) Assert.Fail($"Timed out waiting for: {what}");
                yield return null;
            }
        }

        [SetUp]
        public void SetUp()
        {
            SaveService.Override(new SaveRepository(new MemoryStorage(), new JsonUtilitySaveSerializer(), 5));
            SaveService.Repository.Load();
            GameSignals.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator RiskZone_RisesNearEnemies_AndScalesScore()
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
            ctx.StageDirector.Stop(); // isolate the test from the scripted waves

            var player = ctx.Player;
            Assert.That(player.Risk, Is.Not.Null, "RiskSensor on the player prefab");
            Assert.That(player.Risk.Risk.State, Is.EqualTo(RiskState.Safe));

            // Park several enemies right next to the ship (they can't move: no attack, no target).
            var drone = ctx.Config.ProceduralEnemies[0];
            var pos = (Vector2)player.transform.position;
            for (int i = 0; i < 4; i++)
            {
                var e = ctx.Spawner.Spawn(drone, pos + new Vector2(-0.9f + 0.6f * i, 0.9f));
                Assert.That(e, Is.Not.Null);
            }
            // Keep them close: re-pin every frame while the sensor samples.
            float until = Time.realtimeSinceStartup + 2.5f;
            var enemies = new System.Collections.Generic.List<Enemies.Enemy>();
            ctx.Enemies.CopyTo(enemies);
            while (Time.realtimeSinceStartup < until && player.Risk.Risk.State < RiskState.Danger)
            {
                for (int i = 0; i < enemies.Count; i++)
                    if (enemies[i] != null && enemies[i].IsActiveInstance)
                        enemies[i].transform.position = pos + new Vector2(-0.9f + 0.6f * i, 0.9f);
                yield return null;
            }
            Assert.That(player.Risk.Risk.State, Is.GreaterThanOrEqualTo(RiskState.Danger), "four adjacent enemies should reach Danger");
            float multiplier = player.Risk.Risk.Multiplier;
            Assert.That(multiplier, Is.GreaterThanOrEqualTo(3f));

            // A kill at this risk is worth base x combo x risk.
            int before = ctx.Score.Model.Score;
            enemies[0].Health.Kill(DamageSource.Player);
            yield return null;
            int awarded = ctx.Score.Model.Score - before;
            Assert.That(awarded, Is.EqualTo(Mathf.RoundToInt(drone.ScoreValue * 1 * multiplier)).Within(1), "kill scaled by risk multiplier");
            Assert.That(ctx.Score.Model.HighestRiskMultiplier, Is.GreaterThanOrEqualTo(3f));

            // Overdrive charges from Danger and eventually activates.
            yield return WaitUntil(() => player.Risk.Overdrive.Meter > 0f || player.Risk.Overdrive.IsActive, 3f, "overdrive charging");
            ctx.Enemies.DespawnAll();
        }

        [UnityTest]
        public IEnumerator Graze_AwardsOnce_AndNeverWhenInvulnerable()
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
            ctx.StageDirector.Stop();
            var player = ctx.Player;

            // Spawn invulnerability is active right after launch: a passing bullet must NOT graze.
            player.Health.Invulnerable = true;
            var spec = ProjectileSpec.Enemy(1f, 8f, 3f, Color.red, 0.6f);
            Vector2 origin = (Vector2)player.transform.position + new Vector2(0.55f, 1.6f);
            ProjectileLauncher.Fire(ctx.Pools, ctx.Spawner.EnemyProjectilePrefab, spec, origin, Vector2.down);
            float until = Time.realtimeSinceStartup + 1.5f;
            while (Time.realtimeSinceStartup < until) yield return null;
            Assert.That(ctx.Score.Model.Grazes, Is.EqualTo(0), "no graze while invulnerable");

            // Now vulnerable: a bullet passing 0.55 u to the side (outside the 0.28 hitbox, inside the 0.70 ring) grazes once.
            player.Health.Invulnerable = false;
            ctx.Input.ResetTransient();
            var p = ProjectileLauncher.Fire(ctx.Pools, ctx.Spawner.EnemyProjectilePrefab, spec, origin, Vector2.down);
            Assert.That(p, Is.Not.Null);
            yield return WaitUntil(() => ctx.Score.Model.Grazes >= 1, 3f, "graze awarded after the bullet leaves the ring");
            Assert.That(ctx.Score.Model.Grazes, Is.EqualTo(1));
            Assert.That(ctx.Score.Model.GrazePoints, Is.GreaterThanOrEqualTo(GrazeRules.BasePoints));
            Assert.That(p.Grazed || !p.IsLaunched, Is.True, "projectile flagged so it cannot graze twice");
            Assert.That(player.Health.Hull, Is.EqualTo(player.Health.MaxHull), "the bullet never hit the ship");
        }
    }
}
