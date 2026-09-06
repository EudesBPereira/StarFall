using System.Collections;
using Starfall.Audio;
using Starfall.Combat;
using Starfall.Core;
using Starfall.Logic;
using Starfall.PowerUps;
using Starfall.Save;
using Starfall.VFX;
using UnityEngine;

namespace Starfall.Player
{
    /// <summary>
    /// Composition root of the playable ship: health, movement, weapon, status effects and Ultimate.
    /// Stats come from a <see cref="PlayerLoadout"/> (ship x upgrades). The <see cref="Core.GameFlowController"/>
    /// drives spawn/respawn; this class only reacts to damage.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public sealed class PlayerShip : MonoBehaviour
    {
        private const float CriticalHullFraction = 0.3f;

        [SerializeField] internal ShipDefinition definition;
        [SerializeField] internal SpriteRenderer body;
        [SerializeField] internal SpriteRenderer shieldVisual;
        [SerializeField] internal Collider2D hitbox;
        [SerializeField] internal PlayerMovement movement;
        [SerializeField] internal WeaponController weapon;
        [SerializeField] internal PlayerStatusEffects effects;
        [SerializeField] internal UltimateController ultimate;
        [SerializeField] internal ThrusterFlicker thruster;
        [SerializeField] internal ParticleSystem smoke;
        [SerializeField] internal EngineAudio engineAudio;
        [SerializeField] internal RiskSensor riskSensor;
        [SerializeField] internal CompanionDrone companionDrone;

        private Health _health;
        private GameplayContext _ctx;
        private PlayerLoadout _loadout;
        private Coroutine _invulnerabilityRoutine;
        private Coroutine _flashRoutine;
        private bool _timedInvulnerable;
        private bool _controlEnabled;
        private bool _critical;
        private float _regenTimer;
        private Color _baseColor = Color.white;
        private float _dragSensitivity = 1.4f;

        public Health Health => _health;
        public ShipDefinition Definition => definition;
        public PlayerLoadout Loadout => _loadout;
        public WeaponController Weapon => weapon;
        public PlayerStatusEffects Effects => effects;
        public UltimateController Ultimate => ultimate;
        /// <summary>Risk Zone + Overdrive owner (plan §5). Null-safe for tests that spawn a bare ship.</summary>
        public RiskSensor Risk => riskSensor;
        /// <summary>Temporary in-mission build (plan §2). Reset on every stage start.</summary>
        public BuildState Build { get; } = new BuildState();
        public bool IsAlive => _health != null && _health.IsAlive;
        public bool ControlEnabled => _controlEnabled;
        public bool IsCritical => _critical;

        private void OnEnable()
        {
            GameSignals.EnemyDestroyed += OnEnemyDestroyedLifesteal;
        }

        private void OnDisable()
        {
            GameSignals.EnemyDestroyed -= OnEnemyDestroyedLifesteal;
        }

        /// <summary>Biomech passive (plan §6): kills restore a small, capped fraction of hull.</summary>
        private void OnEnemyDestroyedLifesteal(EnemyKilledInfo info)
        {
            float lifesteal = (definition != null ? definition.LifestealPerKill : 0f) + Build.Modifiers.LifestealPerKill;
            if (!IsAlive || lifesteal <= 0f || !info.CountsForScore) return;
            if (info.Definition == null || info.Definition.IsObstacle) return;
            _health.RestoreHull(FactionRules.LifestealAmount(_health.MaxHull, lifesteal));
        }

        private void Awake()
        {
            _health = GetComponent<Health>();
            _health.DamagedAt += OnDamaged;
            _health.Blocked += OnBlocked;
            _health.Died += OnDied;
            _health.Changed += OnHealthChanged;
            if (effects != null) effects.Changed += OnEffectsChanged;
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.DamagedAt -= OnDamaged;
                _health.Blocked -= OnBlocked;
                _health.Died -= OnDied;
                _health.Changed -= OnHealthChanged;
            }
            if (effects != null) effects.Changed -= OnEffectsChanged;
        }

