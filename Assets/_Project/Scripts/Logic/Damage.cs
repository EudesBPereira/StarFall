// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
namespace Starfall.Logic
{
    /// <summary>Who caused the damage. Used for scoring rules, drop rules and future analytics.</summary>
    public enum DamageSource
    {
        Unknown = 0,
        Player = 1,
        Enemy = 2,
        Boss = 3,
        Environment = 4,
        Ultimate = 5,
    }

    /// <summary>Damage classification. Used by resistances and feedback.</summary>
    public enum DamageType
    {
        Normal = 0,
        Laser = 1,
        Explosive = 2,
        Contact = 3,
        Plasma = 4,
        Kinetic = 5,
        Energy = 6,
    }

    /// <summary>Immutable description of a damage request.</summary>
    public readonly struct DamageInfo
    {
        public readonly float Amount;
        public readonly DamageType Type;
        public readonly DamageSource Source;
        /// <summary>When true the damage skips the shield and hits the hull directly.</summary>
        public readonly bool IgnoreShield;
        /// <summary>When true temporary invulnerability does not block this damage.</summary>
        public readonly bool IgnoreInvulnerability;
        /// <summary>True when the hit rolled a critical (already included in Amount).</summary>
        public readonly bool Critical;

        public DamageInfo(float amount, DamageSource source, DamageType type = DamageType.Normal,
            bool ignoreShield = false, bool ignoreInvulnerability = false, bool critical = false)
        {
            Amount = amount;
            Source = source;
            Type = type;
            IgnoreShield = ignoreShield;
            IgnoreInvulnerability = ignoreInvulnerability;
            Critical = critical;
        }

        /// <summary>Rolls a critical hit. Pure so it can be tested; caller supplies the random value in [0,1).</summary>
        public static float ApplyCritical(float amount, float critChance, float critMultiplier, float roll, out bool isCritical)
        {
            isCritical = critChance > 0f && roll < critChance;
            return isCritical ? amount * critMultiplier : amount;
        }
    }

    /// <summary>Outcome of a damage request.</summary>
    public readonly struct DamageResult
    {
        public readonly bool Applied;
        public readonly bool BlockedByInvulnerability;
        public readonly float ShieldDamage;
        public readonly float HullDamage;
        public readonly bool Killed;
        public readonly bool ShieldBroken;

        public DamageResult(bool applied, bool blocked, float shieldDamage, float hullDamage, bool killed, bool shieldBroken)
        {
            Applied = applied;
            BlockedByInvulnerability = blocked;
            ShieldDamage = shieldDamage;
            HullDamage = hullDamage;
            Killed = killed;
            ShieldBroken = shieldBroken;
        }

        public static readonly DamageResult None = new DamageResult(false, false, 0f, 0f, false, false);
        public static readonly DamageResult Blocked = new DamageResult(false, true, 0f, 0f, false, false);
    }
}
