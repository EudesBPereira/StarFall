using System.Collections;
using Starfall.Bosses;
using Starfall.Core;
using Starfall.Logic;
using Starfall.PowerUps;
using Starfall.Save;
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
        private bool _critical;
        private string _lastSpecial = "";
        private float _specialRefresh;
        private float _riskRefresh;
        private int _tutorialMask;

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
            lives.Changed += OnLivesChanged;

            GameSignals.WeaponLevelChanged += OnWeaponLevelChanged;
            GameSignals.StageMessage += OnStageMessage;
            GameSignals.BossSpawned += OnBossSpawned;
            GameSignals.BossDefeated += OnBossDefeated;
            GameSignals.PowerUpCollected += OnPowerUpCollected;
            GameSignals.PlayerCriticalChanged += OnCriticalChanged;
            GameSignals.WaveStarted += OnWaveStarted;
            GameSignals.ComponentCollected += OnComponentCollected;
            GameSignals.RiskStateChanged += OnRiskStateChanged;
            GameSignals.OverdriveChanged += OnOverdriveChanged;
            GameSignals.Graze += OnGraze;

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
            view.SetCritical(false, 0f);
            view.SetCharge(0f);
            view.SetRisk(RiskState.Safe, 1f, 0f, false);
            view.SetOverdrive(0f, false, 0f);
            view.SetGrazes(0);
            view.SetWave(GameSession.Mode == GameModeId.Campaign ? Loc.F("STAGE {0}", GameSession.CurrentStageIndex + 1) : "");
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
                }
            }
            if (_lives != null) _lives.Changed -= OnLivesChanged;
            GameSignals.WeaponLevelChanged -= OnWeaponLevelChanged;
            GameSignals.StageMessage -= OnStageMessage;
            GameSignals.BossSpawned -= OnBossSpawned;
            GameSignals.BossDefeated -= OnBossDefeated;
            GameSignals.PowerUpCollected -= OnPowerUpCollected;
            GameSignals.PlayerCriticalChanged -= OnCriticalChanged;
            GameSignals.WaveStarted -= OnWaveStarted;
            GameSignals.ComponentCollected -= OnComponentCollected;
            GameSignals.RiskStateChanged -= OnRiskStateChanged;
            GameSignals.OverdriveChanged -= OnOverdriveChanged;
            GameSignals.Graze -= OnGraze;
        }

        private void Update()
        {
            if (!_bound) return;
            if (_boss != null && _boss.IsActiveInstance)
                view.SetBoss(true, _boss.Boss != null ? Loc.T(_boss.Boss.Title) : Loc.T("BOSS"), _boss.Health.MaxHull > 0f ? _boss.Health.Hull / _boss.Health.MaxHull : 0f);

            if (_critical) view.SetCritical(true, 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 8f));
            view.SetCharge(_ctx.Player.Weapon.ChargeFraction);

            // Risk / Overdrive bars refresh 20x per second (they change continuously).
            _riskRefresh -= Time.unscaledDeltaTime;
            if (_riskRefresh <= 0f)
            {
                _riskRefresh = 0.05f;
                var sensor = _ctx.Player.Risk;
                if (sensor != null)
                {
                    view.SetRisk(sensor.Risk.State, sensor.Risk.Multiplier, sensor.Risk.Fraction, sensor.Overdrive.IsActive);
                    view.SetOverdrive(sensor.Overdrive.Fraction, sensor.Overdrive.IsActive, sensor.Overdrive.RemainingSeconds);
                }
            }

            // Special indicator refreshes 10x per second to avoid per-frame string allocation.
            _specialRefresh -= Time.unscaledDeltaTime;
            if (_specialRefresh <= 0f)
            {
                _specialRefresh = 0.1f;
                var effects = _ctx.Player.Effects;
                var kind = effects.Tracker.Longest(out float remaining);
                string text = kind.HasValue ? Loc.T(Label(kind.Value)) + " " + remaining.ToString("0.0") + "s" : "";
                if (text != _lastSpecial)
                {
                    _lastSpecial = text;
                    view.SetSpecial(text);
                }
            }
        }

        private static string Label(PowerUpKind kind)
        {
            switch (kind)
            {
                case PowerUpKind.SpeedBoost: return "SPEED";
                case PowerUpKind.DamageBoost: return "DAMAGE";
                case PowerUpKind.Invincibility: return "INVINCIBLE";
                case PowerUpKind.Slowed: return "SLOWED";
                default: return kind.ToString().ToUpperInvariant();
            }
        }

        private void OnScoreChanged(int score) => view.SetScore(GameSession.CarriedScore + score);
        private void OnMultiplierChanged(int m) => view.SetMultiplier(m);
        private void OnLivesChanged(int lives) => view.SetLives(lives);
        private void OnEnergyChanged(float current, float max) => view.SetEnergy(max > 0f ? current / max : 0f, current >= max);

        private void OnCriticalChanged(bool critical)
        {
            _critical = critical;
            view.SetCritical(critical, 1f);
        }

        private void OnWaveStarted(int wave)
        {
            if (GameSession.Mode == GameModeId.Campaign) view.SetWave(Loc.F("STAGE {0}  WAVE {1}", GameSession.CurrentStageIndex + 1, wave));
            else if (GameSession.Mode == GameModeId.BossRush) view.SetWave(Loc.F("BOSS {0}", wave));
            else view.SetWave(Loc.F("WAVE {0}", wave));
        }

        private void OnComponentCollected(int amount) => OnStageMessage(Loc.F("+{0} COMPONENT", amount), 0.9f);

        private void OnGraze(Vector2 position, float riskMultiplier)
        {
            var sensor = _ctx.Player.Risk;
            view.SetGrazes(sensor != null ? sensor.Grazes : 0);
            var data = SaveService.Data;
            if (data == null || data.grazeFeedback)
            {
                Audio.AudioManager.PlaySfx(Audio.SfxId.Graze, 0.6f);
                if (_ctx.Vfx != null) _ctx.Vfx.SpawnFloatingText(position + Vector2.up * 0.5f, Loc.T("GRAZE"), HudView.AllyColor, 0.45f);
            }
            TutorialHint(4, Loc.T("GRAZE! DODGE CLOSE TO BULLETS FOR BONUS POINTS AND OVERDRIVE"));
        }

        /// <summary>Contextual tutorial for the risk loop, shown once per hint on the first stage (plan §15.1).</summary>
        private void OnRiskStateChanged(RiskState from, RiskState to)
        {
            if (to <= from) return;
            switch (to)
            {
                case RiskState.Alert: TutorialHint(0, Loc.T("RISK ALERT x2 - CLOSER TO ENEMIES = MORE POINTS")); break;
                case RiskState.Danger: TutorialHint(1, Loc.T("DANGER x3 - OVERDRIVE IS CHARGING")); break;
                case RiskState.Extreme: TutorialHint(2, Loc.T("EXTREME x5 - MAXIMUM RISK")); break;
            }
        }

        private void OnOverdriveChanged(bool active)
        {
            if (active) TutorialHint(3, Loc.T("OVERDRIVE! SCORE UP TO x8 - DAMAGE SHORTENS IT"));
            else if (_tutorialMask != 0) OnStageMessage(Loc.T("OVERDRIVE ENDED"), 0.8f);
        }

        private void TutorialHint(int bit, string text)
        {
            if (GameSession.Mode != GameModeId.Campaign || GameSession.CurrentStageIndex != 0) return;
            if ((_tutorialMask & (1 << bit)) != 0) return;
            _tutorialMask |= 1 << bit;
            OnStageMessage(text, 2.4f);
        }

        private void OnHealthChanged()
        {
            var h = _ctx.Player.Health;
            view.SetHull(h.MaxHull > 0f ? h.Hull / h.MaxHull : 0f);
            view.SetShield(h.MaxShield > 0f ? h.Shield / h.MaxShield : 0f);
        }

        private void OnWeaponLevelChanged(int level)
        {
            var weapon = _ctx.Player.Weapon.Definition;
            view.SetWeapon(Loc.Upper(Loc.T(weapon != null ? weapon.DisplayName : "LASER")), level);
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
            view.SetBoss(true, boss.Boss != null ? Loc.T(boss.Boss.Title) : Loc.T("BOSS"), 1f);
        }

        private void OnBossDefeated(BossController boss)
        {
            if (_boss == boss) _boss = null;
            view.SetBoss(false, "", 0f);
        }
    }
}