        /// <summary>Called once by the flow controller after the context is ready.</summary>
        public void Initialize(GameplayContext ctx)
        {
            _ctx = ctx;
            var config = ctx.Config;

            _loadout = PlayerLoadout.FromSave(SaveService.Data, config != null ? config.Ships : null, config != null ? config.Weapons : null, definition);
            if (_loadout.Ship != null) definition = _loadout.Ship;

            _health.Configure(Faction.Player, _loadout.MaxHull, _loadout.MaxShield);
            if (definition != null)
            {
                if (body != null)
                {
                    if (definition.Sprite != null) body.sprite = definition.Sprite;
                    body.color = definition.Tint;
                    body.transform.localScale = Vector3.one * definition.VisualScale;
                    _baseColor = definition.Tint;
                }
                if (hitbox is CircleCollider2D circle) circle.radius = definition.HitboxRadius;
                if (thruster != null) thruster.SetColor(definition.ThrusterColor);
            }

            movement.Configure(_loadout, ctx.PlayArea, config != null ? config.PlayerEdgePadding : 0.45f);
            weapon.Configure(_loadout.Weapon, _loadout, ctx.Pools, ctx.Vfx, ctx.Enemies);
            ultimate.Initialize(config, _loadout);
            if (engineAudio != null) engineAudio.Configure(movement);

            var save = SaveService.Data;
            if (riskSensor != null) riskSensor.Configure(_loadout, save != null && save.showHitbox);
            if (companionDrone != null)
            {
                bool hasDrone = definition != null && definition.HasCompanionDrone;
                companionDrone.Configure(this, definition != null ? definition.DroneDamage : 4f, definition != null ? definition.DroneInterval : 0.5f, definition != null ? definition.ThrusterColor : Color.cyan);
                companionDrone.SetActive(hasDrone);
            }
            _dragSensitivity = save != null ? save.touchSensitivity : 1.4f;
            if (ctx.Input != null) ctx.Input.AutoFire = save == null || save.autoFire;

            gameObject.layer = GameLayers.Player;
            Build.Reset();
            ApplyBuild();
            SetVisible(false);
            _controlEnabled = false;
        }

        /// <summary>Pushes the current build modifiers into every subsystem. Called after each draft pick.</summary>
        public void ApplyBuild()
        {
            var m = Build.Modifiers;
            if (weapon != null) weapon.SetBuild(m);
            if (ultimate != null) ultimate.BuildChargeMultiplier = m.UltimateCharge;
            if (riskSensor != null) riskSensor.BuildGainMultiplier = m.OverdriveGain;
            if (_health != null && m.ShieldBonus > _appliedShieldBonus)
            {
                _health.Model.AddMaxShield(m.ShieldBonus - _appliedShieldBonus);
                _appliedShieldBonus = m.ShieldBonus;
                PulseShield();
            }
        }

        private float _appliedShieldBonus;
        public bool MagnetActive => Build.Modifiers.Magnet;

        /// <summary>Places the ship, restores health and grants spawn invulnerability.</summary>
        public void SpawnAt(Vector2 position, float invulnerabilitySeconds)
        {
            transform.position = new Vector3(position.x, position.y, 0f);
            _health.ResetToFull();
            effects.ClearTemporary();
            movement.ResetMotion();
            weapon.ResetCooldown();
            _regenTimer = 0f;
            if (_ctx != null && _ctx.Input != null) _ctx.Input.ResetTransient();
            SetVisible(true);
            if (hitbox != null) hitbox.enabled = true;
            StartTimedInvulnerability(invulnerabilitySeconds);
            if (riskSensor != null) riskSensor.BeginRun();
            OnHealthChanged();
        }

        public void SetControlEnabled(bool enabled)
        {
            _controlEnabled = enabled;
            if (!enabled && riskSensor != null) riskSensor.EndRun();
            if (!enabled && movement != null) movement.ResetMotion();
            if (engineAudio != null) engineAudio.SetActive(enabled && IsAlive);
        }

        private void Update()
        {
            if (!IsAlive || !_controlEnabled || _ctx == null || _ctx.Input == null) return;
            var input = _ctx.Input;

            ApplyOverdriveEffects();
            movement.Tick(input, effects.SpeedMultiplier * Build.Modifiers.Speed, _dragSensitivity);
            weapon.Tick(input.FireHeld, effects.DamageMultiplier);
            if (input.UltimatePressed) ultimate.TryActivate();
            TickShieldRegen(Time.deltaTime);
        }

