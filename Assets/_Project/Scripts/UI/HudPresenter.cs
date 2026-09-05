using System.Collections;
using Starfall.Bosses;
using Starfall.Core;
using Starfall.Logic;
using Starfall.PowerUps;
using UnityEngine;

namespace Starfall.UI
{
    /// <summary>Binds gameplay models and signals to <see cref="HudView"/>. Contains no gameplay rules.</summary>
    public sealed class HudPresenter : MonoBehaviour
    {
        [SerializeField] internal HudView view;

        private GameplayContext _ctx;
        private LivesModel _lives;
        private BossController _boss;
        private Coroutine _messageRoutine;
        private bool _bound;

        public void Bind(GameplayContext ctx, LivesModel lives)
        {
            _ctx = ctx;
            _lives = lives;
            if (view == null || ctx == null) return;

            var score = ctx.Score.Model;
            score.ScoreChanged += OnScoreChanged;
            score.MultiplierChanged += OnMultiplierChanged;
            ctx.Player.Health.Changed += OnHealthChanged;
            ctx.Player.Ultimate.Energy.Changed += OnEnergyChanged;
            ctx.Player.Effects.Changed += OnEffectsChanged;
            lives.Changed += OnLivesChanged;

            GameSignals.WeaponLevelChanged += OnWeaponLevelChanged;
            GameSignals.StageMessage += OnStageMessage;
            GameSignals.BossSpawned += OnBossSpawned;
            GameSignals.BossDefeated += OnBossDefeated;
            GameSignals.PowerUpCollected += OnPowerUpCollected;

            if (view.ultimateButton != null) view.ultimateButton.onClick.AddListener(() => ctx.Input.PressUltimate());
            if (view.pauseButton != null) view.pauseButton.onClick.AddListener(() => ctx.Input.PressPause());

            _bound = true;
            OnScoreChanged(score.Score);
            OnMultiplierChanged(score.Multiplier);
            OnHealthChanged();
            OnEnergyChanged(ctx.Player.Ultimate.Energy.Current, ctx.Player.Ultimate.Energy.Max);
            OnLivesChanged(lives.Lives);
            OnWeaponLevelChanged(ctx.Player.Weapon.Level);
            view.SetSpecial("");
            view.SetMessage("", false);
            view.SetBoss(false, "", 0f);
        }

        private void OnDestroy()
        {
            if (!_bound) return;
            if (_ctx != null)
            {
                if (_ctx.Score != null)
                {
                    _ctx.Score.Model.ScoreChanged -= OnScoreChanged;
                    _ctx.Score.Model.MultiplierChanged -= OnMultiplierChanged;
                }
                if (_ctx.Player != null)
                {
                    if (_ctx.Player.Health != null) _ctx.Player.Health.Changed -= OnHealthChanged;
                    if (_ctx.Player.Ultimate != null) _ctx.Player.Ultimate.Energy.Changed -= OnEnergyChanged;
                    if (_ctx.Player.Effects != null) _ctx.Player.Effects.Changed -= OnEffectsChanged;
                }
            }
            if (_lives != null) _lives.Changed -= OnLivesChanged;
            GameSignals.WeaponLevelChanged -= OnWeaponLevelChanged;
            GameSignals.StageMessage -= OnStageMessage;
            GameSignals.BossSpawned -= OnBossSpawned;
            GameSignals.BossDefeated -= OnBossDefeated;
            GameSignals.PowerUpCollected -= OnPowerUpCollected;
        }

        private void Update()
        {
            if (!_bound) return;
            if (_boss != null && _boss.IsActiveInstance)
                view.SetBoss(true, _boss.Boss != null ? _boss.Boss.Title : "BOSS", _boss.Health.MaxHull > 0f ? _boss.Health.Hull / _boss.Health.MaxHull : 0f);

            var effects = _ctx.Player.Effects;
            var kind = effects.Tracker.Longest(out float remaining);
            view.SetSpecial(kind.HasValue ? $"{Label(kind.Value)} {remaining:0.0}s" : "");
        }

        private static string Label(PowerUpKind kind)
        {
            switch (kind)
            {
                case PowerUpKind.SpeedBoost: return "SPEED";
                case PowerUpKind.DamageBoost: return "DAMAGE";
                case PowerUpKind.Invincibility: return "INVINCIBLE";
                default: return kind.ToString().ToUpperInvariant();
            }
        }

        private void OnScoreChanged(int score) => view.SetScore(GameSession.CarriedScore + score);
        private void OnMultiplierChanged(int m) => view.SetMultiplier(m);
        private void OnLivesChanged(int lives) => view.SetLives(lives);
        private void OnEnergyChanged(float current, float max) => view.SetEnergy(max > 0f ? current / max : 0f, current >= max);
        private void OnEffectsChanged() { }

        private void OnHealthChanged()
        {
            var h = _ctx.Player.Health;
            view.SetHull(h.MaxHull > 0f ? h.Hull / h.MaxHull : 0f);
            view.SetShield(h.MaxShield > 0f ? h.Shield / h.MaxShield : 0f);
        }

        private void OnWeaponLevelChanged(int level)
        {
            var weapon = _ctx.Player.Weapon.Definition;
            view.SetWeapon(weapon != null ? weapon.DisplayName.ToUpperInvariant() : "LASER", level);
        }

        private void OnPowerUpCollected(PowerUpDefinition def)
        {
            if (def == null) return;
            OnStageMessage(def.DisplayName.ToUpperInvariant(), 0.8f);
        }

        private void OnStageMessage(string message, float seconds)
        {
            if (_messageRoutine != null) StopCoroutine(_messageRoutine);
            _messageRoutine = StartCoroutine(MessageRoutine(message, seconds));
        }

        private IEnumerator MessageRoutine(string message, float seconds)
        {
            view.SetMessage(message, true);
            yield return new WaitForSeconds(seconds);
            view.SetMessage("", false);
            _messageRoutine = null;
        }

        private void OnBossSpawned(BossController boss)
        {
            _boss = boss;
            view.SetBoss(true, boss.Boss != null ? boss.Boss.Title : "BOSS", 1f);
        }

        private void OnBossDefeated(BossController boss)
        {
            if (_boss == boss) _boss = null;
            view.SetBoss(false, "", 0f);
        }
    }
}
