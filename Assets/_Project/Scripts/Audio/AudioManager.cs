using System.Collections;
using Starfall.Bosses;
using Starfall.Core;
using Starfall.Logic;
using Starfall.Save;
using UnityEngine;

namespace Starfall.Audio
{
    /// <summary>
    /// Persistent audio service (music, ambience and pooled SFX sources). It is the one DontDestroyOnLoad singleton
    /// in the project: music must keep playing across scene loads and volumes are global. See docs/ARCHITECTURE.md.
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        private const int SfxSourceCount = 12;

        public static AudioManager Instance { get; private set; }

        private AudioLibrary _library;
        private AudioSource _musicA;
        private AudioSource _musicB;
        private AudioSource _activeMusic;
        private AudioSource _ambient;
        private AudioSource _overdriveLayer;
        private Coroutine _layerFade;
        private AudioSource[] _sfx;
        private int _nextSfx;
        private MusicId _currentMusic = MusicId.None;
        private AmbientId _currentAmbient = AmbientId.None;
        private float _currentMusicVolume = 1f;
        private float _currentAmbientVolume = 1f;
        private Coroutine _fade;

        private float _master = 1f;
        private float _music = 0.8f;
        private float _sfxVolume = 1f;

        public MusicId CurrentMusic => _currentMusic;

        /// <summary>Creates the manager if needed. Safe to call from any scene.</summary>
        public static AudioManager Ensure(AudioLibrary library)
        {
            if (Instance != null)
            {
                if (library != null && Instance._library == null) Instance._library = library;
                return Instance;
            }
            var go = new GameObject("[AudioManager]");
            DontDestroyOnLoad(go);
            var manager = go.AddComponent<AudioManager>();
            manager._library = library;
            manager.Build();
            return manager;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Build()
        {
            _musicA = CreateSource("MusicA", true);
            _musicB = CreateSource("MusicB", true);
            _ambient = CreateSource("Ambient", true);
            _overdriveLayer = CreateSource("OverdriveLayer", true);
            _sfx = new AudioSource[SfxSourceCount];
            for (int i = 0; i < SfxSourceCount; i++) _sfx[i] = CreateSource("Sfx" + i, false);
            ApplySavedSettings();
            GameSignals.EnemyDestroyed += OnEnemyDestroyed;
            GameSignals.BossSpawned += OnBossSpawned;
            GameSignals.BossDefeated += OnBossDefeated;
            GameSignals.AchievementUnlocked += OnAchievementUnlocked;
            GameSignals.OverdriveChanged += OnOverdriveChanged;
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            GameSignals.EnemyDestroyed -= OnEnemyDestroyed;
            GameSignals.BossSpawned -= OnBossSpawned;
            GameSignals.BossDefeated -= OnBossDefeated;
            GameSignals.AchievementUnlocked -= OnAchievementUnlocked;
            GameSignals.OverdriveChanged -= OnOverdriveChanged;
            Instance = null;
        }

        private AudioSource CreateSource(string name, bool loop)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = loop;
            src.spatialBlend = 0f;
            return src;
        }

        // ---- Settings ---------------------------------------------------------------------------

        public void ApplySavedSettings()
        {
            var data = SaveService.Data;
            if (data == null) return;
            SetVolumes(data.masterVolume, data.musicVolume, data.sfxVolume);
        }

        public void SetVolumes(float master, float music, float sfx)
        {
            _master = Mathf.Clamp01(master);
            _music = Mathf.Clamp01(music);
            _sfxVolume = Mathf.Clamp01(sfx);
            if (_activeMusic != null) _activeMusic.volume = MusicGain * _currentMusicVolume;
            if (_ambient != null) _ambient.volume = SfxGain * 0.6f * _currentAmbientVolume;
        }

        private float MusicGain => _master * _music;
        private float SfxGain => _master * _sfxVolume;

        // ---- Music ----------------------------------------------------------------------------------

        public void PlayMusic(MusicId id, float fadeSeconds = 0.8f)
        {
            if (id == _currentMusic && _activeMusic != null && _activeMusic.isPlaying) return;
            AudioClip clip = null;
            float volume = 1f;
            if (_library != null && _library.TryGetMusic(id, out var libClip, out var libVol))
            {
                clip = libClip;
                volume = libVol;
            }
            else if (_library == null || _library.UseSynthesizedPlaceholders)
            {
                clip = PlaceholderAudioSynth.GetMusic(id);
            }

            _currentMusic = id;
            _currentMusicVolume = volume;
            if (_fade != null) StopCoroutine(_fade);
            _fade = StartCoroutine(CrossFade(clip, fadeSeconds));
        }

        public void StopMusic(float fadeSeconds = 0.5f)
        {
            _currentMusic = MusicId.None;
            if (_fade != null) StopCoroutine(_fade);
            _fade = StartCoroutine(CrossFade(null, fadeSeconds));
        }

