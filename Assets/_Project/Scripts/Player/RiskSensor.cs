using System.Collections.Generic;
using Starfall.Audio;
using Starfall.Combat;
using Starfall.Core;
using Starfall.Enemies;
using Starfall.Logic;
using UnityEngine;

namespace Starfall.Player
{
    /// <summary>
    /// Owns the Risk Zone and Overdrive models for the player ship (plan §5.1–§5.3).
    /// Samples nearby threats at a fixed cadence with non-allocating overlap queries, detects grazes
    /// (enemy projectiles that pass through the graze ring and leave without hitting) and raises signals.
    /// </summary>
    public sealed class RiskSensor : MonoBehaviour
    {
        private const float SampleInterval = 0.1f;
        private const float SensorRadius = 3.6f;
        private const float ProjectileWeight = 0.55f;
        private const float CloseKillDistance = 2.2f;
        private static readonly Collider2D[] Buffer = new Collider2D[64];

        [SerializeField] internal CircleCollider2D hitbox;
        [SerializeField] internal SpriteRenderer hitboxVisual;

        private readonly RiskModel _risk = new RiskModel();
        private readonly OverdriveModel _overdrive = new OverdriveModel();
        private readonly List<Projectile> _pending = new List<Projectile>(16);
        private PlayerShip _ship;
        private float _sampleTimer;
        private float _overdriveGain = 1f;
        /// <summary>Overdrive gain multiplier from the temporary build (Risk Tuner). 1 = none.</summary>
        public float BuildGainMultiplier { get; set; } = 1f;
        private bool _enabledForRun;
        private int _threatMask;

        public RiskModel Risk => _risk;
        public OverdriveModel Overdrive => _overdrive;
        public int Grazes { get; private set; }
        public float SecondsInDanger { get; private set; }
        public float GrazeRadius => GrazeRules.GrazeRadius(HitboxRadius);
        private float HitboxRadius => hitbox != null ? hitbox.radius * Mathf.Abs(transform.lossyScale.x) : 0.28f;

        private void Awake()
        {
            _ship = GetComponent<PlayerShip>();
            _threatMask = (1 << GameLayers.Enemy) | (1 << GameLayers.Obstacle) | (1 << GameLayers.EnemyProjectile);
            _risk.StateChanged += OnRiskStateChanged;
            _overdrive.Activated += OnOverdriveActivated;
            _overdrive.Ended += OnOverdriveEnded;
            _overdrive.Changed += OnOverdriveChanged;
        }

        private void OnDestroy()
        {
            _risk.StateChanged -= OnRiskStateChanged;
            _overdrive.Activated -= OnOverdriveActivated;
            _overdrive.Ended -= OnOverdriveEnded;
            _overdrive.Changed -= OnOverdriveChanged;
        }

        /// <summary>Called by the ship after the loadout is known.</summary>
        public void Configure(in PlayerLoadout loadout, bool showHitbox)
        {
            _overdriveGain = loadout.Ship != null ? Mathf.Max(0.1f, loadout.Ship.OverdriveGainMultiplier) : 1f;
            if (hitboxVisual != null)
            {
                hitboxVisual.enabled = showHitbox;
                hitboxVisual.transform.localScale = Vector3.one * (HitboxRadius * 2f / Mathf.Max(0.01f, hitboxVisual.sprite != null ? hitboxVisual.sprite.bounds.size.x : 1f));
            }
        }

        public void BeginRun()
        {
            _risk.Reset();
            _overdrive.Reset();
            _pending.Clear();
            Grazes = 0;
            SecondsInDanger = 0f;
            _enabledForRun = true;
        }

        public void EndRun()
        {
            _enabledForRun = false;
            _risk.OverdriveActive = false;
        }

        private void Update()
        {
            if (!_enabledForRun || _ship == null || !_ship.IsAlive || !_ship.ControlEnabled) return;
            float dt = Time.deltaTime;
            _overdrive.Tick(_risk.State, dt * _overdriveGain * BuildGainMultiplier);
            _risk.OverdriveActive = _overdrive.IsActive;
            if (_risk.State >= RiskState.Danger) SecondsInDanger += dt;

            _sampleTimer -= dt;
            if (_sampleTimer > 0f) return;
            _sampleTimer = SampleInterval;
            Sample();
        }

