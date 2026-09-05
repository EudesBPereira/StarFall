using System;
using System.Collections.Generic;
using Starfall.Logic;
using Starfall.PowerUps;
using UnityEngine;

namespace Starfall.Player
{
    /// <summary>
    /// Applies power-ups to the ship and tracks temporary effects. Rules (renew, no stacking) live in
    /// <see cref="StatusEffectTracker"/> and docs/DECISIONS.md.
    /// </summary>
    public sealed class PlayerStatusEffects : MonoBehaviour
    {
        private readonly StatusEffectTracker _tracker = new StatusEffectTracker();
        private readonly Dictionary<PowerUpKind, float> _magnitudes = new Dictionary<PowerUpKind, float>();

        public StatusEffectTracker Tracker => _tracker;
        public float SpeedMultiplier { get; private set; } = 1f;
        public float DamageMultiplier { get; private set; } = 1f;
        public bool Invincible { get; private set; }

        public event Action Changed;

        private void Awake()
        {
            _tracker.Applied += OnTrackerChanged;
            _tracker.Expired += OnTrackerChanged;
        }

        private void OnDestroy()
        {
            _tracker.Applied -= OnTrackerChanged;
            _tracker.Expired -= OnTrackerChanged;
        }

        private void Update()
        {
            _tracker.Tick(Time.deltaTime);
        }

        /// <summary>Applies a timed effect (speed, damage, invincibility). Instant effects are handled by PlayerShip.</summary>
        public void ApplyTimed(PowerUpDefinition def)
        {
            if (def == null) return;
            _magnitudes[def.Kind] = def.Magnitude;
            _tracker.Apply(def.Kind, def.Duration);
        }

        public bool IsActive(PowerUpKind kind) => _tracker.IsActive(kind);

        /// <summary>Removes every temporary effect (death, stage end).</summary>
        public void ClearTemporary()
        {
            _tracker.ClearAll();
            Recompute();
        }

        private void OnTrackerChanged(PowerUpKind _) => Recompute();

        private void Recompute()
        {
            SpeedMultiplier = _tracker.IsActive(PowerUpKind.SpeedBoost) ? Magnitude(PowerUpKind.SpeedBoost, 1.35f) : 1f;
            DamageMultiplier = _tracker.IsActive(PowerUpKind.DamageBoost) ? Magnitude(PowerUpKind.DamageBoost, 2f) : 1f;
            Invincible = _tracker.IsActive(PowerUpKind.Invincibility);
            Changed?.Invoke();
        }

        private float Magnitude(PowerUpKind kind, float fallback)
        {
            return _magnitudes.TryGetValue(kind, out var m) && m > 0f ? m : fallback;
        }
    }
}
