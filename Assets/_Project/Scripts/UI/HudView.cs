using Starfall.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>Pure view: references to HUD widgets. Populated by <see cref="HudPresenter"/>.</summary>
    public sealed class HudView : MonoBehaviour
    {
        // Plan palette (§13.4)
        public static readonly Color AllyColor = new Color(0.14f, 0.84f, 1f);
        public static readonly Color AlertColor = new Color(1f, 0.82f, 0.4f);
        public static readonly Color DangerColor = new Color(1f, 0.48f, 0.21f);
        public static readonly Color ExtremeColor = new Color(1f, 0.24f, 0.67f);
        public static readonly Color CritColor = new Color(1f, 0.3f, 0.35f);

        [Header("Top left")]
        [SerializeField] internal TMP_Text livesText;
        [SerializeField] internal Image hullFill;
        [SerializeField] internal Image shieldFill;
        [SerializeField] internal TMP_Text criticalText;

        [Header("Top right")]
        [SerializeField] internal TMP_Text scoreText;
        [SerializeField] internal TMP_Text multiplierText;
        [SerializeField] internal TMP_Text waveText;

        [Header("Risk / Overdrive")]
        [SerializeField] internal TMP_Text riskText;
        [SerializeField] internal Image riskFill;
        [SerializeField] internal Image overdriveFill;
        [SerializeField] internal TMP_Text overdriveLabel;
        [SerializeField] internal TMP_Text grazeText;

        [Header("Bottom")]
        [SerializeField] internal Image energyFill;
        [SerializeField] internal TMP_Text energyLabel;
        [SerializeField] internal Button ultimateButton;
        [SerializeField] internal Image ultimateGlow;
        [SerializeField] internal TMP_Text weaponText;
        [SerializeField] internal Image chargeFill;
        [SerializeField] internal TMP_Text specialText;
        [SerializeField] internal Button pauseButton;

        [Header("Center")]
        [SerializeField] internal TMP_Text messageText;
        [SerializeField] internal GameObject bossGroup;
        [SerializeField] internal TMP_Text bossNameText;
        [SerializeField] internal Image bossFill;
        [SerializeField] internal Image bossTrailFill;
        [SerializeField] internal TMP_Text bossPercentText;

        private float _bossTarget = 1f;
        private float _bossTrail = 1f;

        private Color _hullColor = new Color(1f, 0.45f, 0.3f);

        public void SetLives(int lives) { if (livesText != null) livesText.text = $"x{lives}"; }
        public void SetHull(float f) { if (hullFill != null) hullFill.fillAmount = Mathf.Clamp01(f); }
        public void SetShield(float f) { if (shieldFill != null) shieldFill.fillAmount = Mathf.Clamp01(f); }

        public void SetCritical(bool critical, float blink)
        {
            if (criticalText != null)
            {
                criticalText.enabled = critical;
                if (critical) criticalText.color = new Color(CritColor.r, CritColor.g, CritColor.b, 0.5f + 0.5f * blink);
            }
            if (hullFill != null) hullFill.color = critical ? Color.Lerp(_hullColor, Color.white, blink * 0.6f) : _hullColor;
        }

        public void SetScore(int score) { if (scoreText != null) scoreText.text = score.ToString("N0"); }

        public void SetMultiplier(int m)
        {
            if (multiplierText == null) return;
            multiplierText.text = Loc.F("COMBO x{0}", m);
            multiplierText.color = m >= 10 ? AlertColor : m >= 5 ? new Color(0.6f, 1f, 0.8f) : Color.white;
        }

        public void SetWave(string text) { if (waveText != null) waveText.text = text; }

        public static Color RiskColor(RiskState state)
        {
            switch (state)
            {
                case RiskState.Alert: return AlertColor;
                case RiskState.Danger: return DangerColor;
                case RiskState.Extreme: return ExtremeColor;
                default: return AllyColor;
            }
        }

        public static string RiskLabel(RiskState state)
        {
            switch (state)
            {
                case RiskState.Alert: return "ALERT";
                case RiskState.Danger: return "DANGER";
                case RiskState.Extreme: return "EXTREME";
                default: return "SAFE";
            }
        }

        /// <summary>Risk state with its multiplier. The glyph count (!, !!, !!!) conveys the level without relying on color.</summary>
        public void SetRisk(RiskState state, float multiplier, float fraction, bool overdrive)
        {
            if (riskText != null)
            {
                string marks = state == RiskState.Safe ? "" : new string('!', (int)state);
                riskText.text = overdrive ? Loc.F("OVERDRIVE x{0}", multiplier.ToString("0.#")) : Loc.F("RISK {0}{1} x{2}", Loc.T(RiskLabel(state)), marks, multiplier.ToString("0.#"));
                riskText.color = overdrive ? Color.Lerp(AllyColor, ExtremeColor, 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 8f)) : RiskColor(state);
            }
            if (riskFill != null)
            {
                riskFill.fillAmount = Mathf.Clamp01(fraction);
                riskFill.color = overdrive ? AllyColor : RiskColor(state);
            }
        }

        public void SetOverdrive(float fraction, bool active, float remainingSeconds)
        {
            if (overdriveFill != null)
            {
                overdriveFill.fillAmount = Mathf.Clamp01(fraction);
                overdriveFill.color = active ? Color.Lerp(AllyColor, ExtremeColor, 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 10f)) : AllyColor;
            }
            if (overdriveLabel != null)
                overdriveLabel.text = active ? Loc.F("OVERDRIVE {0}s", remainingSeconds.ToString("0.0")) : fraction >= 0.999f ? Loc.T("OVERDRIVE READY") : Loc.T("OVERDRIVE");
        }

        public void SetGrazes(int grazes)
        {
            if (grazeText != null) grazeText.text = grazes > 0 ? Loc.F("GRAZE {0}", grazes) : "";
        }

        public void SetEnergy(float f, bool ready)
        {
            if (energyFill != null) energyFill.fillAmount = Mathf.Clamp01(f);
            if (energyLabel != null) energyLabel.text = ready ? Loc.T("ULTIMATE READY") : Loc.F("ENERGY {0}%", Mathf.RoundToInt(f * 100f));
            if (ultimateGlow != null) ultimateGlow.enabled = ready;
        }

        public void SetWeapon(string name, int level) { if (weaponText != null) weaponText.text = Loc.F("{0} LV.{1}", name, level); }

        public void SetCharge(float f)
        {
            if (chargeFill == null) return;
            chargeFill.enabled = f > 0f;
            chargeFill.fillAmount = Mathf.Clamp01(f);
        }

        public void SetSpecial(string text) { if (specialText != null) specialText.text = text; }

        public void SetMessage(string text, bool visible)
        {
            if (messageText == null) return;
            messageText.text = text;
            messageText.enabled = visible;
        }

        public void SetBoss(bool visible, string name, float fill)
        {
            bool wasVisible = bossGroup != null && bossGroup.activeSelf;
            if (bossGroup != null) bossGroup.SetActive(visible);
            if (bossNameText != null) bossNameText.text = name;
            _bossTarget = Mathf.Clamp01(fill);
            if (!wasVisible && visible) _bossTrail = _bossTarget;
            if (bossFill != null) bossFill.fillAmount = _bossTarget;
            if (bossPercentText != null) bossPercentText.text = visible ? Mathf.CeilToInt(_bossTarget * 100f) + "%" : "";
        }

        private void Update()
        {
            // Damage trail: the bright chunk behind the fill shrinks after a short delay so every hit reads clearly.
            if (bossTrailFill == null || bossGroup == null || !bossGroup.activeSelf) return;
            _bossTrail = _bossTrail > _bossTarget ? Mathf.MoveTowards(_bossTrail, _bossTarget, Time.unscaledDeltaTime * 0.35f) : _bossTarget;
            bossTrailFill.fillAmount = _bossTrail;
        }
    }
}
