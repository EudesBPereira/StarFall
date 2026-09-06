// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;

namespace Starfall.Logic
{
    /// <summary>Tunables for Overdrive (STAR RISK plan §5.2). Defaults documented in docs/BALANCING.md.</summary>
    public struct OverdriveSettings
    {
        public float Max;
        /// <summary>Gain per second while in Danger / Extreme.</summary>
        public float DangerGainPerSecond;
        public float ExtremeGainPerSecond;
        /// <summary>Drain per second while Safe (not active).</summary>
        public float SafeDrainPerSecond;
        public float GrazeGain;
        public float CloseKillGain;
        public float ComboKillGain;
        public float BossPartGain;
        /// <summary>Flat loss when the player takes damage (inactive) or fraction of the remaining time (active).</summary>
        public float DamageLoss;
        public float DamageActiveFraction;
        /// <summary>Seconds the active state lasts at full meter.</summary>
        public float ActiveSeconds;
        /// <summary>Fire-rate multiplier granted while active (faction hook; Federation default).</summary>
        public float FireRateBonus;
        public float UltimateChargeBonus;

        public static OverdriveSettings Default => new OverdriveSettings
        {
            Max = 100f,
            DangerGainPerSecond = 7f,
            ExtremeGainPerSecond = 14f,
            SafeDrainPerSecond = 5f,
            GrazeGain = 4f,
            CloseKillGain = 5f,
            ComboKillGain = 1f,
            BossPartGain = 10f,
            DamageLoss = 30f,
            DamageActiveFraction = 0.5f,
            ActiveSeconds = 8f,
            FireRateBonus = 1.15f,
            UltimateChargeBonus = 1.5f,
        };
    }

    /// <summary>
    /// Skill-earned temporary power state. The meter fills from risky play and, once full, activates
    /// automatically (touch friendly); it then drains over <see cref="OverdriveSettings.ActiveSeconds"/>.
    /// Taking damage while active cuts the remaining time; dying ends it and empties the meter.
    /// </summary>
    public sealed class OverdriveModel
    {
        private readonly OverdriveSettings _settings;
        private float _meter;
        private float _remaining;

        public bool IsActive { get; private set; }
        public float Meter => _meter;
        public float Max => _settings.Max;
        /// <summary>Meter fraction while charging, remaining-time fraction while active.</summary>
        public float Fraction => IsActive
            ? Math.Clamp(_remaining / Math.Max(0.01f, _settings.ActiveSeconds), 0f, 1f)
            : Math.Clamp(_meter / Math.Max(0.01f, _settings.Max), 0f, 1f);
        public float RemainingSeconds => IsActive ? _remaining : 0f;
        public float FireRateMultiplier => IsActive ? _settings.FireRateBonus : 1f;
        public float UltimateChargeMultiplier => IsActive ? _settings.UltimateChargeBonus : 1f;
        public int Activations { get; private set; }

        public event Action Activated;
        public event Action Ended;
        public event Action Changed;

        public OverdriveModel(OverdriveSettings settings)
        {
            _settings = settings;
        }

        public OverdriveModel() : this(OverdriveSettings.Default) { }

        /// <summary>Time-based update. Pass the current risk state so the meter charges or drains accordingly.</summary>
        public void Tick(RiskState risk, float deltaTime)
        {
            if (deltaTime <= 0f) return;
            if (IsActive)
            {
                _remaining -= deltaTime;
                if (_remaining <= 0f) End();
                else Changed?.Invoke();
                return;
            }

            switch (risk)
            {
                case RiskState.Extreme: Add(_settings.ExtremeGainPerSecond * deltaTime); break;
                case RiskState.Danger: Add(_settings.DangerGainPerSecond * deltaTime); break;
                case RiskState.Safe: Add(-_settings.SafeDrainPerSecond * deltaTime); break;
                default: break; // Alert: holds
            }
        }

        public void OnGraze() => Add(_settings.GrazeGain);
        public void OnCloseKill() => Add(_settings.CloseKillGain);
        public void OnComboKill() => Add(_settings.ComboKillGain);
        public void OnBossPartDestroyed() => Add(_settings.BossPartGain);

        /// <summary>Damage taken: loses meter when charging, loses remaining time when active.</summary>
        public void OnDamaged()
        {
            if (IsActive)
            {
                _remaining *= 1f - Math.Clamp(_settings.DamageActiveFraction, 0f, 1f);
                if (_remaining <= 0.05f) End();
                else Changed?.Invoke();
            }
            else
            {
                Add(-_settings.DamageLoss);
            }
        }

        /// <summary>Ship destroyed: everything resets.</summary>
        public void OnDeath()
        {
            if (IsActive) End();
            _meter = 0f;
            Changed?.Invoke();
        }

        public void Reset()
        {
            IsActive = false;
            _meter = 0f;
            _remaining = 0f;
            Changed?.Invoke();
        }

        private void Add(float amount)
        {
            if (IsActive || amount == 0f) return;
            float before = _meter;
            _meter = Math.Clamp(_meter + amount, 0f, _settings.Max);
            if (Math.Abs(_meter - before) > 0.0001f) Changed?.Invoke();
            if (_meter >= _settings.Max) Activate();
        }

        private void Activate()
        {
            IsActive = true;
            _meter = 0f;
            _remaining = _settings.ActiveSeconds;
            Activations++;
            Activated?.Invoke();
            Changed?.Invoke();
        }

        private void End()
        {
            IsActive = false;
            _remaining = 0f;
            Ended?.Invoke();
            Changed?.Invoke();
        }
    }
}
