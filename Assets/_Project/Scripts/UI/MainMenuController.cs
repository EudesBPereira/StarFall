using Starfall.Audio;
using Starfall.Core;
using Starfall.Logic;
using Starfall.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>Main menu flow (GDD §16): Play / Hangar / Upgrades / Ranking / Settings / Credits / Quit.</summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] internal GameConfig config;
        [SerializeField] internal AudioLibrary audioLibrary;
        [SerializeField] internal UiPanel mainPanel;
        [SerializeField] internal MissionPanel missionPanel;
        [SerializeField] internal HangarPanel hangarPanel;
        [SerializeField] internal UpgradesPanel upgradesPanel;
        [SerializeField] internal RankingPanel rankingPanel;
        [SerializeField] internal SettingsPanel settingsPanel;
        [SerializeField] internal UiPanel creditsPanel;
        [SerializeField] internal Button playButton;
        [SerializeField] internal Button hangarButton;
        [SerializeField] internal Button upgradesButton;
        [SerializeField] internal Button rankingButton;
        [SerializeField] internal Button settingsButton;
        [SerializeField] internal Button creditsButton;
        [SerializeField] internal Button quitButton;
        [SerializeField] internal Button creditsBackButton;
        [SerializeField] internal TMP_Text highScoreText;
        [SerializeField] internal TMP_Text pilotText;
        [SerializeField] internal TMP_Text versionText;

        private void Awake()
        {
            SaveService.EnsureInitialized(config != null ? config.StageCount : 5);
            AudioManager.Ensure(audioLibrary);
        }

        private void Start()
        {
            Time.timeScale = 1f;
            AudioManager.Instance.PlayMusic(MusicId.Menu);
            AudioManager.Instance.PlayAmbient(AmbientId.Space);

            Wire(playButton, () => Open(missionPanel));
            Wire(hangarButton, () => Open(hangarPanel));
            Wire(upgradesButton, () => Open(upgradesPanel));
            Wire(rankingButton, () => Open(rankingPanel));
            Wire(settingsButton, () => Open(settingsPanel));
            Wire(creditsButton, () => Open(creditsPanel));
            Wire(creditsBackButton, ShowMain);
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(Quit);
#if UNITY_IOS
                quitButton.gameObject.SetActive(false);
#endif
            }
            if (missionPanel != null) { missionPanel.Initialize(config); missionPanel.Closed += ShowMain; }
            if (hangarPanel != null) { hangarPanel.Initialize(config); hangarPanel.Closed += ShowMain; }
            if (upgradesPanel != null) { upgradesPanel.Initialize(config); upgradesPanel.Closed += ShowMain; }
            if (rankingPanel != null) { rankingPanel.Initialize(config); rankingPanel.Closed += ShowMain; }
            if (settingsPanel != null) settingsPanel.Closed += ShowMain;
            if (versionText != null) versionText.text = $"v{Application.version}";

            HideAll();
            ShowMain();
            AchievementService.Evaluate(null, config != null ? config.StageCount : 5, config != null ? config.BossCount : 4);
        }

        private void OnDestroy()
        {
            if (missionPanel != null) missionPanel.Closed -= ShowMain;
            if (hangarPanel != null) hangarPanel.Closed -= ShowMain;
            if (upgradesPanel != null) upgradesPanel.Closed -= ShowMain;
            if (rankingPanel != null) rankingPanel.Closed -= ShowMain;
            if (settingsPanel != null) settingsPanel.Closed -= ShowMain;
        }

        private static void Wire(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null) button.onClick.AddListener(action);
        }

        private void HideAll()
        {
            if (mainPanel != null) mainPanel.Hide();
            if (missionPanel != null) missionPanel.Hide();
            if (hangarPanel != null) hangarPanel.Hide();
            if (upgradesPanel != null) upgradesPanel.Hide();
            if (rankingPanel != null) rankingPanel.Hide();
            if (settingsPanel != null) settingsPanel.Hide();
            if (creditsPanel != null) creditsPanel.Hide();
        }

        private void Open(UiPanel panel)
        {
            if (panel == null) return;
            if (mainPanel != null) mainPanel.Hide();
            panel.Show();
        }

        private void ShowMain()
        {
            RefreshMain();
            if (mainPanel != null) mainPanel.Show();
        }

        private void RefreshMain()
        {
            var data = SaveService.Data;
            if (highScoreText != null) highScoreText.text = data != null ? $"HIGH SCORE  {data.highScore:N0}" : "";
            if (pilotText != null && data != null)
                pilotText.text = $"PILOT LV.{ProgressionRules.PilotLevel(data.xp)}   {data.credits:N0} CR   {data.components} PARTS";
        }

        private void Quit()
        {
            SaveService.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
