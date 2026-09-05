// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
namespace Starfall.Logic
{
    /// <summary>Power-up families. Colors and values are documented in docs/BALANCING.md.</summary>
    public enum PowerUpKind
    {
        LaserLevel = 0,     // Blue
        ShieldRestore = 1,  // Green
        DamageBoost = 2,    // Red
        SpeedBoost = 3,     // Yellow
        Energy = 4,         // Purple
        Invincibility = 5,  // White
        /// <summary>Debuff applied by Widow's energy webs (not a pickup).</summary>
        Slowed = 6,
    }
}