        /// <summary>Faction reaction to Overdrive (plan §6): Federation = fire rate, Biomech = shield regen, Cyber = crit.</summary>
        private void ApplyOverdriveEffects()
        {
            bool active = riskSensor != null && riskSensor.Overdrive.IsActive;
            var def = definition;
            float fireRate = 1f, crit = 0f;
            if (active && def != null)
            {
                switch (def.Faction)
                {
                    case FactionId.Cyber: crit = def.OverdriveCritBonus > 0f ? def.OverdriveCritBonus : 0.15f; fireRate = 1.05f; break;
                    case FactionId.Biomech: fireRate = 1.05f; if (def.OverdriveShieldRegen > 0f) _health.RestoreShield(def.OverdriveShieldRegen * Time.deltaTime); break;
                    default: fireRate = def.OverdriveFireRateBonus; break;
                }
            }
            weapon.ExternalFireRateMultiplier = fireRate;
            weapon.ExternalCritBonus = crit;
            ultimate.ExternalChargeMultiplier = riskSensor != null ? riskSensor.Overdrive.UltimateChargeMultiplier : 1f;
            if (thruster != null && def != null) thruster.SetColor(active ? Color.Lerp(def.ThrusterColor, new Color(1f, 0.24f, 0.67f, 0.95f), 0.5f + 0.5f * Mathf.Sin(Time.time * 12f)) : def.ThrusterColor);
        }

        private void TickShieldRegen(float dt)
        {
            if (_loadout.ShieldRegenPerSecond <= 0f || _health.Shield >= _health.MaxShield) return;
            _regenTimer += dt;
            if (_regenTimer < _loadout.ShieldRegenDelay) return;
            _health.RestoreShield(_loadout.ShieldRegenPerSecond * dt);
        }

        // ---- Power-ups ----------------------------------------------------------------------

        public void CollectPowerUp(PowerUpDefinition def)
        {
            if (def == null || !IsAlive) return;
            switch (def.Kind)
            {
                case PowerUpKind.LaserLevel:
                    weapon.Upgrade();
                    break;
                case PowerUpKind.ShieldRestore:
                    _health.RestoreShield(def.Magnitude);
                    PulseShield();
                    break;
                case PowerUpKind.Energy:
                    ultimate.AddEnergy(def.Magnitude);
                    break;
                default:
                    effects.ApplyTimed(def);
                    break;
            }
            GameSignals.RaisePowerUpCollected(def);
            AudioManager.PlaySfx(SfxId.PowerUp);
            if (_ctx != null && _ctx.Vfx != null)
            {
                _ctx.Vfx.SpawnPickupBurst(transform.position, def.Color);
                _ctx.Vfx.SpawnFloatingText(transform.position + Vector3.up * 0.8f, def.Label, def.Color);
            }
        }

        // ---- Damage ---------------------------------------------------------------------------

        private void OnDamaged(DamageInfo info, DamageResult result, Vector2 hitPoint)
        {
            _regenTimer = 0f;
            if (riskSensor != null) riskSensor.OnPlayerDamaged();
            GameSignals.RaisePlayerDamaged(info, result);
            AudioManager.PlaySfx(result.ShieldDamage > 0f && result.HullDamage <= 0f ? SfxId.ShieldHit : SfxId.PlayerHit);

            if (_ctx != null)
            {
                if (_ctx.CameraShake != null) _ctx.CameraShake.Shake(result.Killed ? 0.5f : 0.18f, result.Killed ? 0.6f : 0.2f);
                if (_ctx.Vfx != null)
                {
                    if (result.ShieldDamage > 0f) _ctx.Vfx.SpawnShieldHit(hitPoint, new Color(0.4f, 0.8f, 1f));
                    if (result.HullDamage > 0f) _ctx.Vfx.SpawnSparks(hitPoint, new Color(1f, 0.6f, 0.2f));
                }
            }

            if (result.Killed) return;
            FlashBody(new Color(1f, 0.35f, 0.35f));
            PulseShield();
            float hitInvuln = _ctx != null && _ctx.Config != null ? _ctx.Config.HitInvulnerability : 1f;
            StartTimedInvulnerability(hitInvuln);
        }

        private void OnBlocked(Vector2 hitPoint)
        {
            if (_ctx != null && _ctx.Vfx != null) _ctx.Vfx.SpawnShieldHit(hitPoint, new Color(0.9f, 0.9f, 1f));
        }

