// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;

namespace Starfall.Logic
{
    /// <summary>
    /// Shield + hull health model. Shield absorbs damage before hull, death fires exactly once.
    /// </summary>
    public sealed class HealthModel
    {
        public float MaxHull { get; private set; }
        public float MaxShield { get; private set; }
        public float Hull { get; private set; }
        public float Shield { get; private set; }
        public bool IsAlive { get; private set; }
        public bool Invulnerable { get; set; }

        public float HullFraction => MaxHull <= 0f ? 0f : Hull / MaxHull;
        public float ShieldFraction => MaxShield <= 0f ? 0f : Shield / MaxShield;
        public bool HasShield => Shield > 0f;

        public event Action<DamageInfo, DamageResult> Damaged;
        public event Action<DamageInfo> Died;
        public event Action Changed;

        public HealthModel(float maxHull, float maxShield)
        {
            Configure(maxHull, maxShield);
        }

        /// <summary>Redefines maximum values and fully restores the entity.</summary>
        public void Configure(float maxHull, float maxShield)
        {
            MaxHull = Math.Max(0f, maxHull);
            MaxShield = Math.Max(0f, maxShield);
            ResetToFull();
        }

        public void ResetToFull()
        {
            Hull = MaxHull;
            Shield = MaxShield;
            IsAlive = MaxHull > 0f;
            Invulnerable = false;
            Changed?.Invoke();
        }

        public DamageResult TakeDamage(in DamageInfo info)
        {
            if (!IsAlive || info.Amount <= 0f)
                return DamageResult.None;

            if (Invulnerable && !info.IgnoreInvulnerability)
                return DamageResult.Blocked;

            float remaining = info.Amount;
            float shieldDamage = 0f;
            bool shieldBroken = false;

            if (!info.IgnoreShield && Shield > 0f)
            {
                shieldDamage = Math.Min(Shield, remaining);
                Shield -= shieldDamage;
                remaining -= shieldDamage;
                shieldBroken = Shield <= 0f;
            }

            float hullDamage = 0f;
            if (remaining > 0f)
            {
                hullDamage = Math.Min(Hull, remaining);
                Hull -= hullDamage;
            }

            bool killed = Hull <= 0f;
            if (killed)
            {
                Hull = 0f;
                IsAlive = false;
            }

            var result = new DamageResult(true, false, shieldDamage, hullDamage, killed, shieldBroken);
            Changed?.Invoke();
            Damaged?.Invoke(info, result);
            if (killed)
                Died?.Invoke(info);
            return result;
        }

        /// <summary>Kills immediately regardless of invulnerability. Fires Died once.</summary>
        public void Kill(DamageSource source)
        {
            if (!IsAlive) return;
            var info = new DamageInfo(Hull + Shield, source, DamageType.Normal, true, true);
            TakeDamage(info);
        }

        public void RestoreShield(float amount)
        {
            if (!IsAlive || amount <= 0f) return;
            Shield = Math.Min(MaxShield, Shield + amount);
            Changed?.Invoke();
        }

        public void RestoreHull(float amount)
        {
            if (!IsAlive || amount <= 0f) return;
            Hull = Math.Min(MaxHull, Hull + amount);
            Changed?.Invoke();
        }
    }
}
