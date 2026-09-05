using Starfall.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>Game over screen (GDD §16 Derrota): final score, run summary, rewards, retry or menu.</summary>
    public sealed class GameOverPanel : UiPanel
    {
        [SerializeField] internal TMP_Text scoreText;
        [SerializeField] internal TMP_Text detailText;
        [SerializeField] internal TMP_Text rewardsText;
        [SerializeField] internal TMP_Text recordText;
        [SerializeField] internal Button restartButton;
        [SerializeField] internal Button menuButton;

        public void Show(RunStats run, in RunRewards rewards, bool isRecord, int rank)
        {
            if (scoreText != null) scoreText.text = $"FINAL SCORE  {run.Score:N0}";
            if (detailText != null)
            {
                detailText.text = run.Mode == GameModeId.Survival || run.Mode == GameModeId.DailyChallenge
                    ? $"WAVES SURVIVED  {run.WavesSurvived}   ENEMIES  {run.Kills}"
                    : $"ENEMIES  {run.Kills}   BOSSES  {run.BossKills}";
            }
            if (rewardsText != null) rewardsText.text = VictoryPanel.FormatRewards(rewards);
            if (recordText != null)
            {
                recordText.gameObject.SetActive(isRecord || rank >= 0);
                recordText.text = isRecord ? "NEW HIGH SCORE!" : rank >= 0 ? $"RANKING #{rank + 1}" : "";
            }
            if (restartButton != null)
            {
                var label = restartButton.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = run.Mode == GameModeId.Campaign ? "RETRY STAGE" : "PLAY AGAIN";
            }
            Show();
        }
    }
}
