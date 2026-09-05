namespace Starfall.Combat
{
    public enum Faction
    {
        Neutral = 0,
        Player = 1,
        Enemy = 2,
    }

    /// <summary>Physics layer indices configured by the project bootstrap. Keep in sync with TagManager.</summary>
    public static class GameLayers
    {
        public const int Player = 6;
        public const int PlayerProjectile = 7;
        public const int Enemy = 8;
        public const int EnemyProjectile = 9;
        public const int PowerUp = 10;
        public const int Obstacle = 11;

        public static int ProjectileLayerFor(Faction faction) =>
            faction == Faction.Player ? PlayerProjectile : EnemyProjectile;
    }

    /// <summary>Sprite sorting orders so every layer of the scene is readable.</summary>
    public static class SortingOrders
    {
        public const int Background = -100;
        public const int Stars = -90;
        public const int Debris = -80;
        public const int Enemy = 0;
        public const int EnemyShield = 2;
        public const int Fog = 5;
        public const int Player = 10;
        public const int PlayerShield = 12;
        public const int PowerUp = 15;
        public const int Projectile = 20;
        public const int Laser = 25;
        public const int Vfx = 30;
        public const int FloatingText = 40;
    }
}
