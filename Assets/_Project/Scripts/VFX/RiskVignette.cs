using Starfall.Combat;
using Starfall.Core;
using Starfall.Logic;
using Starfall.Save;
using UnityEngine;

namespace Starfall.VFX
{
    /// <summary>
    /// Screen-edge tint that follows the Risk Zone state so the player feels the danger level without reading
    /// the HUD (plan §5.1). Overdrive adds a cyan/magenta pulse. Honors the "reduced effects" setting.
    /// Colors follow the plan palette (§13.4): alert #FFD166, danger #FF7B35, extreme #FF3CAC, ally #24D6FF.
    /// </summary>
    public sealed class RiskVignette : MonoBehaviour
    {
        [SerializeField] internal SpriteRenderer ring;

        private static readonly Color SafeColor = new Color(0.14f, 0.84f, 1f, 0f);
        private static readonly Color AlertColor = new Color(1f, 0.82f, 0.4f, 0.22f);
        private static readonly Color DangerColor = new Color(1f, 0.48f, 0.21f, 0.34f);
        private static readonly Color ExtremeColor = new Color(1f, 0.24f, 0.67f, 0.46f);
        private static readonly Color OverdriveA = new Color(0.14f, 0.84f, 1f, 0.5f);
        private static readonly Color OverdriveB = new Color(1f, 0.24f, 0.67f, 0.5f);

        private RiskState _state = RiskState.Safe;
        private bool _overdrive;
        private Color _current;
        private Color _target;
        private float _pulse;

        private void OnEnable()
        {
            GameSignals.RiskStateChanged += OnRiskStateChanged;
            GameSignals.OverdriveChanged += OnOverdriveChanged;
            GameSignals.PlayerDied += OnPlayerDied;
            _target = SafeColor;
            _current = SafeColor;
            Fit();
        }

        private void OnDisable()
        {
            GameSignals.RiskStateChanged -= OnRiskStateChanged;
            GameSignals.OverdriveChanged -= OnOverdriveChanged;
            GameSignals.PlayerDied -= OnPlayerDied;
        }

        private void Fit()
        {
            var ctx = GameplayContext.Current;
            if (ring == null || ctx == null || ctx.PlayArea == null || ring.sprite == null) return;
            var area = ctx.PlayArea;
            float diag = Mathf.Sqrt(area.Width * area.Width + area.Height * area.Height);
            float size = ring.sprite.bounds.size.x;
            ring.transform.position = new Vector3(area.Center.x, area.Center.y, 0f);
            ring.transform.localScale = Vector3.one * (size > 0f ? diag * 1.05f / size : 1f);
            ring.sortingOrder = SortingOrders.Vfx + 4;
            ring.color = SafeColor;
        }

        private void OnRiskStateChanged(RiskState from, RiskState to)
        {
            _state = to;
            _target = ColorFor(to);
            _pulse = 1f;
        }

        private void OnOverdriveChanged(bool active)
        {
            _overdrive = active;
            _pulse = 1f;
        }

        private void OnPlayerDied()
        {
            _state = RiskState.Safe;
            _overdrive = false;
            _target = SafeColor;
        }

        private static Color ColorFor(RiskState s)
        {
            switch (s)
            {
                case RiskState.Alert: return AlertColor;
                case RiskState.Danger: return DangerColor;
                case RiskState.Extreme: return ExtremeColor;
                default: return SafeColor;
            }
        }

        private void Update()
        {
            if (ring == null) return;
            var data = SaveService.Data;
            bool reduced = data != null && data.reducedEffects;
            float dt = Time.unscaledDeltaTime;
            _pulse = Mathf.Max(0f, _pulse - dt * 2f);

            Color target = _target;
            if (_overdrive)
            {
                float t = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f);
                target = Color.Lerp(OverdriveA, OverdriveB, t);
                if (_state == RiskState.Safe) target.a *= 0.6f;
            }
            if (reduced) target.a *= 0.45f;
            target.a += _pulse * (reduced ? 0.08f : 0.18f);

            _current = Color.Lerp(_current, target, dt * 6f);
            ring.color = _current;
            ring.enabled = _current.a > 0.01f;
        }
    }
}
