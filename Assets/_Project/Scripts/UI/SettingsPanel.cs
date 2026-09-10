using Starfall.Logic;
using System;
using Starfall.Audio;
using Starfall.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>Audio, control and accessibility preferences (plan §15.2). Writes to the save when closed.</summary>
    public sealed class SettingsPanel : UiPanel
    {
        [SerializeField] internal Slider masterSlider;
        [SerializeField] internal Slider musicSlider;
        [SerializeField] internal Slider sfxSlider;
        [SerializeField] internal Slider sensitivitySlider;
        [SerializeField] internal Slider shakeSlider;
        [SerializeField] internal Toggle autoFireToggle;
        [SerializeField] internal Toggle reducedEffectsToggle;
        [SerializeField] internal Toggle grazeFeedbackToggle;
        [SerializeField] internal Toggle showHitboxToggle;
        [SerializeField] internal Button resetProgressButton;
        [SerializeField] internal TMP_Text resetFeedbackText;
        [SerializeField] internal Button backButton;
        [SerializeField] internal Button languageButton;

        private bool _suppress;
        public event Action Closed;

        private void Awake()
        {
            if (masterSlider != null) masterSlider.onValueChanged.AddListener(_ => ApplyLive());
            if (musicSlider != null) musicSlider.onValueChanged.AddListener(_ => ApplyLive());
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(_ => ApplyLive());
            if (sensitivitySlider != null) sensitivitySlider.onValueChanged.AddListener(_ => ApplyLive());
            if (shakeSlider != null) shakeSlider.onValueChanged.AddListener(_ => ApplyLive());
            if (autoFireToggle != null) autoFireToggle.onValueChanged.AddListener(_ => ApplyLive());
            if (reducedEffectsToggle != null) reducedEffectsToggle.onValueChanged.AddListener(_ => ApplyLive());
            if (grazeFeedbackToggle != null) grazeFeedbackToggle.onValueChanged.AddListener(_ => ApplyLive());
            if (showHitboxToggle != null) showHitboxToggle.onValueChanged.AddListener(_ => ApplyLive());
            if (resetProgressButton != null) resetProgressButton.onClick.AddListener(ResetProgress);
            if (backButton != null) backButton.onClick.AddListener(Close);
            if (languageButton != null) languageButton.onClick.AddListener(CycleLanguage);
        }

        public override void Show()
        {
            var data = SaveService.Data;
            _suppress = true;
            if (data != null)
            {
                if (masterSlider != null) masterSlider.value = data.masterVolume;
                if (musicSlider != null) musicSlider.value = data.musicVolume;
                if (sfxSlider != null) sfxSlider.value = data.sfxVolume;
                if (sensitivitySlider != null) sensitivitySlider.value = data.touchSensitivity;
                if (shakeSlider != null) shakeSlider.value = data.screenShake;
                if (autoFireToggle != null) autoFireToggle.isOn = data.autoFire;
                if (reducedEffectsToggle != null) reducedEffectsToggle.isOn = data.reducedEffects;
                if (grazeFeedbackToggle != null) grazeFeedbackToggle.isOn = data.grazeFeedback;
                if (showHitboxToggle != null) showHitboxToggle.isOn = data.showHitbox;
            }
            if (resetFeedbackText != null) resetFeedbackText.text = "";
            RefreshLanguageLabel();
            _suppress = false;
            base.Show();
        }

        private void ApplyLive()
        {
            if (_suppress) return;
            var data = SaveService.Data;
            if (data == null) return;
            if (masterSlider != null) data.masterVolume = masterSlider.value;
            if (musicSlider != null) data.musicVolume = musicSlider.value;
            if (sfxSlider != null) data.sfxVolume = sfxSlider.value;
            if (sensitivitySlider != null) data.touchSensitivity = sensitivitySlider.value;
            if (shakeSlider != null) data.screenShake = shakeSlider.value;
            if (autoFireToggle != null) data.autoFire = autoFireToggle.isOn;
            if (reducedEffectsToggle != null) data.reducedEffects = reducedEffectsToggle.isOn;
            if (grazeFeedbackToggle != null) data.grazeFeedback = grazeFeedbackToggle.isOn;
            if (showHitboxToggle != null) data.showHitbox = showHitboxToggle.isOn;
            if (AudioManager.Instance != null) AudioManager.Instance.SetVolumes(data.masterVolume, data.musicVolume, data.sfxVolume);
        }

        private void CycleLanguage()
        {
            var data = SaveService.Data;
            var next = Loc.Next(Loc.Current);
            if (data != null) data.language = (int)next;
            Loc.Set(next);
            SaveService.Save();
            RefreshLanguageLabel();
            Audio.AudioManager.PlaySfx(Audio.SfxId.UiConfirm);
        }

        private void RefreshLanguageLabel()
        {
            if (languageButton == null) return;
            var label = languageButton.GetComponentInChildren<TMPro.TMP_Text>();
            if (label != null) label.text = Loc.F("LANGUAGE: {0}", Loc.DisplayName(Loc.Current));
        }

        private void ResetProgress()
        {
            SaveService.ResetProgress();
            if (resetFeedbackText != null) resetFeedbackText.text = Loc.T("Progress reset.");
        }

        public void Close()
        {
            ApplyLive();
            SaveService.Save();
            Hide();
            Closed?.Invoke();
        }
    }
}
