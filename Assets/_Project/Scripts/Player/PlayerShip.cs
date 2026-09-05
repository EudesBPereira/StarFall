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
    /// The <see cref="Core.GameFlowController"/> drives spawn/respawn; this class only reacts to damage.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public sealed class PlayerShip : MonoBehaviour
    {
        [SerializeField] internal ShipDefinition definition;
        [SerializeField] internal SpriteRenderer body;
        [SerializeField] internal SpriteRenderer shieldVisual;
        [SerializeField] internal Collider2D hitbox;
        [SerializeField] internal PlayerMovement movement;
        [SerializeField] internal WeaponController weapon;
        [SerializeField] internal PlayerStatusEffects effects;
        [SerializeField] internal UltimateController ultimate;
        [SerializeField] internal ThrusterFlicker thruster;

        private Health _health;
        private GameplayContext _ctx;
        private Coroutine _invulnerabilityRoutine;
        private Coroutine _flashRoutine;
        private bool _timedInvulnerable;
        private bool _controlEnabled;
        private Color _baseColor = Color.white;
        private float _dragSensitivity = 1.4f;

        public Health Health => _health;
        public ShipDefinition Definition => definition;
        public WeaponController Weapon => weapon;
        public PlayerStatusEffects Effects => effects;
        public UltimateController Ultimate => ultimate;
        public bool IsAlive => _health != null && _health.IsAlive;
        public bool ControlEnabled => _controlEnabled;

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

            if (definition != null)
            {
                _health.Configure(Faction.Player, definition.MaxHull, definition.MaxShield);
                if (body != null)
                {
                    if (definition.Sprite != null) body.sprite = definition.Sprite;
                    body.color = definition.Tint;
                    _baseColor = definition.Tint;
                }
                if (hitbox is CircleCollider2D circle) circle.radius = definition.HitboxRadius;
            }

            movement.Configure(definition, ctx.PlayArea, config != null ? config.PlayerEdgePadding : 0.45f);
            weapon.Configure(definition != null ? definition.Weapon : null, ctx.Pools, ctx.Vfx);
            ultimate.Initialize(config);

            var save = SaveService.Data;
            _dragSensitivity = save != null ? save.touchSensitivity : 1.4f;
            if (ctx.Input != null) ctx.Input.AutoFire = save == null || save.autoFire;

            gameObject.layer = GameLayers.Player;
            SetVisible(false);
            _controlEnabled = false;
        }

        /// <summary>Places the ship, restores health and grants spawn invulnerability.</summary>
        public void SpawnAt(Vector2 position, float invulnerabilitySeconds)
        {
            transform.position = new Vector3(position.x, position.y, 0f);
            _health.ResetToFull();
            effects.ClearTemporary();
            movement.ResetMotion();
            weapon.ResetCooldown();
            if (_ctx != null && _ctx.Input != null) _ctx.Input.ResetTransient();
            SetVisible(true);
            if (hitbox != null) hitbox.enabled = true;
            StartTimedInvulnerability(invulnerabilitySeconds);
            OnHealthChanged();
        }

        public void SetControlEnabled(bool enabled)
        {
            _controlEnabled = enabled;
            if (!enabled && movement != null) movement.ResetMotion();
        }

        private void Update()
        {
            if (!IsAlive || !_controlEnabled || _ctx == null || _ctx.Input == null) return;
            var input = _ctx.Input;

            movement.Tick(input, effects.SpeedMultiplier, _dragSensitivity);
            weapon.Tick(input.FireHeld, effects.DamageMultiplier);
            if (input.UltimatePressed) ultimate.TryActivate();
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
            GameSignals.RaisePlayerDamaged(info, result);
            AudioManager.PlaySfx(result.ShieldDamage > 0f && result.HullDamage <= 0f ? SfxId.ShieldHit : SfxId.PlayerHit);

            if (_ctx != null)
            {
                if (_ctx.CameraShake != null) _ctx.CameraShake.Shake(result.Killed ? 0.5f : 0.18f, result.Killed ? 0.6f : 0.2f);
                if (_ctx.Vfx != null)
                {
                    if (result.ShieldDamage > 0f) _ctx.Vfx.SpawnShieldHit(hitPoint, new Color(0.4f, 0.8f, 1f));
                    if (result.HullDamage > 0f) _ctx.Vfx.SpawnImpact(hitPoint, new Color(1f, 0.5f, 0.2f));
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
            if (hitbox != null) hitbox.enabled = false;
            StopInvulnerabilityRoutine();
            effects.ClearTemporary();
            SetVisible(false);
            if (_ctx != null && _ctx.Vfx != null) _ctx.Vfx.SpawnExplosion(transform.position, 1.6f, new Color(1f, 0.6f, 0.3f));
            AudioManager.PlaySfx(SfxId.ExplosionLarge);
            GameSignals.RaisePlayerDied();
        }

        private void OnHealthChanged()
        {
            if (shieldVisual == null) return;
            float f = _health.MaxShield > 0f ? _health.Shield / _health.MaxShield : 0f;
            var c = shieldVisual.color;
            c.a = Mathf.Lerp(0f, 0.55f, f);
            shieldVisual.color = c;
            shieldVisual.enabled = f > 0f && body != null && body.enabled;
        }

        private void OnEffectsChanged()
        {
            RefreshInvulnerability();
            if (body != null && effects.Invincible)
                body.color = Color.Lerp(_baseColor, Color.white, 0.6f);
            else if (body != null && _flashRoutine == null)
                body.color = _baseColor;
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
            if (shieldVisual == null) return;
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
        }
    }
}
