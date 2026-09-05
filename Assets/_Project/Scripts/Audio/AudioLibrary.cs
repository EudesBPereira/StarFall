using System;
using UnityEngine;

namespace Starfall.Audio
{
    /// <summary>
    /// Integration point for final audio assets. Any slot left empty falls back to a synthesized placeholder
    /// (when enabled) or to silence; the game never throws because a clip is missing.
    /// </summary>
    [CreateAssetMenu(menuName = "Starfall/Audio/Audio Library", fileName = "AudioLibrary")]
    public sealed class AudioLibrary : ScriptableObject
    {
        [Serializable]
        public struct SfxEntry
        {
            public SfxId Id;
            public AudioClip[] Clips;
            [Range(0f, 1f)] public float Volume;
        }

        [Serializable]
        public struct MusicEntry
        {
            public MusicId Id;
            public AudioClip Clip;
            [Range(0f, 1f)] public float Volume;
        }

        [Tooltip("Generate simple synthesized sounds at runtime for empty slots (placeholder audio).")]
        public bool UseSynthesizedPlaceholders = true;
        public SfxEntry[] Sfx = new SfxEntry[0];
        public MusicEntry[] Music = new MusicEntry[0];

        public bool TryGetSfx(SfxId id, out AudioClip clip, out float volume)
        {
            clip = null;
            volume = 1f;
            if (Sfx == null) return false;
            for (int i = 0; i < Sfx.Length; i++)
            {
                if (Sfx[i].Id != id || Sfx[i].Clips == null || Sfx[i].Clips.Length == 0) continue;
                var clips = Sfx[i].Clips;
                clip = clips[UnityEngine.Random.Range(0, clips.Length)];
                volume = Sfx[i].Volume <= 0f ? 1f : Sfx[i].Volume;
                return clip != null;
            }
            return false;
        }

        public bool TryGetMusic(MusicId id, out AudioClip clip, out float volume)
        {
            clip = null;
            volume = 1f;
            if (Music == null) return false;
            for (int i = 0; i < Music.Length; i++)
            {
                if (Music[i].Id != id || Music[i].Clip == null) continue;
                clip = Music[i].Clip;
                volume = Music[i].Volume <= 0f ? 1f : Music[i].Volume;
                return true;
            }
            return false;
        }
    }
}