        private IEnumerator CrossFade(AudioClip next, float seconds)
        {
            var from = _activeMusic;
            var to = from == _musicA ? _musicB : _musicA;
            _activeMusic = to;

            if (next != null)
            {
                to.clip = next;
                to.volume = 0f;
                to.Play();
            }

            float t = 0f;
            float fromStart = from != null ? from.volume : 0f;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float k = seconds <= 0f ? 1f : Mathf.Clamp01(t / seconds);
                if (from != null) from.volume = Mathf.Lerp(fromStart, 0f, k);
                if (next != null) to.volume = Mathf.Lerp(0f, MusicGain * _currentMusicVolume, k);
                yield return null;
            }
            if (from != null) { from.Stop(); from.clip = null; }
            if (next != null) to.volume = MusicGain * _currentMusicVolume;
            else to.Stop();
            _fade = null;
        }

        // ---- Ambience -------------------------------------------------------------------------------

        public void PlayAmbient(AmbientId id)
        {
            if (_ambient == null) return;
            if (id == _currentAmbient && _ambient.isPlaying) return;
            _currentAmbient = id;
            AudioClip clip = null;
            float volume = 1f;
            if (_library != null && _library.TryGetAmbient(id, out var libClip, out var libVol))
            {
                clip = libClip;
                volume = libVol;
            }
            else if (_library == null || _library.UseSynthesizedPlaceholders)
            {
                clip = PlaceholderAudioSynth.GetAmbient(id);
            }
            _currentAmbientVolume = volume;
            _ambient.Stop();
            _ambient.clip = clip;
            _ambient.volume = SfxGain * 0.6f * volume;
            if (clip != null) _ambient.Play();
        }

        public void StopAmbient()
        {
            _currentAmbient = AmbientId.None;
            if (_ambient != null) _ambient.Stop();
        }

        // ---- SFX -------------------------------------------------------------------------------------

        public static void PlaySfx(SfxId id, float volumeScale = 1f)
        {
            if (Instance == null) return;
            Instance.PlaySfxInternal(id, volumeScale);
        }

        private void PlaySfxInternal(SfxId id, float volumeScale)
        {
            AudioClip clip = null;
            float volume = 1f;
            if (_library != null && _library.TryGetSfx(id, out var libClip, out var libVol))
            {
                clip = libClip;
                volume = libVol;
            }
            else if (_library == null || _library.UseSynthesizedPlaceholders)
            {
                clip = PlaceholderAudioSynth.GetSfx(id);
            }
            if (clip == null || _sfx == null) return;

            var src = _sfx[_nextSfx];
            _nextSfx = (_nextSfx + 1) % _sfx.Length;
            src.pitch = Random.Range(0.96f, 1.04f);
            src.PlayOneShot(clip, SfxGain * volume * volumeScale);
        }

        // ---- Signal-driven feedback ----------------------------------------------------------------------

        private void OnEnemyDestroyed(EnemyKilledInfo info)
        {
            PlaySfxInternal(info.IsBoss ? SfxId.ExplosionLarge : SfxId.ExplosionSmall, info.IsBoss ? 1f : 0.7f);
        }

        private void OnBossSpawned(BossController boss)
        {
            PlaySfxInternal(SfxId.BossWarning, 1f);
            bool final = boss != null && boss.Boss != null && boss.Boss.BossId == BossId.OmegaCore;
            PlayMusic(final ? MusicId.FinalBoss : MusicId.Boss, 1.2f);
        }

        private void OnBossDefeated(BossController boss)
        {
            var ctx = GameplayContext.Current;
            if (ctx != null && ctx.CurrentStage != null && !ctx.CurrentStage.KeepBossMusicAfterDefeat)
                PlayMusic(ctx.CurrentStage.Music, 1.5f);
        }

        private void OnAchievementUnlocked(AchievementId id) => PlaySfxInternal(SfxId.Achievement, 1f);

        /// <summary>Overdrive adds a rhythmic layer that fades in and out smoothly (plan §14.3).</summary>
        private void OnOverdriveChanged(bool active)
        {
            if (_overdriveLayer == null) return;
            if (_layerFade != null) StopCoroutine(_layerFade);
            _layerFade = StartCoroutine(FadeLayer(active));
        }

        private IEnumerator FadeLayer(bool on)
        {
            if (on)
            {
                AudioClip clip = null;
                float volume = 1f;
                if (_library != null && _library.TryGetMusic(MusicId.OverdriveLayer, out var libClip, out var libVol)) { clip = libClip; volume = libVol; }
                else if (_library == null || _library.UseSynthesizedPlaceholders) clip = PlaceholderAudioSynth.GetMusic(MusicId.OverdriveLayer);
                if (clip == null) yield break;
                if (_overdriveLayer.clip != clip) { _overdriveLayer.clip = clip; }
                if (!_overdriveLayer.isPlaying) { _overdriveLayer.volume = 0f; _overdriveLayer.Play(); }
                float target = MusicGain * volume * 0.8f;
                while (_overdriveLayer.volume < target) { _overdriveLayer.volume = Mathf.MoveTowards(_overdriveLayer.volume, target, Time.unscaledDeltaTime * 1.5f); yield return null; }
            }
            else
            {
                while (_overdriveLayer.volume > 0f) { _overdriveLayer.volume = Mathf.MoveTowards(_overdriveLayer.volume, 0f, Time.unscaledDeltaTime * 1.2f); yield return null; }
                _overdriveLayer.Stop();
            }
            _layerFade = null;
        }
    }
}
