using Starfall.Combat;
using Starfall.Logic;
using UnityEngine;

namespace Starfall.Player
{
    /// <summary>
    /// Final ship numbers for a run: ship definition x permanent upgrades. Computed once when the ship spawns
    /// so gameplay code never reads the save directly.
    /// </summary>
    public readonly struct PlayerLoadout
    {
        public readonly ShipDefinition Ship;
        public readonly WeaponDefinition Weapon;
        public readonly float MaxHull;
        public readonly float MaxShield;
        public readonly float ShieldRegenPerSecond;
        public readonly float ShieldRegenDelay;
        public readonly float MoveSpeed;
        public readonly float Acceleration;
        public readonly float Deceleration;
        public readonly float DamageMultiplier;
        public readonly float FireRateMultiplier;
        public readonly float RangeMultiplier;
        public readonly float CritChance;
        public readonly float CritMultiplier;
        public readonly float UltimateChargeMultiplier;
        public readonly float UltimatePowerMultiplier;

        public PlayerLoadout(ShipDefinition ship, WeaponDefinition weapon, in LoadoutModifiers mods)
        {
            Ship = ship;
            Weapon = weapon;
            float hull = ship != null ? ship.MaxHull : 100f;
            float shield = ship != null ? ship.MaxShield : 50f;
            MaxHull = Mathf.Round(hull * mods.HullMultiplier);
            MaxShield = Mathf.Round(shield * mods.ShieldMultiplier);
            ShieldRegenPerSecond = (ship != null ? ship.ShieldRegenPerSecond : 0f) + mods.ShieldRegenPerSecondBonus;
            ShieldRegenDelay = ship != null ? ship.ShieldRegenDelay : 4f;
            MoveSpeed = (ship != null ? ship.MoveSpeed : 9f) * mods.SpeedMultiplier;
            Acceleration = (ship != null ? ship.Acceleration : 60f) * mods.AccelerationMultiplier;
            Deceleration = (ship != null ? ship.Deceleration : 80f) * mods.AccelerationMultiplier;
            DamageMultiplier = (ship != null ? ship.DamageMultiplier : 1f) * mods.DamageMultiplier;
            FireRateMultiplier = mods.FireRateMultiplier;
            RangeMultiplier = mods.RangeMultiplier;
            CritChance = ship != null ? ship.CritChance : 0.05f;
            CritMultiplier = ship != null ? ship.CritMultiplier : 2f;
            UltimateChargeMultiplier = (ship != null ? ship.UltimateChargeMultiplier : 1f) * mods.UltimateChargeMultiplier;
            UltimatePowerMultiplier = (ship != null ? ship.UltimatePowerMultiplier : 1f) * mods.UltimatePowerMultiplier;
        }

        /// <summary>Builds the loadout from the save (selected ship/weapon + upgrades) with safe fallbacks.</summary>
        public static PlayerLoadout FromSave(SaveData save, ShipDefinition[] ships, WeaponDefinition[] weapons, ShipDefinition fallbackShip)
        {
            ShipDefinition ship = fallbackShip;
            WeaponDefinition weapon = null;
            var mods = LoadoutModifiers.Identity;
            if (save != null)
            {
                if (ships != null) foreach (var s in ships) if (s != null && (int)s.Id == save.selectedShip) ship = s;
                if (weapons != null) foreach (var w in weapons) if (w != null && (int)w.Id == save.selectedWeapon) weapon = w;
                mods = UpgradeCatalog.Compute(save.upgradeLevels);
            }
            if (weapon == null && ship != null) weapon = ship.Weapon;
            if (weapon == null && weapons != null && weapons.Length > 0) weapon = weapons[0];
            return new PlayerLoadout(ship, weapon, mods);
        }
    }
}
