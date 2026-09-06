// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System;

namespace Starfall.Logic
{
    /// <summary>Risk Zone states (STAR RISK plan §5.1). Order matters: higher index = more danger.</summary>
    public enum RiskState
    {
        Safe = 0,
        Alert = 1,
        Danger = 2,
        Extreme = 3,
    }

    /// <summary>Tunables for the risk model. Defaults documented in docs/BALANCING.md.</summary>
    public struct RiskSettings
    {
        /// <summary>Threat needed to reach each state (index 1..3). Threat is the smoothed value in [0, ~2].</summary>
        public float AlertThreshold;
        public float DangerThreshold;
        public float ExtremeThreshold;
        /// <summary>How fast the smoothed level chases a higher threat (units per second).</summary>
        public float RiseRate;
        /// <summary>How fast it drops when threat is lower (units per second). Lower than rise = hysteresis.</summary>
        public float FallRate;
        /// <summary>Minimum seconds a state is kept before it can drop, to avoid flicker.</summary>
        public float MinStateHold;
        /// <summary>Score multiplier per state (index 0..3).</summary>
        public float SafeMultiplier, AlertMultiplier, DangerMultiplier, ExtremeMultiplier;
        /// <summary>Multiplier scale while Overdrive is active; result is capped at OverdriveCap.</summary>
        public float OverdriveScale;
        public float OverdriveCap;

        public static RiskSettings Default => new RiskSettings
        {
            AlertThreshold = 0.25f,
            DangerThreshold = 0.6f,
            ExtremeThreshold = 1.1f,
            RiseRate = 3.5f,
            FallRate = 1.2f,
            MinStateHold = 0.35f,
            SafeMultiplier = 1f,
            AlertMultiplier = 2f,
            DangerMultiplier = 3f,
            ExtremeMultiplier = 5f,
            OverdriveScale = 1.6f,
            OverdriveCap = 8f,
        };
    }

    /// <summary>
    /// Smooths a raw threat reading into a stable risk state with hysteresis, and exposes the score multiplier.
    /// The raw threat is produced by the sensor (distance-weighted sum of nearby enemies and projectiles).
    /// </summary>
    public sealed class RiskModel
    {
        private readonly RiskSettings _settings;
        private float _level;
        private float _stateAge;
        private float _timeInDanger;

        public RiskState State { get; private set; } = RiskState.Safe;
        /// <summary>Smoothed threat level (0 = nothing nearby).</summary>
        public float Level => _level;
        /// <summary>Normalized progress inside the current state, for HUD fills.</summary>
        public float Fraction => Math.Clamp(_level / Math.Max(0.01f, _settings.ExtremeThreshold), 0f, 1f);
        /// <summary>Seconds spent in Danger or Extreme without dropping below (feeds Overdrive).</summary>
        public float TimeInDanger => _timeInDanger;
        public bool OverdriveActive { get; set; }

        public event Action<RiskState, RiskState> StateChanged;

        public RiskModel(RiskSettings settings)
        {
            _settings = settings;
        }

        public RiskModel() : this(RiskSettings.Default) { }

        /// <summary>Feeds one sensor reading. Call at a fixed cadence (e.g. 10 Hz) with the elapsed time.</summary>
        public void Tick(float rawThreat, float deltaTime)
        {
            if (deltaTime <= 0f) return;
            rawThreat = Math.Max(0f, rawThreat);
            float rate = rawThreat > _level ? _settings.RiseRate : _settings.FallRate;
            _level = MoveTowards(_level, rawThreat, rate * deltaTime);
            _stateAge += deltaTime;

            var target = Classify(_level);
            if (target > State || (target < State && _stateAge >= _settings.MinStateHold))
            {
                var previous = State;
                State = target;
                _stateAge = 0f;
                StateChanged?.Invoke(previous, State);
            }

            if (State >= RiskState.Danger) _timeInDanger += deltaTime;
            else _timeInDanger = 0f;
        }

        /// <summary>Base multiplier of the current state (without Overdrive).</summary>
        public float StateMultiplier => MultiplierFor(State);

        /// <summary>Score multiplier including the Overdrive scale (capped).</summary>
        public float Multiplier
        {
            get
            {
                float m = StateMultiplier;
                if (OverdriveActive) m = Math.Min(_settings.OverdriveCap, m * _settings.OverdriveScale);
                return m;
            }
        }

        public float MultiplierFor(RiskState state)
        {
            switch (state)
            {
                case RiskState.Alert: return _settings.AlertMultiplier;
                case RiskState.Danger: return _settings.DangerMultiplier;
                case RiskState.Extreme: return _settings.ExtremeMultiplier;
                default: return _settings.SafeMultiplier;
            }
        }

        public void Reset()
        {
            _level = 0f;
            _stateAge = 0f;
            _timeInDanger = 0f;
            OverdriveActive = false;
            if (State != RiskState.Safe)
            {
                var previous = State;
                State = RiskState.Safe;
                StateChanged?.Invoke(previous, State);
            }
        }

        private RiskState Classify(float level)
        {
            if (level >= _settings.ExtremeThreshold) return RiskState.Extreme;
            if (level >= _settings.DangerThreshold) return RiskState.Danger;
            if (level >= _settings.AlertThreshold) return RiskState.Alert;
            return RiskState.Safe;
        }

        private static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Math.Abs(target - current) <= maxDelta) return target;
            return current + Math.Sign(target - current) * maxDelta;
        }

        /// <summary>
        /// Threat contribution of one object: weight scaled by closeness (1 at distance 0, 0 at the sensor edge),
        /// squared so that near objects dominate. Pure so the sensor and the tests share the formula.
        /// </summary>
        public static float ThreatContribution(float distance, float sensorRadius, float weight)
        {
            if (sensorRadius <= 0f || distance >= sensorRadius || weight <= 0f) return 0f;
            float closeness = 1f - Math.Max(0f, distance) / sensorRadius;
            return weight * closeness * closeness;
        }
    }
}
