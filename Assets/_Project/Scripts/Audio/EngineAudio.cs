using Starfall.Player;
using Starfall.Save;
using UnityEngine;

namespace Starfall.Audio
{
    /// <summary>Looping engine hum whose pitch/volume follow the ship speed (GDD §20 "Motores").</summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class EngineAudio : MonoBehaviour
    {
        [SerializeField] internal float baseVolume = 0.25f;

        private AudioSource _source;
        private PlayerMovement _movement;
        private bool _active;

        public void Configure(PlayerMovement movement)
        {
            _movement = movement;
            _source = GetComponent<AudioSource>();
            _source.loop = true;
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            if (_source.clip == null) _source.clip = PlaceholderAudioSynth.GetEngineLoop();
        }

        public void SetActive(bool active)
        {
            _active = active;
            if (_source == null) return;
            if (active && _source.clip != null && !_source.isPlaying) _source.Play();
            if (!active && _source.isPlaying) _source.Stop();
        }

        private void Update()
        {
            if (!_active || _source == null || _movement == null) return;
            float k = _movement.MaxSpeed > 0f ? Mathf.Clamp01(_movement.CurrentSpeed / _movement.MaxSpeed) : 0f;
            var data = SaveService.Data;
            float gain = data != null ? data.masterVolume * data.sfxVolume : 1f;
            _source.pitch = Mathf.Lerp(0.85f, 1.35f, k);
            _source.volume = baseVolume * gain * Mathf.Lerp(0.6f, 1f, k) * (Time.timeScale > 0f ? 1f : 0f);
        }
    }
}
