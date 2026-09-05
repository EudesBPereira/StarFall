using Starfall.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>Stage results (GDD §16 Vitória): score, best multiplier, kills, damage, lives and rewards.</summary>
    public sealed class VictoryPanel : UiPanel
    {
        [SerializeField] internal TMP_Text titleText;
        [SerializeField] internal TMP_Text scoreText;
        [SerializeField] internal TMP_Text multiplierText;
        [SerializeField] internal TMP_Text enemiesText;
        [SerializeField] internal TMP_Text damageText;
        [SerializeField] internal TMP_Text livesText;
        [SerializeField] internal TMP_Text rewardsText;
        [SerializeField] internal TMP_Text recordText;
        [SerializeField] internal Button nextButton;
        [SerializeField] internal Button menuButton;

        public void Show(string title, RunStats run, ScoreModel score, int lives, in RunRewards rewards, bool isRecord, int rank, bool hasNext)
        {
            if (titleText != null) titleText.text = title;
            if (scoreText != null) scoreText.text = $"SCORE  {run.Score:N0}";
            if (multiplierText != null) multiplierText.text = $"BEST MULTIPLIER  x{score.HighestMultiplier}";
            if (enemiesText != null) enemiesText.text = run.Mode == GameModeId.BossRush ? $"BOSSES DESTROYED  {run.BossKills}" : $"ENEMIES DESTROYED  {score.EnemiesDestroyed}";
            if (damageText != null) damageText.text = $"HITS TAKEN  {score.DamageTakenCount}";
            if (livesText != null) livesText.text = $"LIVES LEFT  {lives}";
            if (rewardsText != null) rewardsText.text = FormatRewards(rewards);
            if (recordText != null)
            {
                recordText.gameObject.SetActive(isRecord || rank >= 0);
                recordText.text = isRecord ? "NEW HIGH SCORE!" : rank >= 0 ? $"RANKING #{rank + 1}" : "";
            }
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(hasNext);
                firstSelected = hasNext ? nextButton.gameObject : (menuButton != null ? menuButton.gameObject : null);
            }
            Show();
        }

        public static string FormatRewards(in RunRewards r) => $"+{r.Credits:N0} CREDITS   +{r.Xp} XP   +{r.Components} COMPONENTS";
    }
}
