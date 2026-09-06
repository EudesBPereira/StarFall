using System;
using System.Collections.Generic;
using UnityEngine;

namespace Starfall.Audio
{
    /// <summary>
    /// Generates tiny procedural clips so the game has audible feedback before final assets exist.
    /// Everything here is a PLACEHOLDER and is created in memory (no fake files on disk).
    /// Music styles follow GDD §21 loosely (pad, arpeggio, synthwave bass, industrial pulse, epic chords).
    /// </summary>
    public static class PlaceholderAudioSynth
    {
        private const int SampleRate = 22050;
        private static readonly Dictionary<SfxId, AudioClip> SfxCache = new Dictionary<SfxId, AudioClip>();
        private static readonly Dictionary<MusicId, AudioClip> MusicCache = new Dictionary<MusicId, AudioClip>();
        private static readonly Dictionary<AmbientId, AudioClip> AmbientCache = new Dictionary<AmbientId, AudioClip>();
        private static AudioClip _engine;

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

        public static AudioClip GetAmbient(AmbientId id)
        {
            if (id == AmbientId.None) return null;
            if (AmbientCache.TryGetValue(id, out var cached) && cached != null) return cached;
            var clip = BuildAmbient(id);
            AmbientCache[id] = clip;
            return clip;
        }

        public static AudioClip GetEngineLoop()
        {
            if (_engine != null) return _engine;
            int n = SampleRate * 2;
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate;
                float s = Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.5f + Osc(Wave.Saw, 110f * t) * 0.25f + Mathf.Sin(2f * Mathf.PI * 165f * t) * 0.15f;
                s *= 0.8f + 0.2f * Mathf.Sin(2f * Mathf.PI * 7f * t);
                data[i] = s * 0.5f;
            }
            _engine = Make("ph_engine", data);
            return _engine;
        }

        private static AudioClip Build(SfxId id)
        {
            switch (id)
            {
                case SfxId.Laser: return Tone("ph_laser", 0.09f, 1400f, 700f, 0.35f, Wave.Square);
                case SfxId.Plasma: return Tone("ph_plasma", 0.16f, 600f, 250f, 0.4f, Wave.Saw);
                case SfxId.Spread: return Tone("ph_spread", 0.08f, 1100f, 500f, 0.35f, Wave.Square);
                case SfxId.Railgun: return Sweep("ph_railgun", 0.35f, 2400f, 200f, 0.5f);
                case SfxId.Missile: return Noise("ph_missile", 0.3f, 0.4f, 1800f);
                case SfxId.EnergyCannon: return Sweep("ph_cannon", 0.5f, 300f, 1500f, 0.6f);
                case SfxId.Charge: return Sweep("ph_chargeUp", 0.6f, 200f, 900f, 0.3f);
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
                case SfxId.Alarm: return Arpeggio("ph_alarm", new[] { 880f, 660f, 880f, 660f, 880f, 660f }, 0.12f, 0.35f);
                case SfxId.WebShot: return Tone("ph_web", 0.25f, 400f, 900f, 0.3f, Wave.Sine);
                case SfxId.Summon: return Arpeggio("ph_summon", new[] { 220f, 330f, 440f }, 0.1f, 0.35f);
                case SfxId.Achievement: return Arpeggio("ph_achievement", new[] { 523f, 659f, 784f, 1047f }, 0.11f, 0.45f);
                case SfxId.Purchase: return Arpeggio("ph_purchase", new[] { 784f, 1047f }, 0.08f, 0.4f);
                case SfxId.Graze: return Tone("ph_graze", 0.05f, 2200f, 3200f, 0.22f, Wave.Sine);
                case SfxId.RiskUp: return Arpeggio("ph_riskUp", new[] { 660f, 990f }, 0.05f, 0.3f);
                case SfxId.RiskDown: return Arpeggio("ph_riskDown", new[] { 990f, 660f }, 0.05f, 0.2f);
                case SfxId.OverdriveStart: return Sweep("ph_odStart", 0.7f, 300f, 2400f, 0.55f);
                case SfxId.OverdriveEnd: return Sweep("ph_odEnd", 0.5f, 1800f, 300f, 0.4f);
                case SfxId.ComboUp: return Arpeggio("ph_combo", new[] { 880f, 1320f }, 0.05f, 0.3f);
                case SfxId.RankReveal: return Arpeggio("ph_rank", new[] { 523f, 659f, 784f, 1047f, 1319f }, 0.1f, 0.45f);
                default: return Tone("ph_default", 0.08f, 800f, 800f, 0.3f, Wave.Sine);
            }
        }

