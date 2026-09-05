using System;
using Starfall.Logic;
using UnityEngine;

namespace Starfall.Combat
{
    /// <summary>
    /// MonoBehaviour adapter over <see cref="HealthModel"/>. Owns shield + hull and exposes Unity-friendly events.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] internal Faction faction = Faction.Enemy;
        [SerializeField, Min(0f)] internal float maxHull = 10f;
        [SerializeField, Min(0f)] internal float maxShield = 0f;

        private HealthModel _model;

        public HealthModel Model => _model ??= new HealthModel(maxHull, maxShield);
        public Faction Faction => faction;
        public bool IsAlive => Model.IsAlive;
        public float Hull => Model.Hull;
        public float Shield => Model.Shield;
        public float MaxHull => Model.MaxHull;
        public float MaxShield => Model.MaxShield;
        public bool Invulnerable
        {
            get => Model.Invulnerable;
            set => Model.Invulnerable = value;
        }

        /// <summary>Fired after damage was applied (not when blocked). Includes the world hit point.</summary>
        public event Action<DamageInfo, DamageResult, Vector2> DamagedAt;
        /// <summary>Fired when damage was blocked by invulnerability.</summary>
        public event Action<Vector2> Blocked;
        public event Action<DamageInfo> Died;
        public event Action Changed;

        private void Awake()
        {
            var model = Model;
            model.Died += OnModelDied;
            model.Changed += OnModelChanged;
        }

        private void OnDestroy()
        {
            if (_model == null) return;
            _model.Died -= OnModelDied;
            _model.Changed -= OnModelChanged;
        }

        public void Configure(Faction newFaction, float hull, float shield)
        {
            faction = newFaction;
            maxHull = hull;
            maxShield = shield;
            Model.Configure(hull, shield);
        }

        public void ResetToFull() => Model.ResetToFull();

        public DamageResult ApplyDamage(in DamageInfo info, Vector2 hitPoint)
        {
            var result = Model.TakeDamage(info);
            if (result.Applied)
                DamagedAt?.Invoke(info, result, hitPoint);
            else if (result.BlockedByInvulnerability)
                Blocked?.Invoke(hitPoint);
            return result;
        }

        public void Kill(DamageSource source) => Model.Kill(source);
        public void RestoreShield(float amount) => Model.RestoreShield(amount);
        public void RestoreHull(float amount) => Model.RestoreHull(amount);

        private void OnModelDied(DamageInfo info) => Died?.Invoke(info);
        private void OnModelChanged() => Changed?.Invoke();
    }
}
