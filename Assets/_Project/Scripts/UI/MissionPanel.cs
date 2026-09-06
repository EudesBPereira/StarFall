using System;
using Starfall.Core;
using Starfall.Logic;
using Starfall.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>Mission select: campaign stages plus the extra modes (GDD §23).</summary>
    public sealed class MissionPanel : MenuPanel
    {
        [SerializeField] internal ListRow[] stageRows;
        [SerializeField] internal Button survivalButton;
        [SerializeField] internal Button bossRushButton;
        [SerializeField] internal Button dailyButton;
        [SerializeField] internal TMP_Text modeInfoText;

        public override void Initialize(GameConfig config)
        {
            base.Initialize(config);
            for (int i = 0; i < stageRows.Length; i++)
            {
                int index = i;
                if (stageRows[i] != null && stageRows[i].button != null)
                    stageRows[i].button.onClick.AddListener(() => StartCampaign(index));
            }
            if (survivalButton != null) survivalButton.onClick.AddListener(() => StartMode(GameModeId.Survival));
            if (bossRushButton != null) bossRushButton.onClick.AddListener(() => StartMode(GameModeId.BossRush));
            if (dailyButton != null) dailyButton.onClick.AddListener(() => StartMode(GameModeId.DailyChallenge));
        }

        public override void Show()
        {
            Refresh();
            base.Show();
        }

        private void Refresh()
        {
            var save = SaveService.Data;
            if (Config == null || save == null) return;
            for (int i = 0; i < stageRows.Length; i++)
            {
                var row = stageRows[i];
                if (row == null) continue;
                var stage = i < Config.Stages.Length ? Config.Stages[i] : null;
                bool unlocked = i <= save.unlockedStage && stage != null;
                bool completed = save.IsStageCompleted(i);
                string title = stage != null ? $"{i + 1}. {stage.DisplayName.ToUpperInvariant()}" : $"{i + 1}. ???";
                int slot = Mathf.Min(i, save.stageBestScores.Length - 1);
                string rank = completed && save.stageBestRanks != null && slot < save.stageBestRanks.Length ? StageResultRules.RankLabel((StageRank)save.stageBestRanks[slot]) : "";
                string bosses = stage != null && !string.IsNullOrEmpty(stage.BossName) ? $"  {stage.BossName.ToUpperInvariant()}" : "";
                string detail = !unlocked ? "LOCKED" : completed ? $"RANK {rank}  BEST {save.stageBestScores[slot]:N0}{bosses}" : "NEW" + bosses;
                row.Set(title, detail, "", unlocked, i == save.unlockedStage && !completed);
                row.gameObject.SetActive(stage != null);
            }
            bool campaignDone = ProgressionRules.AllStagesCompleted(save, Config.StageCount);
            if (bossRushButton != null) bossRushButton.interactable = save.bossesDefeatedMask != 0;
            if (modeInfoText != null)
            {
                string daily = save.dailyDate == DateTime.Now.ToString("yyyy-MM-dd") ? $"today best {save.dailyBestScore:N0}" : "not played today";
                modeInfoText.text = $"SURVIVAL best wave {save.survivalBestWave}   |   BOSS RUSH best {save.bossRushBestScore:N0}   |   DAILY {daily}" +
                                    (campaignDone ? "\nCAMPAIGN COMPLETE" : "") + (save.bossesDefeatedMask == 0 ? "\nBoss Rush unlocks after defeating a boss." : "");
            }
            if (stageRows.Length > 0)
            {
                int focus = Mathf.Clamp(save.unlockedStage, 0, stageRows.Length - 1);
                firstSelected = stageRows[focus] != null ? stageRows[focus].gameObject : (backButton != null ? backButton.gameObject : null);
            }
        }

        private void StartCampaign(int stageIndex)
        {
            SaveService.Save();
            GameSession.StartNewRun(stageIndex);
            SceneLoader.LoadGameplay(Config);
        }

        private void StartMode(GameModeId mode)
        {
            SaveService.Save();
            if (mode == GameModeId.DailyChallenge) GameSession.StartDaily(DateTime.Now);
            else GameSession.StartNewRun(mode, 0, Environment.TickCount);
            SceneLoader.LoadGameplay(Config);
        }
    }
}