        private static AudioClip BuildMusic(MusicId id)
        {
            // Placeholder loops: a chord progression rendered from simple oscillators, styled per GDD §21.
            float[][] progressions =
            {
                new[] { 110f, 130.8f, 164.8f }, // A minor
                new[] { 87.3f, 110f, 130.8f },  // F major
                new[] { 130.8f, 164.8f, 196f }, // C major
                new[] { 98f, 123.5f, 146.8f },  // G major
            };
            if (id == MusicId.OverdriveLayer) return BuildOverdriveLayer();
            bool boss = id == MusicId.Boss || id == MusicId.FinalBoss;
            bool electronic = id == MusicId.Stage2 || id == MusicId.Survival;
            bool synthwave = id == MusicId.Stage3;
            bool industrial = id == MusicId.Stage4;
            bool epic = id == MusicId.Stage5 || id == MusicId.FinalBoss;
            float chordSeconds = boss ? 1.2f : electronic ? 1.6f : 2.4f;
            float tempo = boss ? 3f : electronic || synthwave ? 2.2f : industrial ? 1.8f : 1.5f;
            int chords = progressions.Length;
            int samplesPerChord = Mathf.RoundToInt(chordSeconds * SampleRate);
            var data = new float[samplesPerChord * chords];
            float gain = 0.15f;
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
                        float f = chord[n] * (synthwave ? 0.5f : 1f);
                        if (electronic)
                        {
                            // arpeggio: one note at a time
                            int step = (int)(t * tempo * 4f) % chord.Length;
                            if (step != n) continue;
                            s += Osc(Wave.Square, f * 2f * t) * 0.6f;
                        }
                        else
                        {
                            s += Mathf.Sin(2f * Mathf.PI * f * t) * 0.5f;
                            s += (epic ? Osc(Wave.Saw, f * t) : Mathf.Sin(2f * Mathf.PI * f * 2f * t)) * 0.18f;
                        }
                    }
                    float pulse = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * tempo * t);
                    if (boss || industrial) s += Sign(Mathf.Sin(2f * Mathf.PI * 55f * t)) * 0.12f * pulse;
                    if (synthwave) s += Osc(Wave.Saw, chord[0] * 0.5f * t) * 0.2f * (0.5f + 0.5f * Sign(Mathf.Sin(2f * Mathf.PI * tempo * 2f * t)));
                    if (industrial) s += Noise01(i) * 0.08f * (pulse > 0.9f ? 1f : 0f);
                    data[c * samplesPerChord + i] = s * gain * env * (0.75f + 0.25f * pulse);
                }
            }
            return Make("ph_music_" + id, data);
        }

        /// <summary>Rhythmic pulse layer mixed on top of the stage music while Overdrive is active (plan §14.2).</summary>
        private static AudioClip BuildOverdriveLayer()
        {
            int n = SampleRate * 2;
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate;
                float beat = 1f - (t * 4f - Mathf.Floor(t * 4f));
                float s = Osc(Wave.Saw, 110f * t) * 0.25f * beat * beat + Sign(Mathf.Sin(2f * Mathf.PI * 55f * t)) * 0.12f * beat;
                s += Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.05f * (beat > 0.9f ? 1f : 0f);
                data[i] = s * 0.6f;
            }
            return Make("ph_music_overdrive", data);
        }

        private static AudioClip BuildAmbient(AmbientId id)
        {
            int n = SampleRate * 4;
            var data = new float[n];
            var rng = new System.Random((int)id * 31);
            float last = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate;
                float s = 0f;
                switch (id)
                {
                    case AmbientId.Space:
                        s = Mathf.Sin(2f * Mathf.PI * 48f * t) * 0.15f * (0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 0.2f * t));
                        break;
                    case AmbientId.Asteroids:
                        last += 0.02f * ((float)(rng.NextDouble() * 2 - 1) - last);
                        s = last * 0.8f + Mathf.Sin(2f * Mathf.PI * 40f * t) * 0.08f;
                        break;
                    case AmbientId.Nebula:
                        s = (Mathf.Sin(2f * Mathf.PI * 60f * t) + Mathf.Sin(2f * Mathf.PI * 63f * t)) * 0.08f * (0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 0.35f * t));
                        break;
                    case AmbientId.Fortress:
                        last += 0.05f * ((float)(rng.NextDouble() * 2 - 1) - last);
                        s = last * 0.5f + Sign(Mathf.Sin(2f * Mathf.PI * 2f * t)) * 0.03f + Mathf.Sin(2f * Mathf.PI * 90f * t) * 0.06f;
                        break;
                    case AmbientId.Hive:
                        s = Mathf.Sin(2f * Mathf.PI * (70f + 10f * Mathf.Sin(2f * Mathf.PI * 0.5f * t)) * t) * 0.12f;
                        s += Mathf.Sin(2f * Mathf.PI * 35f * t) * 0.08f;
                        break;
                }
                // crossfade ends so the loop is seamless
                float fade = Mathf.Min(1f, Mathf.Min(t * 2f, (4f - t) * 2f));
                data[i] = s * fade;
            }
            return Make("ph_ambient_" + id, data);
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

        private static float Noise01(int i)
        {
            unchecked
            {
                uint x = (uint)i * 2654435761u;
                x ^= x >> 13;
                x *= 0x5bd1e995;
                x ^= x >> 15;
                return (x & 0xFFFF) / 32768f - 1f;
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
