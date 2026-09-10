using Starfall.Logic;
using Starfall.Player;
using Starfall.Waves;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>Stage intro shown before control is handed to the player (GDD §16 Briefing de Missão).</summary>
    public sealed class BriefingPanel : UiPanel
    {
        [SerializeField] internal TMP_Text titleText;
        [SerializeField] internal TMP_Text subtitleText;
        [SerializeField] internal TMP_Text bodyText;
        [SerializeField] internal TMP_Text objectivesText;
        [SerializeField] internal TMP_Text loadoutText;
        [SerializeField] internal TMP_Text hintText;
        [SerializeField] internal Button launchButton;

        public void Show(StageDefinition stage, GameModeId mode, int stageNumber, string hint, PlayerLoadout loadout)
        {
            string title;
            switch (mode)
            {
                case GameModeId.Survival: title = Loc.T("SURVIVAL"); break;
                case GameModeId.BossRush: title = Loc.T("BOSS RUSH"); break;
                case GameModeId.DailyChallenge: title = Loc.T("DAILY CHALLENGE"); break;
                default: title = stage != null ? Loc.F("STAGE {0}: {1}", stageNumber, Loc.Upper(Loc.T(stage.DisplayName))) : Loc.F("STAGE {0}", stageNumber); break;
            }
            if (titleText != null) titleText.text = title;
            if (subtitleText != null) subtitleText.text = mode == GameModeId.Campaign && stage != null ? Loc.T(stage.Subtitle) : "";
            if (bodyText != null)
            {
                switch (mode)
                {
                    case GameModeId.Survival: bodyText.text = Loc.T("Endless waves. Every 8th wave brings a mini-boss.\nHow long can the fleet hold?"); break;
                    case GameModeId.BossRush: bodyText.text = Loc.T("Every Swarm flagship, back to back.\nNo waves, no mercy."); break;
                    case GameModeId.DailyChallenge: bodyText.text = Loc.F("Today's seed: {0}\nFaster enemies, fewer drops. One attempt counts per day.", System.DateTime.Now.ToString("yyyy-MM-dd")); break;
                    default: bodyText.text = stage != null ? Loc.T(stage.Briefing) : ""; break;
                }
            }
            if (objectivesText != null)
                objectivesText.text = mode == GameModeId.Campaign && stage != null && !string.IsNullOrEmpty(stage.Objectives) ? Loc.T("OBJECTIVES\n") + Loc.T(stage.Objectives) : "";
            if (loadoutText != null)
            {
                string ship = loadout.Ship != null ? loadout.Ship.DisplayName : "Vanguard";
                string weapon = loadout.Weapon != null ? loadout.Weapon.DisplayName : "Laser";
                loadoutText.text = Loc.Upper(Loc.T(ship)) + "  |  " + Loc.Upper(Loc.T(weapon));
            }
            if (hintText != null) hintText.text = Loc.T(hint);
            Show();
        }
    }
}
