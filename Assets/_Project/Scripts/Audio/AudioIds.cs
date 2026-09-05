namespace Starfall.Audio
{
    public enum SfxId
    {
        Laser = 0,
        EnemyShot = 1,
        Impact = 2,
        ShieldHit = 3,
        ExplosionSmall = 4,
        ExplosionLarge = 5,
        PowerUp = 6,
        PlayerHit = 7,
        Ultimate = 8,
        UiSelect = 9,
        UiConfirm = 10,
        UiError = 11,
        BossWarning = 12,
        LaserCharge = 13,
        Plasma = 14,
        Railgun = 15,
        Missile = 16,
        EnergyCannon = 17,
        Charge = 18,
        Alarm = 19,
        WebShot = 20,
        Summon = 21,
        Achievement = 22,
        Purchase = 23,
        Spread = 24,
    }

    public enum MusicId
    {
        None = 0,
        Menu = 1,
        Stage1 = 2,
        Stage2 = 3,
        Stage3 = 4,
        Boss = 5,
        Stage4 = 6,
        Stage5 = 7,
        Survival = 8,
        FinalBoss = 9,
    }

    /// <summary>Looping ambience per environment (GDD §20 "Ambiente").</summary>
    public enum AmbientId
    {
        None = 0,
        Space = 1,
        Asteroids = 2,
        Nebula = 3,
        Fortress = 4,
        Hive = 5,
    }
}