        private void Sample()
        {
            Vector2 center = transform.position;
            float threat = 0f;
            int count = Physics2D.OverlapCircleNonAlloc(center, SensorRadius, Buffer, _threatMask);
            float grazeOuter = GrazeRadius;
            bool invulnerable = _ship.Health.Invulnerable;

            for (int i = 0; i < count; i++)
            {
                var col = Buffer[i];
                if (col == null) continue;
                float distance = Vector2.Distance(center, col.ClosestPoint(center));

                if (col.gameObject.layer == GameLayers.EnemyProjectile)
                {
                    threat += RiskModel.ThreatContribution(distance, SensorRadius, ProjectileWeight);
                    if (col.TryGetComponent<Projectile>(out var projectile))
                        ConsiderGraze(projectile, distance, grazeOuter, invulnerable);
                    continue;
                }

                float weight = 1f;
                if (col.TryGetComponent<Enemy>(out var enemy) && enemy.Definition != null)
                {
                    weight = enemy.Definition.RiskWeight;
                    if (enemy.IsBoss) weight *= 1.25f;
                }
                threat += RiskModel.ThreatContribution(distance, SensorRadius, weight);
            }

            _risk.Tick(threat, SampleInterval);
            ResolvePendingGrazes(center, grazeOuter);
        }

        private void ConsiderGraze(Projectile projectile, float distance, float grazeOuter, bool invulnerable)
        {
            if (projectile == null || projectile.Grazed || projectile.HitPlayer || _pending.Contains(projectile)) return;
            if (distance > grazeOuter) return;
            if (!GrazeRules.CanAward(invulnerable, projectile.Grazed, projectile.Spec.Speed)) return;
            _pending.Add(projectile);
        }

        /// <summary>A pending projectile is awarded once it leaves the ring (or despawns) without having hit the ship.</summary>
        private void ResolvePendingGrazes(Vector2 center, float grazeOuter)
        {
            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                var p = _pending[i];
                if (p == null || !p.IsLaunched)
                {
                    if (p != null && !p.HitPlayer) AwardGraze(p, center);
                    _pending.RemoveAt(i);
                    continue;
                }
                if (p.HitPlayer)
                {
                    _pending.RemoveAt(i);
                    continue;
                }
                float d = Vector2.Distance(center, (Vector2)p.transform.position);
                if (d > grazeOuter + 0.25f)
                {
                    AwardGraze(p, center);
                    _pending.RemoveAt(i);
                }
            }
        }

        private void AwardGraze(Projectile p, Vector2 center)
        {
            p.Grazed = true;
            Grazes++;
            _overdrive.OnGraze();
            GameSignals.RaiseGraze(center, _risk.Multiplier);
        }

        private void OnEnable() => GameSignals.BossPartDestroyed += OnBossPartDestroyed;
        private void OnDisable() => GameSignals.BossPartDestroyed -= OnBossPartDestroyed;
        private void OnBossPartDestroyed(Vector2 position, int points) { if (_enabledForRun) _overdrive.OnBossPartDestroyed(); }

        // ---- Hooks from the ship -------------------------------------------------------------------

        public void OnPlayerDamaged() => _overdrive.OnDamaged();

        public void OnPlayerDied()
        {
            _overdrive.OnDeath();
            _risk.Reset();
            _pending.Clear();
        }

        /// <summary>Kill feedback: close kills and combo kills charge Overdrive.</summary>
        public void OnEnemyKilled(Vector2 position, bool isBoss)
        {
            if (!_enabledForRun) return;
            float d = Vector2.Distance(position, transform.position);
            if (d <= CloseKillDistance) _overdrive.OnCloseKill();
            else _overdrive.OnComboKill();
            if (isBoss) _overdrive.OnBossPartDestroyed();
        }

        // ---- Signals ----------------------------------------------------------------------------------

        private void OnRiskStateChanged(RiskState from, RiskState to)
        {
            GameSignals.RaiseRiskStateChanged(from, to);
            AudioManager.PlaySfx(to > from ? SfxId.RiskUp : SfxId.RiskDown, 0.6f);
        }

        private void OnOverdriveActivated()
        {
            GameSignals.RaiseOverdriveChanged(true);
            AudioManager.PlaySfx(SfxId.OverdriveStart);
        }

        private void OnOverdriveEnded()
        {
            GameSignals.RaiseOverdriveChanged(false);
            AudioManager.PlaySfx(SfxId.OverdriveEnd, 0.8f);
        }

        private void OnOverdriveChanged() => GameSignals.RaiseOverdriveMeter(_overdrive.Fraction, _overdrive.IsActive);
    }
}
