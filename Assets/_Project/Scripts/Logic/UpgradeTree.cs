// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;

namespace Starfall.Logic
{
    /// <summary>Permanent upgrade nodes (GDD §14). Index order must match SaveData.upgradeLevels.</summary>
    public enum UpgradeNode
    {
        WeaponDamage = 0,
        WeaponFireRate = 1,
        WeaponRange = 2,
        ShieldCapacity = 3,
        ShieldRegen = 4,
        HullMax = 5,
        MobilitySpeed = 6,
        MobilityAcceleration = 7,
        SpecialRecharge = 8,
        SpecialPower = 9,
    }

    /// <summary>Multipliers applied to the ship stats after upgrades (1 = no change).</summary>
    public struct LoadoutModifiers
    {
        public float DamageMultiplier;
        public float FireRateMultiplier;
        public float RangeMultiplier;
        public float ShieldMultiplier;
        public float ShieldRegenPerSecondBonus;
        public float HullMultiplier;
        public float SpeedMultiplier;
        public float AccelerationMultiplier;
        public float UltimateChargeMultiplier;
        public float UltimatePowerMultiplier;

        public static LoadoutModifiers Identity => new LoadoutModifiers
        {
            DamageMultiplier = 1f, FireRateMultiplier = 1f, RangeMultiplier = 1f, ShieldMultiplier = 1f,
            ShieldRegenPerSecondBonus = 0f, HullMultiplier = 1f, SpeedMultiplier = 1f, AccelerationMultiplier = 1f,
            UltimateChargeMultiplier = 1f, UltimatePowerMultiplier = 1f,
        };
    }

    /// <summary>Costs, bonuses and purchase rules of the upgrade tree. Values in docs/BALANCING.md.</summary>
    public static class UpgradeCatalog
    {
        public const int MaxLevel = 5;
        public const int NodeCount = 10;

        private static readonly int[] BaseCost = { 400, 400, 300, 350, 450, 350, 300, 250, 500, 600 };
        private static readonly float[] BonusPerLevel = { 0.08f, 0.07f, 0.10f, 0.12f, 1.0f, 0.10f, 0.05f, 0.08f, 0.10f, 0.15f };

        public static string DisplayName(UpgradeNode node)
        {
            switch (node)
            {
                case UpgradeNode.WeaponDamage: return "Weapon Damage";
                case UpgradeNode.WeaponFireRate: return "Fire Rate";
                case UpgradeNode.WeaponRange: return "Weapon Range";
                case UpgradeNode.ShieldCapacity: return "Shield Capacity";
                case UpgradeNode.ShieldRegen: return "Shield Regeneration";
                case UpgradeNode.HullMax: return "Hull Integrity";
                case UpgradeNode.MobilitySpeed: return "Engine Speed";
                case UpgradeNode.MobilityAcceleration: return "Thrusters";
                case UpgradeNode.SpecialRecharge: return "Ultimate Recharge";
                case UpgradeNode.SpecialPower: return "Ultimate Power";
                default: return node.ToString();
            }
        }

        public static string Category(UpgradeNode node)
        {
            switch (node)
            {
                case UpgradeNode.WeaponDamage:
                case UpgradeNode.WeaponFireRate:
                case UpgradeNode.WeaponRange: return "WEAPONS";
                case UpgradeNode.ShieldCapacity:
                case UpgradeNode.ShieldRegen: return "SHIELD";
                case UpgradeNode.HullMax: return "HULL";
                case UpgradeNode.MobilitySpeed:
                case UpgradeNode.MobilityAcceleration: return "MOBILITY";
                default: return "SPECIAL";
            }
        }

