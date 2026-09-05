using Starfall.Core;
using Starfall.Logic;
using UnityEngine;

namespace Starfall.Scoring
{
    /// <summary>
    /// Owns the <see cref="ScoreModel"/> for the current stage and feeds it from game signals so enemies never
    /// talk to the score directly. Points are only granted once per kill because HealthModel dies once.
    /// </summary>
    public sealed class ScoreService : MonoBehaviour
    {
        [SerializeField] internal GameConfig config;

        private ScoreModel _model;

        public ScoreModel Model => _model ??= CreateModel();

        private ScoreModel CreateModel()
        {
            int max = config != null ? config.MaxMultiplier : 10;
            int step = config != null ? config.KillsPerMultiplierStep : 4;
            return new ScoreModel(max, step);
        }

        private void OnEnable()
        {
            GameSignals.EnemyDestroyed += OnEnemyDestroyed;
            GameSignals.PlayerDamaged += OnPlayerDamaged;
        }

        private void OnDisable()
        {
            GameSignals.EnemyDestroyed -= OnEnemyDestroyed;
            GameSignals.PlayerDamaged -= OnPlayerDamaged;
        }

        private void OnEnemyDestroyed(EnemyKilledInfo info)
        {
            if (!info.CountsForScore || info.Definition == null) return;
            Model.RegisterKill(info.Definition.ScoreValue);
        }

        private void OnPlayerDamaged(DamageInfo info, DamageResult result)
        {
            if (!result.Applied) return;
            Model.RegisterPlayerDamaged();
        }

        public void AddBonus(int points) => Model.AddBonus(points);

        /// <summary>Total for the run, including previous stages.</summary>
        public int RunTotal => GameSession.CarriedScore + Model.Score;
    }
}
