using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>Pure view: references to HUD widgets. Populated by <see cref="HudPresenter"/>.</summary>
    public sealed class HudView : MonoBehaviour
    {
        [Header("Top left")]
        [SerializeField] internal TMP_Text livesText;
        [SerializeField] internal Image hullFill;
        [SerializeField] internal Image shieldFill;

        [Header("Top right")]
        [SerializeField] internal TMP_Text scoreText;
        [SerializeField] internal TMP_Text multiplierText;

        [Header("Bottom")]
        [SerializeField] internal Image energyFill;
        [SerializeField] internal TMP_Text energyLabel;
        [SerializeField] internal Button ultimateButton;
        [SerializeField] internal Image ultimateGlow;
        [SerializeField] internal TMP_Text weaponText;
        [SerializeField] internal TMP_Text specialText;
        [SerializeField] internal Button pauseButton;

        [Header("Center")]
        [SerializeField] internal TMP_Text messageText;
        [SerializeField] internal GameObject bossGroup;
        [SerializeField] internal TMP_Text bossNameText;
        [SerializeField] internal Image bossFill;

        public void SetLives(int lives) { if (livesText != null) livesText.text = $"x{lives}"; }
        public void SetHull(float f) { if (hullFill != null) hullFill.fillAmount = Mathf.Clamp01(f); }
        public void SetShield(float f) { if (shieldFill != null) shieldFill.fillAmount = Mathf.Clamp01(f); }
        public void SetScore(int score) { if (scoreText != null) scoreText.text = score.ToString("N0"); }
        public void SetMultiplier(int m)
        {
            if (multiplierText == null) return;
            multiplierText.text = $"x{m}";
            multiplierText.color = m >= 10 ? new Color(1f, 0.85f, 0.2f) : m >= 5 ? new Color(0.6f, 1f, 0.8f) : Color.white;
        }
        public void SetEnergy(float f, bool ready)
        {
            if (energyFill != null) energyFill.fillAmount = Mathf.Clamp01(f);
            if (energyLabel != null) energyLabel.text = ready ? "ULTIMATE READY" : $"ENERGY {Mathf.RoundToInt(f * 100f)}%";
            if (ultimateGlow != null) ultimateGlow.enabled = ready;
        }
        public void SetWeapon(string name, int level) { if (weaponText != null) weaponText.text = $"{name} LV.{level}"; }
        public void SetSpecial(string text) { if (specialText != null) specialText.text = text; }
        public void SetMessage(string text, bool visible)
        {
            if (messageText == null) return;
            messageText.text = text;
            messageText.enabled = visible;
        }
        public void SetBoss(bool visible, string name, float fill)
        {
            if (bossGroup != null) bossGroup.SetActive(visible);
            if (bossNameText != null) bossNameText.text = name;
            if (bossFill != null) bossFill.fillAmount = Mathf.Clamp01(fill);
        }
    }
}
