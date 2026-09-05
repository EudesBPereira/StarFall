using Starfall.Audio;
using Starfall.Core;
using Starfall.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>Main menu flow: Play / Continue / Settings / Credits / Quit.</summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] internal GameConfig config;
        [SerializeField] internal AudioLibrary audioLibrary;
        [SerializeField] internal UiPanel mainPanel;
        [SerializeField] internal SettingsPanel settingsPanel;
        [SerializeField] internal UiPanel creditsPanel;
        [SerializeField] internal Button playButton;
        [SerializeField] internal Button continueButton;
        [SerializeField] internal Button settingsButton;
        [SerializeField] internal Button creditsButton;
        [SerializeField] internal Button quitButton;
        [SerializeField] internal Button creditsBackButton;
        [SerializeField] internal TMP_Text highScoreText;
        [SerializeField] internal TMP_Text continueLabel;
        [SerializeField] internal TMP_Text versionText;

        private void Awake()
        {
            SaveService.EnsureInitialized(config != null ? config.StageCount : 3);
            AudioManager.Ensure(audioLibrary);
        }

        private void Start()
        {
            Time.timeScale = 1f;
            AudioManager.Instance.PlayMusic(MusicId.Menu);

            if (playButton != null) playButton.onClick.AddListener(() => StartRun(0));
            if (continueButton != null) continueButton.onClick.AddListener(ContinueRun);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (creditsButton != null) creditsButton.onClick.AddListener(OpenCredits);
            if (creditsBackButton != null) creditsBackButton.onClick.AddListener(CloseCredits);
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(Quit);
#if UNITY_IOS
                quitButton.gameObject.SetActive(false);
#endif
            }
            if (settingsPanel != null) settingsPanel.Closed += OnSettingsClosed;
            if (versionText != null) versionText.text = $"v{Application.version}";

            RefreshMain();
            if (settingsPanel != null) settingsPanel.Hide();
            if (creditsPanel != null) creditsPanel.Hide();
            if (mainPanel != null) mainPanel.Show();
        }

        private void OnDestroy()
        {
            if (settingsPanel != null) settingsPanel.Closed -= OnSettingsClosed;
        }

        private void RefreshMain()
        {
            var data = SaveService.Data;
            bool hasProgress = data != null && data.unlockedStage > 0;
            if (continueButton != null) continueButton.gameObject.SetActive(hasProgress);
            if (continueLabel != null && data != null) continueLabel.text = $"CONTINUE (STAGE {data.unlockedStage + 1})";
            if (highScoreText != null) highScoreText.text = data != null ? $"HIGH SCORE  {data.highScore:N0}" : "";
        }

        private void StartRun(int stageIndex)
        {
            GameSession.StartNewRun(stageIndex);
            SceneLoader.LoadGameplay(config);
        }

        private void ContinueRun()
        {
            var data = SaveService.Data;
            StartRun(data != null ? data.unlockedStage : 0);
        }

        private void OpenSettings()
        {
            if (mainPanel != null) mainPanel.Hide();
            if (settingsPanel != null) settingsPanel.Show();
        }

        private void OnSettingsClosed()
        {
            RefreshMain();
            if (mainPanel != null) mainPanel.Show();
        }

        private void OpenCredits()
        {
            if (mainPanel != null) mainPanel.Hide();
            if (creditsPanel != null) creditsPanel.Show();
        }

        private void CloseCredits()
        {
            if (creditsPanel != null) creditsPanel.Hide();
            if (mainPanel != null) mainPanel.Show();
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
