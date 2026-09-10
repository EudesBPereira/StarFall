using System.Collections.Generic;
using System.Text;
using Starfall.Core;
using Starfall.Logic;
using Starfall.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>Local leaderboard per mode plus the achievement list (GDD §16, §22). No network access (D-017).</summary>
    public sealed class RankingPanel : MenuPanel
    {
        [SerializeField] internal Button[] modeTabs;
        [SerializeField] internal ListRow[] rows;
        [SerializeField] internal TMP_Text emptyText;
        [SerializeField] internal TMP_Text achievementsText;

        private GameModeId _mode = GameModeId.Campaign;
        private readonly List<LeaderboardEntry> _buffer = new List<LeaderboardEntry>(10);
        private readonly StringBuilder _sb = new StringBuilder(512);

        public override void Initialize(GameConfig config)
        {
            base.Initialize(config);
            for (int i = 0; i < modeTabs.Length; i++)
            {
                int mode = i;
                if (modeTabs[i] != null) modeTabs[i].onClick.AddListener(() => { _mode = (GameModeId)mode; Refresh(); });
            }
        }

        public override void Show()
        {
            Refresh();
            base.Show();
        }

        private void Refresh()
        {
            SaveService.LeaderboardFor(_mode, _buffer);
            for (int i = 0; i < rows.Length; i++)
            {
                var row = rows[i];
                if (row == null) continue;
                bool has = i < _buffer.Count;
                row.gameObject.SetActive(has);
                if (!has) continue;
                var e = _buffer[i];
                string ship = e.ship >= 0 && e.ship < ProgressionRules.ShipCount ? ((ShipId)e.ship).ToString().ToUpperInvariant() : "";
                string detail = _mode == GameModeId.Campaign ? Loc.F("STAGE {0}  {1}  {2}", e.stage + 1, ship, e.date)
                    : _mode == GameModeId.BossRush ? $"{ship}  {e.date}"
                    : Loc.F("WAVE {0}  {1}  {2}", e.wave, ship, e.date);
                row.Set($"#{i + 1}   {e.score:N0}", detail, "", false, false);
            }
            if (emptyText != null) emptyText.gameObject.SetActive(_buffer.Count == 0);
            for (int i = 0; i < modeTabs.Length; i++)
                if (modeTabs[i] != null) modeTabs[i].interactable = i != (int)_mode;

            var save = SaveService.Data;
            if (achievementsText != null && save != null)
            {
                _sb.Clear();
                _sb.Append(Loc.T("ACHIEVEMENTS\n"));
                for (int i = 0; i < AchievementRules.Count; i++)
                {
                    var id = (AchievementId)i;
                    _sb.Append(save.HasAchievement(i) ? "[X] " : "[ ] ")
                       .Append(Loc.T(AchievementRules.DisplayName(id)))
                       .Append(" - ")
                       .Append(Loc.T(AchievementRules.Description(id)))
                       .Append('\n');
                }
                achievementsText.text = _sb.ToString();
            }
        }
    }
}
