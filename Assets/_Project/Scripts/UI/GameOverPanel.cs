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
            if (scoreText != null) scoreText.text = Loc.F("FINAL SCORE  {0}", run.Score.ToString("N0"));
            if (detailText != null)
            {
                detailText.text = run.Mode == GameModeId.Survival || run.Mode == GameModeId.DailyChallenge
                    ? Loc.F("WAVES SURVIVED  {0}   ENEMIES  {1}", run.WavesSurvived, run.Kills)
                    : Loc.F("ENEMIES  {0}   BOSSES  {1}", run.Kills, run.BossKills);
            }
            if (rewardsText != null) rewardsText.text = VictoryPanel.FormatRewards(rewards);
            if (recordText != null)
            {
                recordText.gameObject.SetActive(isRecord || rank >= 0);
                recordText.text = isRecord ? Loc.T("NEW HIGH SCORE!") : rank >= 0 ? Loc.F("RANKING #{0}", rank + 1) : "";
            }
            if (restartButton != null)
            {
                var label = restartButton.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = Loc.T(run.Mode == GameModeId.Campaign ? "RETRY STAGE" : "PLAY AGAIN");
            }
            Show();
        }
    }
}
