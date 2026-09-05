using System;
using System.Collections.Generic;
using Starfall.Logic;
using Starfall.PowerUps;
using UnityEngine;

namespace Starfall.Player
{
    /// <summary>
    /// Applies power-ups and debuffs to the ship and tracks temporary effects. Rules (renew, no stacking) live in
    /// <see cref="StatusEffectTracker"/> and docs/DECISIONS.md.
    /// </summary>
    public sealed class PlayerStatusEffects : MonoBehaviour
    {
        private const float SlowedMultiplier = 0.55f;

        private readonly StatusEffectTracker _tracker = new StatusEffectTracker();
        private readonly Dictionary<PowerUpKind, float> _magnitudes = new Dictionary<PowerUpKind, float>();

        public StatusEffectTracker Tracker => _tracker;
        public float SpeedMultiplier { get; private set; } = 1f;
        public float DamageMultiplier { get; private set; } = 1f;
        public bool Invincible { get; private set; }
        public bool Slowed { get; private set; }

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

        /// <summary>Applies a negative effect (e.g. Slowed by a web). Invincible ships shrug it off.</summary>
        public void ApplyDebuff(PowerUpKind kind, float seconds)
        {
            if (Invincible || seconds <= 0f) return;
            _tracker.Apply(kind, seconds);
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
            Slowed = _tracker.IsActive(PowerUpKind.Slowed);
            float speed = _tracker.IsActive(PowerUpKind.SpeedBoost) ? Magnitude(PowerUpKind.SpeedBoost, 1.35f) : 1f;
            if (Slowed) speed *= SlowedMultiplier;
            SpeedMultiplier = speed;
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
