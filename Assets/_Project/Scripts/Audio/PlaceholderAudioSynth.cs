using System;
using System.Collections.Generic;
using UnityEngine;

namespace Starfall.Audio
{
    /// <summary>
    /// Generates tiny procedural clips so the game has audible feedback before final assets exist.
    /// Everything here is a PLACEHOLDER and is created in memory (no fake files on disk).
    /// </summary>
    public static class PlaceholderAudioSynth
    {
        private const int SampleRate = 22050;
        private static readonly Dictionary<SfxId, AudioClip> SfxCache = new Dictionary<SfxId, AudioClip>();
        private static readonly Dictionary<MusicId, AudioClip> MusicCache = new Dictionary<MusicId, AudioClip>();

        public static AudioClip GetSfx(SfxId id)
        {
            if (SfxCache.TryGetValue(id, out var cached) && cached != null) return cached;
            var clip = Build(id);
            SfxCache[id] = clip;
            return clip;
        }

        public static AudioClip GetMusic(MusicId id)
        {
            if (id == MusicId.None) return null;
            if (MusicCache.TryGetValue(id, out var cached) && cached != null) return cached;
            var clip = BuildMusic(id);
            MusicCache[id] = clip;
            return clip;
        }

        private static AudioClip Build(SfxId id)
        {
            switch (id)
            {
                case SfxId.Laser: return Tone("ph_laser", 0.09f, 1400f, 700f, 0.35f, Wave.Square);
                case SfxId.EnemyShot: return Tone("ph_enemyShot", 0.10f, 500f, 250f, 0.3f, Wave.Saw);
                case SfxId.Impact: return Noise("ph_impact", 0.06f, 0.35f, 6000f);
                case SfxId.ShieldHit: return Tone("ph_shield", 0.12f, 900f, 1300f, 0.3f, Wave.Sine);
                case SfxId.ExplosionSmall: return Noise("ph_explS", 0.25f, 0.55f, 2500f);
                case SfxId.ExplosionLarge: return Noise("ph_explL", 0.7f, 0.8f, 1200f);
                case SfxId.PowerUp: return Arpeggio("ph_powerup", new[] { 660f, 880f, 1100f, 1320f }, 0.06f, 0.4f);
                case SfxId.PlayerHit: return Tone("ph_playerHit", 0.18f, 300f, 90f, 0.5f, Wave.Saw);
                case SfxId.Ultimate: return Sweep("ph_ultimate", 1.0f, 200f, 1800f, 0.6f);
                case SfxId.UiSelect: return Tone("ph_uiSel", 0.04f, 900f, 900f, 0.25f, Wave.Square);
                case SfxId.UiConfirm: return Arpeggio("ph_uiOk", new[] { 700f, 1050f }, 0.05f, 0.3f);
                case SfxId.UiError: return Tone("ph_uiErr", 0.15f, 220f, 180f, 0.3f, Wave.Square);
                case SfxId.BossWarning: return Arpeggio("ph_warning", new[] { 440f, 330f, 440f, 330f }, 0.18f, 0.5f);
                case SfxId.LaserCharge: return Sweep("ph_charge", 0.9f, 150f, 1200f, 0.45f);
                default: return Tone("ph_default", 0.08f, 800f, 800f, 0.3f, Wave.Sine);
            }
        }

