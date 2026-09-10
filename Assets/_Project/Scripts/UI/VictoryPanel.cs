using Starfall.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>Stage results (plan §9.2, §9.3): score breakdown, rank, best risk, grazes, lives and rewards.</summary>
    public sealed class VictoryPanel : UiPanel
    {
        [SerializeField] internal TMP_Text titleText;
        [SerializeField] internal TMP_Text rankText;
        [SerializeField] internal TMP_Text scoreText;
        [SerializeField] internal TMP_Text breakdownText;
        [SerializeField] internal TMP_Text multiplierText;
        [SerializeField] internal TMP_Text enemiesText;
        [SerializeField] internal TMP_Text damageText;
        [SerializeField] internal TMP_Text livesText;
        [SerializeField] internal TMP_Text rewardsText;
        [SerializeField] internal TMP_Text recordText;
        [SerializeField] internal Button nextButton;
        [SerializeField] internal Button menuButton;

        public void Show(string title, RunStats run, ScoreModel score, in StageResult result, int lives, in RunRewards rewards, bool isRecord, int rank, bool hasNext)
        {
            if (titleText != null) titleText.text = Loc.T(title);
            if (rankText != null)
            {
                rankText.text = run.Mode == GameModeId.Campaign ? Loc.F("RANK {0}", StageResultRules.RankLabel(result.Rank)) : "";
                rankText.color = RankColor(result.Rank);
            }
            if (scoreText != null) scoreText.text = Loc.F("SCORE  {0}", result.Total.ToString("N0"));
            if (breakdownText != null)
            {
                breakdownText.text =
                    Loc.F("KILLS {0}   GRAZE {1}   OBJECTIVE {2}\n", result.KillPoints.ToString("N0"), result.GrazePoints.ToString("N0"), result.ObjectiveBonus.ToString("N0")) +
                    Loc.F("TIME {0}   NO DAMAGE {1}", result.TimeBonus.ToString("N0"), result.NoDamageBonus.ToString("N0"));
            }
            if (multiplierText != null) multiplierText.text = Loc.F("BEST COMBO x{0}   BEST RISK x{1}   GRAZES {2}", score.HighestMultiplier, score.HighestRiskMultiplier.ToString("0.#"), score.Grazes);
            if (enemiesText != null) enemiesText.text = run.Mode == GameModeId.BossRush ? Loc.F("BOSSES DESTROYED  {0}", run.BossKills) : Loc.F("ENEMIES DESTROYED  {0}", score.EnemiesDestroyed);
            if (damageText != null) damageText.text = Loc.F("HITS TAKEN  {0}   OVERDRIVES  {1}", score.DamageTakenCount, run.OverdriveActivations);
            if (livesText != null) livesText.text = Loc.F("LIVES LEFT  {0}", lives);
            if (rewardsText != null) rewardsText.text = FormatRewards(rewards);
            if (recordText != null)
            {
                recordText.gameObject.SetActive(isRecord || rank >= 0);
                recordText.text = isRecord ? Loc.T("NEW HIGH SCORE!") : rank >= 0 ? Loc.F("RANKING #{0}", rank + 1) : "";
            }
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(hasNext);
                firstSelected = hasNext ? nextButton.gameObject : (menuButton != null ? menuButton.gameObject : null);
            }
            Show();
        }

        public static Color RankColor(StageRank rank)
        {
            switch (rank)
            {
                case StageRank.SSS: return new Color(1f, 0.24f, 0.67f);
                case StageRank.SS: return new Color(1f, 0.48f, 0.21f);
                case StageRank.S: return new Color(1f, 0.82f, 0.4f);
                case StageRank.A: return new Color(0.36f, 1f, 0.62f);
                case StageRank.B: return new Color(0.14f, 0.84f, 1f);
                case StageRank.C: return new Color(0.75f, 0.85f, 0.95f);
                default: return new Color(0.6f, 0.65f, 0.7f);
            }
        }

        public static string FormatRewards(in RunRewards r)
        {
            string detail = r.FirstClearCredits > 0 ? Loc.F("  (BASE {0} + RANK {1} + RISK {2} + FIRST CLEAR {3})", r.BaseCredits, r.RankCredits, r.RiskCredits, r.FirstClearCredits)
                : r.RankCredits > 0 || r.RiskCredits > 0 ? Loc.F("  (BASE {0} + RANK {1} + RISK {2})", r.BaseCredits, r.RankCredits, r.RiskCredits) : "";
            return Loc.F("+{0} CREDITS{1}\n+{2} XP   +{3} COMPONENTS", r.Credits.ToString("N0"), detail, r.Xp, r.Components);
        }
    }
}
