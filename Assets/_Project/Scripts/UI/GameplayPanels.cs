using Starfall.Waves;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>Stage intro shown before control is handed to the player.</summary>
    public sealed class BriefingPanel : UiPanel
    {
        [SerializeField] internal TMP_Text titleText;
        [SerializeField] internal TMP_Text subtitleText;
        [SerializeField] internal TMP_Text bodyText;
        [SerializeField] internal TMP_Text hintText;
        [SerializeField] internal Button launchButton;

        public void Show(StageDefinition stage, int stageNumber, string hint)
        {
            if (titleText != null) titleText.text = stage != null ? $"STAGE {stageNumber}: {stage.DisplayName.ToUpperInvariant()}" : $"STAGE {stageNumber}";
            if (subtitleText != null) subtitleText.text = stage != null ? stage.Subtitle : "";
            if (bodyText != null) bodyText.text = stage != null ? stage.Briefing : "";
            if (hintText != null) hintText.text = hint;
            Show();
        }
    }

    public sealed class PausePanel : UiPanel
    {
        [SerializeField] internal Button continueButton;
        [SerializeField] internal Button settingsButton;
        [SerializeField] internal Button restartButton;
        [SerializeField] internal Button menuButton;
    }

    public sealed class VictoryPanel : UiPanel
    {
        [SerializeField] internal TMP_Text titleText;
        [SerializeField] internal TMP_Text scoreText;
        [SerializeField] internal TMP_Text multiplierText;
        [SerializeField] internal TMP_Text enemiesText;
        [SerializeField] internal TMP_Text damageText;
        [SerializeField] internal TMP_Text livesText;
        [SerializeField] internal TMP_Text recordText;
        [SerializeField] internal Button nextButton;
        [SerializeField] internal Button menuButton;

        public void Show(string title, int score, int highestMultiplier, int enemies, int damageTaken, int lives, bool isRecord, bool hasNext)
        {
            if (titleText != null) titleText.text = title;
            if (scoreText != null) scoreText.text = $"SCORE  {score:N0}";
            if (multiplierText != null) multiplierText.text = $"BEST MULTIPLIER  x{highestMultiplier}";
            if (enemiesText != null) enemiesText.text = $"ENEMIES DESTROYED  {enemies}";
            if (damageText != null) damageText.text = $"HITS TAKEN  {damageTaken}";
            if (livesText != null) livesText.text = $"LIVES LEFT  {lives}";
            if (recordText != null) recordText.gameObject.SetActive(isRecord);
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(hasNext);
                firstSelected = hasNext ? nextButton.gameObject : (menuButton != null ? menuButton.gameObject : null);
            }
            Show();
        }
    }

    public sealed class GameOverPanel : UiPanel
    {
        [SerializeField] internal TMP_Text scoreText;
        [SerializeField] internal TMP_Text recordText;
        [SerializeField] internal Button restartButton;
        [SerializeField] internal Button menuButton;

        public void Show(int score, bool isRecord)
        {
            if (scoreText != null) scoreText.text = $"FINAL SCORE  {score:N0}";
            if (recordText != null) recordText.gameObject.SetActive(isRecord);
            Show();
        }
    }
}