        private static AudioClip BuildMusic(MusicId id)
        {
            // Simple looping pad: a slow chord progression rendered from sine partials. Placeholder only.
            float[][] progressions =
            {
                new[] { 110f, 130.8f, 164.8f }, // A minor
                new[] { 87.3f, 110f, 130.8f },  // F major
                new[] { 130.8f, 164.8f, 196f }, // C major
                new[] { 98f, 123.5f, 146.8f },  // G major
            };
            float chordSeconds = id == MusicId.Boss ? 1.2f : 2.4f;
            float tempoMul = id == MusicId.Boss ? 2f : 1f;
            int chords = progressions.Length;
            int samplesPerChord = Mathf.RoundToInt(chordSeconds * SampleRate);
            var data = new float[samplesPerChord * chords];
            float gain = id == MusicId.Menu ? 0.16f : 0.14f;
            int seed = (int)id;
            for (int c = 0; c < chords; c++)
            {
                var chord = progressions[(c + seed) % chords];
                for (int i = 0; i < samplesPerChord; i++)
                {
                    float t = (float)i / SampleRate;
                    float env = Mathf.Min(1f, Mathf.Min(t * 4f, (chordSeconds - t) * 4f));
                    float s = 0f;
                    for (int n = 0; n < chord.Length; n++)
                    {
                        float f = chord[n] * (id == MusicId.Stage3 ? 0.5f : 1f);
                        s += Mathf.Sin(2f * Mathf.PI * f * t) * 0.5f;
                        s += Mathf.Sin(2f * Mathf.PI * f * 2f * t) * 0.18f;
                    }
                    float pulse = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * (1.5f * tempoMul) * t);
                    if (id == MusicId.Boss) s += Sign(Mathf.Sin(2f * Mathf.PI * 55f * t)) * 0.12f * pulse;
                    data[c * samplesPerChord + i] = s * gain * env * (0.75f + 0.25f * pulse);
                }
            }
            var clip = AudioClip.Create("ph_music_" + id, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private enum Wave { Sine, Square, Saw }

        private static AudioClip Tone(string name, float seconds, float fromHz, float toHz, float gain, Wave wave)
        {
            int n = Mathf.Max(1, Mathf.RoundToInt(seconds * SampleRate));
            var data = new float[n];
            float phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                float f = Mathf.Lerp(fromHz, toHz, t);
                phase += f / SampleRate;
                float env = 1f - t;
                data[i] = Osc(wave, phase) * gain * env;
            }
            return Make(name, data);
        }

        private static AudioClip Sweep(string name, float seconds, float fromHz, float toHz, float gain)
        {
            int n = Mathf.RoundToInt(seconds * SampleRate);
            var data = new float[n];
            float phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                float f = Mathf.Lerp(fromHz, toHz, t * t);
                phase += f / SampleRate;
                float env = Mathf.Sin(t * Mathf.PI);
                data[i] = (Mathf.Sin(2f * Mathf.PI * phase) * 0.7f + Osc(Wave.Saw, phase * 0.5f) * 0.3f) * gain * env;
            }
            return Make(name, data);
        }

        private static AudioClip Noise(string name, float seconds, float gain, float cutoffHz)
        {
            int n = Mathf.RoundToInt(seconds * SampleRate);
            var data = new float[n];
            var rng = new System.Random(name.GetHashCode());
            float last = 0f;
            float alpha = Mathf.Clamp01(cutoffHz / SampleRate * 2f);
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                last += alpha * (white - last);
                float env = Mathf.Pow(1f - t, 2f);
                data[i] = last * gain * env;
            }
            return Make(name, data);
        }

        private static AudioClip Arpeggio(string name, float[] notes, float noteSeconds, float gain)
        {
            int per = Mathf.RoundToInt(noteSeconds * SampleRate);
            var data = new float[per * notes.Length];
            for (int k = 0; k < notes.Length; k++)
            {
                float phase = 0f;
                for (int i = 0; i < per; i++)
                {
                    float t = (float)i / per;
                    phase += notes[k] / SampleRate;
                    data[k * per + i] = Osc(Wave.Square, phase) * gain * (1f - t * 0.6f);
                }
            }
            return Make(name, data);
        }

        private static float Osc(Wave wave, float phase)
        {
            float p = phase - Mathf.Floor(phase);
            switch (wave)
            {
                case Wave.Square: return p < 0.5f ? 0.6f : -0.6f;
                case Wave.Saw: return (p * 2f - 1f) * 0.7f;
                default: return Mathf.Sin(p * 2f * Mathf.PI);
            }
        }

        private static float Sign(float v) => v >= 0f ? 1f : -1f;

        private static AudioClip Make(string name, float[] data)
        {
            var clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
