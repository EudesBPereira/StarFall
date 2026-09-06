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
        /// <summary>Environmental hazard (solar flare band, meteor shower) — plan §7.5 "evento ambiental".</summary>
        Hazard = 6,
        /// <summary>Pauses the stage and offers a seeded draft of temporary build mods.</summary>
        BuildChoice = 7,
        /// <summary>Optional wave that only fires for some runs (seeded by Chance) — "encontro aleatório".</summary>
        RandomEncounter = 8,
    }

    public enum HazardKind
    {
        /// <summary>A horizontal band of radiation sweeps down after a telegraph; standing in it burns hull.</summary>
        SolarFlare = 0,
        /// <summary>Fast asteroids rain for a few seconds.</summary>
        MeteorShower = 1,
        /// <summary>Visibility drops (fog thickens) for a while.</summary>
        NebulaPulse = 2,
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
        [Tooltip("Wave / RandomEncounter: alternative waves; one is picked by the run seed (partially random stages).")]
        public WaveDefinition[] Variants = new WaveDefinition[0];
        [Tooltip("RandomEncounter: probability that the encounter happens in a run.")]
        [Range(0f, 1f)] public float Chance = 0.5f;
        public HazardKind Hazard = HazardKind.SolarFlare;
    }

    /// <summary>
    /// Data-driven stage: ordered events (waves, delays, messages, bosses), look, music and ambience.
    /// Authoring guide in README.md.
    /// </summary>
    [CreateAssetMenu(menuName = "Starfall/Stages/Stage Definition", fileName = "Stage")]
    public sealed class StageDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string DisplayName = "Sector";
        public string Subtitle = "";
        [TextArea(2, 6)] public string Briefing = "";
        [TextArea(1, 4)] public string Objectives = "";

        [Header("Flow")]
        public StageEvent[] Events = new StageEvent[0];
        [Tooltip("Bonus points granted on completion (not multiplied).")]
        [Min(0)] public int CompletionBonus = 1000;
        [Tooltip("Seconds a good run takes; finishing faster grants the time bonus (plan §9.2).")]
        [Min(0f)] public float ParTimeSeconds = 180f;
        [Tooltip("Score of a good run; rank thresholds C..SSS derive from it (plan §9.3).")]
        [Min(1000)] public int RankTargetScore = 20000;
        [Tooltip("Credits granted for completing the stage, independent of score (plan §11.5).")]
        [Min(0)] public int BaseCredits = 300;
        [Tooltip("Hull/damage multiplier applied to every enemy of this stage (difficulty curve).")]
        [Min(0.1f)] public float EnemyStatMultiplier = 1f;
        [Tooltip("Mini-boss shown in the mission list (informational).")]
        public string MiniBossName = "";
        public string BossName = "";
        public MusicId Music = MusicId.Stage1;
        public AmbientId Ambient = AmbientId.Space;
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
        public Color DebrisTint = new Color(0.6f, 0.65f, 0.75f, 0.55f);
        [Tooltip("Optional large silhouette scrolling slowly behind everything (planet, fortress wall).")]
        public Sprite BackdropSprite;
        public Color BackdropTint = new Color(0.3f, 0.3f, 0.4f, 0.5f);
        [Min(0f)] public float BackdropScale = 6f;

        [Header("Asteroid field")]
        public EnemyDefinition AsteroidDefinition;
        [Min(0.1f)] public float AsteroidInterval = 1.2f;
    }
}
