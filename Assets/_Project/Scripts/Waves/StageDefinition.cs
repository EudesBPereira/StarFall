using System;
using Starfall.Audio;
using Starfall.Bosses;
using Starfall.Enemies;
using UnityEngine;

namespace Starfall.Waves
{
    public enum StageEventType
    {
        Wave = 0,
        Delay = 1,
        Message = 2,
        MiniBoss = 3,
        Boss = 4,
        AsteroidField = 5,
    }

    [Serializable]
    public sealed class StageEvent
    {
        public StageEventType Type = StageEventType.Wave;
        public WaveDefinition Wave;
        public BossDefinition Boss;
        [Tooltip("Message text, or empty.")]
        public string Message;
        [Tooltip("Delay seconds / message duration.")]
        [Min(0f)] public float Seconds = 2f;
        [Tooltip("AsteroidField: enable (true) or disable (false) the field.")]
        public bool Flag = true;
    }

    /// <summary>
    /// Data-driven stage: ordered events (waves, delays, messages, bosses), look and music.
    /// Authoring guide in README.md.
    /// </summary>
    [CreateAssetMenu(menuName = "Starfall/Stages/Stage Definition", fileName = "Stage")]
    public sealed class StageDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string DisplayName = "Sector";
        public string Subtitle = "";
        [TextArea(2, 6)] public string Briefing = "";

        [Header("Flow")]
        public StageEvent[] Events = new StageEvent[0];
        [Tooltip("Bonus points granted on completion (not multiplied).")]
        [Min(0)] public int CompletionBonus = 1000;
        public MusicId Music = MusicId.Stage1;
        public bool KeepBossMusicAfterDefeat = false;

        [Header("Look")]
        public Color BackgroundTop = new Color(0.02f, 0.03f, 0.10f);
        public Color BackgroundBottom = new Color(0.05f, 0.02f, 0.12f);
        [Tooltip("Overlay drawn above enemies and below projectiles (alpha = intensity).")]
        public Color Fog = new Color(0.5f, 0.2f, 0.8f, 0f);
        [Range(0f, 1f)] public float StarDensity = 0.6f;
        public Color StarTint = Color.white;
        public Sprite[] DebrisSprites = new Sprite[0];
        [Min(0f)] public float DebrisInterval = 0f;

        [Header("Asteroid field")]
        public EnemyDefinition AsteroidDefinition;
        [Min(0.1f)] public float AsteroidInterval = 1.2f;
    }
}
