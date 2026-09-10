namespace Starfall.Audio
{
    /// <summary>
    /// Mix table for the final audio (docs/BALANCING.md, "Mixagem"). The generated clips are all mastered loud, so
    /// the per-clip gain is what keeps frequent sounds (weapon fire, enemy shots, hits) under the music while rare,
    /// meaningful ones (explosions, warnings, ultimate) still punch through. Applied when the AudioLibrary is filled.
    /// </summary>
    public static class AudioMix
    {
        /// <summary>Same-id sounds fired closer than this are dropped (weapon spam would otherwise stack to a wall).</summary>
        public const float MinRepeatSeconds = 0.05f;
        /// <summary>Gain applied to a same-id sound that starts while the previous one is still fresh.</summary>
        public const float OverlapGain = 0.6f;
        public const float OverlapWindowSeconds = 0.18f;

        public static float SfxVolume(SfxId id)
        {
            switch (id)
            {
                // Player weapons: fire many times per second, keep well under the music.
                case SfxId.Laser: return 0.28f;
                case SfxId.Spread: return 0.28f;
                case SfxId.Plasma: return 0.36f;
                case SfxId.Railgun: return 0.42f;
                case SfxId.Missile: return 0.38f;
                case SfxId.EnergyCannon: return 0.5f;
                case SfxId.Charge: return 0.4f;
                // Enemy fire and small hits: background texture.
                case SfxId.EnemyShot: return 0.3f;
                case SfxId.Impact: return 0.22f;
                case SfxId.ShieldHit: return 0.45f;
                case SfxId.WebShot: return 0.45f;
                // Events the player must notice.
                case SfxId.ExplosionSmall: return 0.5f;
                case SfxId.ExplosionLarge: return 0.85f;
                case SfxId.PlayerHit: return 0.85f;
                case SfxId.PowerUp: return 0.6f;
                case SfxId.Ultimate: return 0.9f;
                case SfxId.LaserCharge: return 0.7f;
                case SfxId.Alarm: return 0.6f;
                case SfxId.BossWarning: return 0.85f;
                case SfxId.Summon: return 0.6f;
                // Risk / score feedback: frequent, subtle.
                case SfxId.Graze: return 0.3f;
                case SfxId.RiskUp: return 0.4f;
                case SfxId.RiskDown: return 0.35f;
                case SfxId.ComboUp: return 0.35f;
                case SfxId.OverdriveStart: return 0.7f;
                case SfxId.OverdriveEnd: return 0.6f;
                // Meta / UI.
                case SfxId.RankReveal: return 0.7f;
                case SfxId.Achievement: return 0.7f;
                case SfxId.Purchase: return 0.6f;
                case SfxId.UiSelect: return 0.35f;
                case SfxId.UiConfirm: return 0.5f;
                case SfxId.UiError: return 0.5f;
                default: return 0.5f;
            }
        }

        public static float MusicVolume(MusicId id)
        {
            switch (id)
            {
                case MusicId.OverdriveLayer: return 0.8f;
                default: return 1f;
            }
        }

        public static float AmbientVolume(AmbientId id) => 0.7f;
    }
}
