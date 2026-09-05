using System;
using Starfall.Audio;
using Starfall.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>Audio + control preferences. Writes to the save when closed.</summary>
    public sealed class SettingsPanel : UiPanel
    {
        [SerializeField] internal Slider masterSlider;
        [SerializeField] internal Slider musicSlider;
        [SerializeField] internal Slider sfxSlider;
        [SerializeField] internal Slider sensitivitySlider;
        [SerializeField] internal Toggle autoFireToggle;
        [SerializeField] internal Button resetProgressButton;
        [SerializeField] internal TMP_Text resetFeedbackText;
        [SerializeField] internal Button backButton;

        private bool _suppress;
        public event Action Closed;

        private void Awake()
        {
            if (masterSlider != null) masterSlider.onValueChanged.AddListener(_ => ApplyLive());
            if (musicSlider != null) musicSlider.onValueChanged.AddListener(_ => ApplyLive());
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(_ => ApplyLive());
            if (sensitivitySlider != null) sensitivitySlider.onValueChanged.AddListener(_ => ApplyLive());
            if (autoFireToggle != null) autoFireToggle.onValueChanged.AddListener(_ => ApplyLive());
            if (resetProgressButton != null) resetProgressButton.onClick.AddListener(ResetProgress);
            if (backButton != null) backButton.onClick.AddListener(Close);
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
                if (autoFireToggle != null) autoFireToggle.isOn = data.autoFire;
            }
            if (resetFeedbackText != null) resetFeedbackText.text = "";
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
            if (autoFireToggle != null) data.autoFire = autoFireToggle.isOn;
            if (AudioManager.Instance != null) AudioManager.Instance.SetVolumes(data.masterVolume, data.musicVolume, data.sfxVolume);
        }

        private void ResetProgress()
        {
            SaveService.ResetProgress();
            if (resetFeedbackText != null) resetFeedbackText.text = "Progress reset.";
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
