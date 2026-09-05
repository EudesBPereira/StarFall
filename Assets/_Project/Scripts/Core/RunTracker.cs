using Starfall.Logic;
using UnityEngine;

namespace Starfall.Core
{
    /// <summary>Feeds <see cref="GameSession.Run"/> from game signals (kills, hits, waves, components).</summary>
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
        }

        private void OnDisable()
        {
            GameSignals.EnemyDestroyed -= OnEnemyDestroyed;
            GameSignals.PlayerDamaged -= OnPlayerDamaged;
            GameSignals.PlayerDied -= OnPlayerDied;
            GameSignals.WaveStarted -= OnWaveStarted;
            GameSignals.BossDefeated -= OnBossDefeated;
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
    }
}
