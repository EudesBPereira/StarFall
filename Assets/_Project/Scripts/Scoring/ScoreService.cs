using Starfall.Core;
using Starfall.Logic;
using UnityEngine;

namespace Starfall.Scoring
{
    /// <summary>
    /// Owns the <see cref="ScoreModel"/> for the current stage and feeds it from game signals so enemies never
    /// talk to the score directly. Kills are scaled by the player's current Risk multiplier (plan §9.2) and
    /// grazes add flat, risk-scaled points. Points are only granted once per kill because HealthModel dies once.
    /// </summary>
    public sealed class ScoreService : MonoBehaviour
    {
        [SerializeField] internal GameConfig config;

        private ScoreModel _model;
        private float _elapsed;
        private bool _timing;

        public ScoreModel Model => _model ??= CreateModel();
        /// <summary>Seconds of gameplay (scaled time) since the stage started; drives the time bonus.</summary>
        public float Elapsed => _elapsed;

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
            GameSignals.Graze += OnGraze;
            GameSignals.GameStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            GameSignals.EnemyDestroyed -= OnEnemyDestroyed;
            GameSignals.PlayerDamaged -= OnPlayerDamaged;
            GameSignals.Graze -= OnGraze;
            GameSignals.GameStateChanged -= OnGameStateChanged;
        }

        private void Update()
        {
            if (_timing) _elapsed += Time.deltaTime;
        }

        private void OnGameStateChanged(GameState from, GameState to)
        {
            _timing = to == GameState.Playing || to == GameState.PlayerDown;
        }

        private float CurrentRiskMultiplier
        {
            get
            {
                var ctx = GameplayContext.Current;
                return ctx != null && ctx.Player != null && ctx.Player.Risk != null ? ctx.Player.Risk.Risk.Multiplier : 1f;
            }
        }

        private void OnEnemyDestroyed(EnemyKilledInfo info)
        {
            if (!info.CountsForScore || info.Definition == null) return;
            float risk = CurrentRiskMultiplier;
            int awarded = Model.RegisterKill(info.Definition.ScoreValue, risk);
            var ctx = GameplayContext.Current;
            if (ctx != null && ctx.Player != null && ctx.Player.Risk != null)
                ctx.Player.Risk.OnEnemyKilled(info.Position, info.IsBoss);
            if (awarded > 0 && risk >= 3f && ctx != null && ctx.Vfx != null)
                ctx.Vfx.SpawnFloatingText(info.Position + Vector2.up * 0.4f, $"+{awarded:N0}", risk >= 5f ? new Color(1f, 0.24f, 0.67f) : new Color(1f, 0.48f, 0.21f), 0.7f);
        }

        private void OnGraze(Vector2 position, float riskMultiplier)
        {
            Model.RegisterGraze(riskMultiplier);
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