        private void OnDied(DamageInfo info)
        {
            _controlEnabled = false;
            if (riskSensor != null) riskSensor.OnPlayerDied();
            if (weapon != null) { weapon.ExternalFireRateMultiplier = 1f; weapon.ExternalCritBonus = 0f; }
            if (hitbox != null) hitbox.enabled = false;
            StopInvulnerabilityRoutine();
            effects.ClearTemporary();
            SetCritical(false);
            SetVisible(false);
            if (engineAudio != null) engineAudio.SetActive(false);
            if (_ctx != null && _ctx.Vfx != null) _ctx.Vfx.SpawnExplosion(transform.position, 1.6f, new Color(1f, 0.6f, 0.3f));
            AudioManager.PlaySfx(SfxId.ExplosionLarge);
            GameSignals.RaisePlayerDied();
        }

        private void OnHealthChanged()
        {
            if (shieldVisual != null)
            {
                float f = _health.MaxShield > 0f ? _health.Shield / _health.MaxShield : 0f;
                var c = shieldVisual.color;
                c.a = Mathf.Lerp(0f, 0.55f, f);
                shieldVisual.color = c;
                shieldVisual.enabled = f > 0f && body != null && body.enabled;
            }
            bool critical = IsAlive && _health.MaxHull > 0f && _health.Hull / _health.MaxHull <= CriticalHullFraction;
            SetCritical(critical);
        }

        private void SetCritical(bool critical)
        {
            if (_critical == critical) return;
            _critical = critical;
            if (smoke != null)
            {
                if (critical) smoke.Play(true);
                else smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            if (critical) AudioManager.PlaySfx(SfxId.Alarm);
            GameSignals.RaisePlayerCriticalChanged(critical);
        }

        private void OnEffectsChanged()
        {
            RefreshInvulnerability();
            if (body != null && effects.Invincible)
                body.color = Color.Lerp(_baseColor, Color.white, 0.6f);
            else if (body != null && _flashRoutine == null)
                body.color = effects.Slowed ? Color.Lerp(_baseColor, new Color(0.6f, 0.3f, 0.9f), 0.5f) : _baseColor;
        }

        // ---- Invulnerability & visuals ----------------------------------------------------------

        private void StartTimedInvulnerability(float seconds)
        {
            StopInvulnerabilityRoutine();
            if (seconds <= 0f) { _timedInvulnerable = false; RefreshInvulnerability(); return; }
            _invulnerabilityRoutine = StartCoroutine(InvulnerabilityRoutine(seconds));
        }

        private void StopInvulnerabilityRoutine()
        {
            if (_invulnerabilityRoutine != null) StopCoroutine(_invulnerabilityRoutine);
            _invulnerabilityRoutine = null;
            _timedInvulnerable = false;
            RefreshInvulnerability();
        }

        private IEnumerator InvulnerabilityRoutine(float seconds)
        {
            _timedInvulnerable = true;
            RefreshInvulnerability();
            float end = Time.time + seconds;
            bool visible = true;
            while (Time.time < end)
            {
                visible = !visible;
                if (body != null) body.enabled = visible;
                yield return new WaitForSeconds(0.08f);
            }
            if (body != null && IsAlive) body.enabled = true;
            _timedInvulnerable = false;
            _invulnerabilityRoutine = null;
            RefreshInvulnerability();
        }

        private void RefreshInvulnerability()
        {
            if (_health == null) return;
            _health.Invulnerable = _timedInvulnerable || (effects != null && effects.Invincible);
        }

        private void FlashBody(Color color)
        {
            if (body == null) return;
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine(color));
        }

        private IEnumerator FlashRoutine(Color color)
        {
            body.color = color;
            yield return new WaitForSeconds(0.08f);
            body.color = effects != null && effects.Invincible ? Color.Lerp(_baseColor, Color.white, 0.6f) : _baseColor;
            _flashRoutine = null;
        }

        private void PulseShield()
        {
            if (shieldVisual == null || !gameObject.activeInHierarchy) return;
            shieldVisual.transform.localScale = Vector3.one * 1.25f;
            StartCoroutine(ShieldPulseRoutine());
        }

        private IEnumerator ShieldPulseRoutine()
        {
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                shieldVisual.transform.localScale = Vector3.one * Mathf.Lerp(1.25f, 1f, t / 0.2f);
                yield return null;
            }
            shieldVisual.transform.localScale = Vector3.one;
        }

        private void SetVisible(bool visible)
        {
            if (body != null) body.enabled = visible;
            if (thruster != null) thruster.SetActive(visible);
            if (shieldVisual != null) shieldVisual.enabled = visible && _health != null && _health.Shield > 0f;
            if (!visible && smoke != null) smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
