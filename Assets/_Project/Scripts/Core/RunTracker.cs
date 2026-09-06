using Starfall.Logic;
using UnityEngine;

namespace Starfall.Core
{
    /// <summary>Feeds <see cref="GameSession.Run"/> from game signals (kills, hits, waves, components, risk, grazes).</summary>
    public sealed class RunTracker : MonoBehaviour
    {
        public RunStats Stats => GameSession.Run;

        private void OnEnable()
        {
            GameSignals.EnemyDestroyed += OnEnemyDestroyed;
            GameSignals.PlayerDamaged += OnPlayerDamaged;
            GameSignals.PlayerDied += OnPlayerDied;
            GameSignals.WaveStarted += OnWaveStarted;
            GameSignals.BossDefeated += OnBossDefeated;
            GameSignals.Graze += OnGraze;
            GameSignals.OverdriveChanged += OnOverdriveChanged;
            GameSignals.RiskStateChanged += OnRiskStateChanged;
        }

        private void OnDisable()
        {
            GameSignals.EnemyDestroyed -= OnEnemyDestroyed;
            GameSignals.PlayerDamaged -= OnPlayerDamaged;
            GameSignals.PlayerDied -= OnPlayerDied;
            GameSignals.WaveStarted -= OnWaveStarted;
            GameSignals.BossDefeated -= OnBossDefeated;
            GameSignals.Graze -= OnGraze;
            GameSignals.OverdriveChanged -= OnOverdriveChanged;
            GameSignals.RiskStateChanged -= OnRiskStateChanged;
        }

        private void Update()
        {
            var ctx = GameplayContext.Current;
            if (ctx == null || ctx.Player == null || ctx.Player.Risk == null) return;
            if (Time.timeScale <= 0f) return;
            var risk = ctx.Player.Risk.Risk;
            if (risk.State >= RiskState.Danger) Stats.SecondsInDanger += Time.deltaTime;
        }

        private void OnEnemyDestroyed(EnemyKilledInfo info)
        {
            if (!info.CountsForScore || info.Definition == null || info.Definition.IsObstacle) return;
            var run = Stats;
            run.Kills++;
            if (info.Definition.IsElite && !info.IsBoss) run.EliteKills++;
            if (info.IsBoss) run.BossKills++;
            if (info.Definition.ComponentReward > 0)
            {
                run.ComponentsCollected += info.Definition.ComponentReward;
                GameSignals.RaiseComponentCollected(info.Definition.ComponentReward);
            }
        }

        private void OnPlayerDamaged(DamageInfo info, DamageResult result)
        {
            if (result.Applied) Stats.HitsTaken++;
        }

        private void OnPlayerDied() => Stats.LivesLost++;

        private void OnWaveStarted(int wave) => Stats.WavesSurvived = Mathf.Max(Stats.WavesSurvived, wave);

        private void OnBossDefeated(Bosses.BossController boss)
        {
            if (boss != null && boss.Boss != null) Stats.BossesDefeatedMask |= 1 << (int)boss.Boss.BossId;
        }

        private void OnGraze(Vector2 position, float riskMultiplier) => Stats.Grazes++;

        private void OnOverdriveChanged(bool active)
        {
            if (active) Stats.OverdriveActivations++;
        }

        private void OnRiskStateChanged(RiskState from, RiskState to)
        {
            var ctx = GameplayContext.Current;
            if (ctx == null || ctx.Player == null || ctx.Player.Risk == null) return;
            float m = ctx.Player.Risk.Risk.MultiplierFor(to);
            if (m > Stats.HighestRiskMultiplier) Stats.HighestRiskMultiplier = m;
        }
    }
}
