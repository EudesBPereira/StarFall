using Starfall.Combat;
using Starfall.Logic;
using UnityEngine;

namespace Starfall.Player
{
    /// <summary>Playable ship data (GDD §15). Values documented in docs/BALANCING.md.</summary>
    [CreateAssetMenu(menuName = "Starfall/Ships/Ship Definition", fileName = "Ship")]
    public sealed class ShipDefinition : ScriptableObject
    {
        [Header("Identity")]
        public ShipId Id = ShipId.Vanguard;
        public FactionId Faction = FactionId.Federation;
        public string DisplayName = "SF-01 Vanguard";
        [TextArea] public string Description = "Experimental interceptor of the Earth Defense Fleet.";
        [Tooltip("Credits to unlock in the Hangar (0 = starter or campaign reward).")]
        [Min(0)] public int Cost = 0;
        public string UnlockHint = "";

        [Header("Durability")]
        [Min(1f)] public float MaxHull = 100f;
        [Min(0f)] public float MaxShield = 50f;
        [Tooltip("Shield points regenerated per second after the regen delay.")]
        [Min(0f)] public float ShieldRegenPerSecond = 0f;
        [Min(0f)] public float ShieldRegenDelay = 4f;

        [Header("Movement")]
        [Min(0.1f)] public float MoveSpeed = 9f;
        [Min(0.1f)] public float Acceleration = 60f;
        [Min(0.1f)] public float Deceleration = 80f;
        [Tooltip("Bank angle (degrees) at full horizontal speed.")]
        [Min(0f)] public float BankAngle = 18f;

        [Header("Combat")]
        [Range(0f, 1f)] public float CritChance = 0.05f;
        [Min(1f)] public float CritMultiplier = 2f;
        [Tooltip("Multiplier on all weapon damage.")]
        [Min(0.1f)] public float DamageMultiplier = 1f;
        [Tooltip("Multiplier on energy gained per kill.")]
        [Min(0.1f)] public float UltimateChargeMultiplier = 1f;
        [Tooltip("Multiplier on Ultimate damage to elites/bosses.")]
        [Min(0.1f)] public float UltimatePowerMultiplier = 1f;
        [Tooltip("Default weapon when none is selected.")]
        public WeaponDefinition Weapon;

        [Header("Risk / Overdrive (plan §5.2, §6)")]
        [Tooltip("Scales how fast the Overdrive meter fills. Below 1 = more demanding (Nova-X).")]
        [Min(0.1f)] public float OverdriveGainMultiplier = 1f;
        [Tooltip("Federation: fire-rate multiplier while Overdrive is active.")]
        [Min(1f)] public float OverdriveFireRateBonus = 1.15f;
        [Tooltip("Biomech: shield regenerated per second while Overdrive is active.")]
        [Min(0f)] public float OverdriveShieldRegen = 0f;
        [Tooltip("Cyber: extra critical chance while Overdrive is active.")]
        [Range(0f, 1f)] public float OverdriveCritBonus = 0f;
        [Tooltip("Federation passive: damage bonus per consecutive hit without missing (capped).")]
        [Range(0f, 0.05f)] public float PrecisionBonusPerHit = 0f;

        [Header("Faction kit (plan §6)")]
        public UltimateKind Ultimate = UltimateKind.OrbitalStrike;
        [Tooltip("Cyber: hits mark the target for extra damage.")]
        public bool MarksTargets = false;
        [Tooltip("Cyber: orbiting drone that fires on its own.")]
        public bool HasCompanionDrone = false;
        [Min(0f)] public float DroneDamage = 4f;
        [Min(0.05f)] public float DroneInterval = 0.5f;
        [Tooltip("Biomech: fraction of max hull restored per kill (capped at 5%).")]
        [Range(0f, 0.05f)] public float LifestealPerKill = 0f;

        [Header("Visuals")]
        public Sprite Sprite;
        public Color Tint = Color.white;
        public Color ThrusterColor = new Color(0.4f, 0.8f, 1f, 0.9f);
        [Min(0.05f)] public float HitboxRadius = 0.28f;
        [Min(0.1f)] public float VisualScale = 1f;
    }
}