        /// <summary>Bonus description for one level (e.g. "+8% damage").</summary>
        public static string BonusLabel(UpgradeNode node)
        {
            float b = BonusPerLevel[(int)node];
            switch (node)
            {
                case UpgradeNode.ShieldRegen: return $"+{b:0.#} shield/s";
                case UpgradeNode.WeaponFireRate: return $"+{b * 100f:0}% fire rate";
                case UpgradeNode.WeaponRange: return $"+{b * 100f:0}% range";
                case UpgradeNode.ShieldCapacity: return $"+{b * 100f:0}% shield";
                case UpgradeNode.HullMax: return $"+{b * 100f:0}% hull";
                case UpgradeNode.MobilitySpeed: return $"+{b * 100f:0}% speed";
                case UpgradeNode.MobilityAcceleration: return $"+{b * 100f:0}% acceleration";
                case UpgradeNode.SpecialRecharge: return $"+{b * 100f:0}% energy gain";
                case UpgradeNode.SpecialPower: return $"+{b * 100f:0}% ultimate damage";
                default: return $"+{b * 100f:0}% damage";
            }
        }

        /// <summary>Credits needed to go from <paramref name="currentLevel"/> to the next one.</summary>
        public static int CreditCost(UpgradeNode node, int currentLevel)
        {
            if (currentLevel >= MaxLevel) return 0;
            return (int)Math.Round(BaseCost[(int)node] * Math.Pow(currentLevel + 1, 1.6));
        }

        /// <summary>Components needed for the next level (only the last levels ask for parts).</summary>
        public static int ComponentCost(UpgradeNode node, int currentLevel)
        {
            if (currentLevel >= MaxLevel) return 0;
            return currentLevel >= 2 ? currentLevel - 1 : 0;
        }

        public static bool CanPurchase(SaveData save, UpgradeNode node)
        {
            if (save == null) return false;
            int level = save.GetUpgradeLevel((int)node);
            if (level >= MaxLevel) return false;
            return save.credits >= CreditCost(node, level) && save.components >= ComponentCost(node, level);
        }

        /// <summary>Buys one level. Returns false when not affordable or maxed.</summary>
        public static bool Purchase(SaveData save, UpgradeNode node)
        {
            if (!CanPurchase(save, node)) return false;
            int level = save.GetUpgradeLevel((int)node);
            save.credits -= CreditCost(node, level);
            save.components -= ComponentCost(node, level);
            save.upgradeLevels[(int)node] = level + 1;
            return true;
        }

        public static bool AllMaxed(SaveData save)
        {
            if (save == null) return false;
            for (int i = 0; i < NodeCount; i++)
                if (save.GetUpgradeLevel(i) < MaxLevel) return false;
            return true;
        }

        public static LoadoutModifiers Compute(int[] levels)
        {
            var m = LoadoutModifiers.Identity;
            if (levels == null) return m;
            float L(UpgradeNode n) => (int)n < levels.Length ? Math.Clamp(levels[(int)n], 0, MaxLevel) : 0;
            m.DamageMultiplier = 1f + BonusPerLevel[0] * L(UpgradeNode.WeaponDamage);
            m.FireRateMultiplier = 1f + BonusPerLevel[1] * L(UpgradeNode.WeaponFireRate);
            m.RangeMultiplier = 1f + BonusPerLevel[2] * L(UpgradeNode.WeaponRange);
            m.ShieldMultiplier = 1f + BonusPerLevel[3] * L(UpgradeNode.ShieldCapacity);
            m.ShieldRegenPerSecondBonus = BonusPerLevel[4] * L(UpgradeNode.ShieldRegen);
            m.HullMultiplier = 1f + BonusPerLevel[5] * L(UpgradeNode.HullMax);
            m.SpeedMultiplier = 1f + BonusPerLevel[6] * L(UpgradeNode.MobilitySpeed);
            m.AccelerationMultiplier = 1f + BonusPerLevel[7] * L(UpgradeNode.MobilityAcceleration);
            m.UltimateChargeMultiplier = 1f + BonusPerLevel[8] * L(UpgradeNode.SpecialRecharge);
            m.UltimatePowerMultiplier = 1f + BonusPerLevel[9] * L(UpgradeNode.SpecialPower);
            return m;
        }
    }
}
