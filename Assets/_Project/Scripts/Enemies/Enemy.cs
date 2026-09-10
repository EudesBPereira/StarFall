using System.Collections;
using Starfall.Audio;
using Starfall.Combat;
using Starfall.Core;
using Starfall.Logic;
using Starfall.Player;
using Starfall.Pooling;
using Starfall.PowerUps;
using Starfall.VFX;
using UnityEngine;

namespace Starfall.Enemies
{
    /// <summary>
    /// Generic pooled enemy. Everything specific (stats, look, movement, attack) comes from
    /// <see cref="EnemyDefinition"/>; bosses extend this class only for phase logic.
    /// </summary>
    [RequireComponent(typeof(PooledObject), typeof(Health))]
    public class Enemy : MonoBehaviour, IPoolable
    {
        [SerializeField] internal SpriteRenderer body;
        [SerializeField] internal SpriteRenderer shieldVisual;
        [SerializeField] internal CircleCollider2D hitbox;
        [SerializeField] internal ThrusterFlicker thruster;

        private PooledObject _pooled;
        private Health _health;
        private EnemyDefinition _definition;
        private GameplayContext _ctx;
        private EnemySpawner _spawner;
        private IMovementStrategy _movement;
        private IAttackStrategy _attack;
        private MovementKind _movementKind;
        private AttackKind _attackKind;
        private bool _initialized;
        private bool _dying;
        private float _age;
        private float _contactCooldown;
        private float _statMultiplier = 1f;
        private Coroutine _flashRoutine;
        private float _markTimer;
        private float _stunTimer;

        public EnemyDefinition Definition => _definition;
        public Health Health => _health;
        public PlayArea Area => _ctx != null ? _ctx.PlayArea : null;
        public Transform Target => _ctx != null && _ctx.Player != null && _ctx.Player.IsAlive ? _ctx.Player.transform : null;
        public float Age => _age;
        public bool IsActiveInstance => _initialized && !_dying && _pooled != null && _pooled.IsSpawned;
        public bool IsCommon => _definition != null && !_definition.IsElite && !_definition.IsBoss;
        public bool IsBoss => _definition != null && _definition.IsBoss;
        public virtual MovementParams Movement => _definition != null ? _definition.MovementSettings : MovementParams.Default;
        public AttackParams AttackSettings => _definition != null ? _definition.AttackSettings : AttackParams.Default;
        public bool IsOnScreen => Area == null || Area.IsInside(transform.position, -0.2f);
        public Vector2 MuzzlePosition => (Vector2)transform.position + Vector2.down * (_definition != null ? _definition.ColliderRadius * _definition.Scale : 0.3f);
        /// <summary>Speed scale from the difficulty of endless modes / daily modifiers.</summary>
        public float SpeedMultiplier { get; private set; } = 1f;
        public bool IsMarked => _markTimer > 0f;
        public bool IsStunned => _stunTimer > 0f;

        protected GameplayContext Context => _ctx;
        protected bool Dying => _dying;

        protected virtual void Awake()
        {
            _pooled = GetComponent<PooledObject>();
            _health = GetComponent<Health>();
            _health.DamagedAt += OnDamaged;
            _health.Died += OnDied;
        }

        protected virtual void OnDestroy()
        {
            if (_health == null) return;
            _health.DamagedAt -= OnDamaged;
            _health.Died -= OnDied;
        }

        /// <summary>Called by <see cref="EnemySpawner"/> right after the pooled instance is activated.</summary>
        public virtual void Initialize(EnemyDefinition definition, GameplayContext ctx, EnemySpawner spawner, float statMultiplier = 1f, float speedMultiplier = 1f)
        {
            _definition = definition;
            _ctx = ctx;
            _spawner = spawner;
            _age = 0f;
            _contactCooldown = 0f;
            _dying = false;
            _initialized = true;
            _statMultiplier = Mathf.Max(0.1f, statMultiplier);
            SpeedMultiplier = Mathf.Max(0.1f, speedMultiplier);
            _markTimer = 0f;
            _stunTimer = 0f;

            gameObject.layer = definition.IsObstacle ? GameLayers.Obstacle : GameLayers.Enemy;
            transform.localScale = Vector3.one * definition.Scale;
            transform.rotation = Quaternion.identity;

            if (body != null)
            {
                if (definition.Sprite != null) body.sprite = definition.Sprite;
                body.color = definition.Tint;
                body.enabled = true;
                body.sortingOrder = SortingOrders.Enemy;
                body.transform.localRotation = Quaternion.identity;
            }
            if (hitbox != null)
            {
                hitbox.radius = definition.ColliderRadius;
                hitbox.enabled = true;
            }
            if (thruster != null)
            {
                thruster.SetColor(definition.ThrusterColor);
                thruster.SetActive(definition.HasThruster && !definition.IsObstacle);
                thruster.transform.localPosition = new Vector3(0f, definition.ColliderRadius * 0.9f, 0f);
            }

            _health.Configure(Faction.Enemy, definition.MaxHull * _statMultiplier, definition.MaxShield * _statMultiplier);
            RefreshShieldVisual();

            if (_movement == null || _movementKind != definition.Movement)
            {
                _movementKind = definition.Movement;
                _movement = MovementStrategies.Create(definition.Movement);
            }
            if (_attack == null || _attackKind != definition.Attack)
            {
                _attackKind = definition.Attack;
                _attack = AttackStrategies.Create(definition.Attack);
            }
            _movement.Begin(this);
            _attack?.Begin(this);

            if (ctx != null && ctx.Enemies != null) ctx.Enemies.Register(this);
        }

        protected virtual void Update()
        {
            if (!IsActiveInstance) return;
            float dt = Time.deltaTime;
            _age += dt;
            if (_contactCooldown > 0f) _contactCooldown -= dt;
            TickStatus(dt);

            if (!IsStunned)
            {
                _movement.Tick(this, dt);
                _attack?.Tick(this, dt);
            }

            var area = Area;
            if (_age > _definition.MinLifetime && area != null && area.IsOutside(transform.position, area.DespawnMargin))
                Despawn();
        }

        // ---- Combat -------------------------------------------------------------------------------

        private void OnTriggerEnter2D(Collider2D other) => HandleContact(other);
        private void OnTriggerStay2D(Collider2D other) => HandleContact(other);

        private void HandleContact(Collider2D other)
        {
            if (!IsActiveInstance || _contactCooldown > 0f || _definition.ContactDamage <= 0f) return;
            if (!other.TryGetComponent<PlayerShip>(out var ship) || !ship.IsAlive) return;

            _contactCooldown = 0.5f;
            var info = new DamageInfo(_definition.ContactDamage * _statMultiplier, DamageSource.Enemy, DamageType.Contact);
            ship.Health.ApplyDamage(info, other.ClosestPoint(transform.position));

            if (_definition.SelfDestructOnContact)
                _health.Kill(DamageSource.Environment);
        }

        private void OnDamaged(DamageInfo info, DamageResult result, Vector2 hitPoint)
        {
            if (result.Killed) return;
            OnDamageFeedback(info, result, hitPoint);
            if (result.ShieldDamage > 0f) PulseShield();
            RefreshShieldVisual();
            if (result.ShieldBroken && _ctx != null && _ctx.Vfx != null)
                _ctx.Vfx.SpawnShieldBreak(transform.position, _definition.ShieldColor);
        }

        /// <summary>Visual reaction to a non-lethal hit. Bosses override it with heavier feedback.</summary>
        protected virtual void OnDamageFeedback(in DamageInfo info, in DamageResult result, Vector2 hitPoint)
        {
            Flash(result.ShieldDamage > 0f && result.HullDamage <= 0f ? _definition.ShieldColor : Color.white);
        }

        private void OnDied(DamageInfo info)
        {
            if (!_initialized || _dying) return;
            _dying = true;
            HandleDeath(info.Source);
        }

        protected virtual void HandleDeath(DamageSource source)
        {
            if (_ctx != null && _ctx.Vfx != null)
            {
                _ctx.Vfx.SpawnExplosion(transform.position, _definition.ExplosionScale, _definition.ExplosionColor);
                if (!_definition.IsObstacle) _ctx.Vfx.SpawnSparks(transform.position, _definition.ExplosionColor);
                if (_definition.IsElite) _ctx.Vfx.SpawnFloatingText(transform.position + Vector3.up * 0.6f, Loc.T("ELITE"), new Color(1f, 0.85f, 0.3f), 0.8f);
            }

            GameSignals.RaiseEnemyDestroyed(new EnemyKilledInfo(_definition, transform.position, source, IsBoss));

            if ((source == DamageSource.Player || source == DamageSource.Ultimate) && _definition.DropTable != null && _spawner != null)
            {
                var drop = _definition.DropTable.Roll(_spawner.DropChanceMultiplier);
                if (drop != null) _spawner.SpawnPickup(drop, transform.position);
            }

            Release();
        }

        /// <summary>Removes the enemy without rewards (left the screen, stage reset).</summary>
        public void Despawn()
        {
            if (!_initialized) return;
            Release();
        }

        private void Release()
        {
            if (_ctx != null && _ctx.Enemies != null) _ctx.Enemies.Unregister(this);
            _initialized = false;
            if (_flashRoutine != null) { StopCoroutine(_flashRoutine); _flashRoutine = null; }
            _pooled.Release();
        }

        // ---- Helpers for strategies -----------------------------------------------------------------

        public Vector2 DirectionToTarget(Vector2 from)
        {
            var target = Target;
            if (target == null) return Vector2.down;
            Vector2 dir = (Vector2)target.position - from;
            return dir.sqrMagnitude > 0.001f ? dir.normalized : Vector2.down;
        }

        public void FaceDirection(Vector2 direction)
        {
            if (body == null || direction.sqrMagnitude < 0.001f) return;
            body.transform.rotation = Quaternion.FromToRotation(Vector3.down, direction);
        }

        private PooledObject ProjectilePrefab =>
            _definition != null && _definition.ProjectilePrefab != null ? _definition.ProjectilePrefab
            : _spawner != null ? _spawner.EnemyProjectilePrefab : null;

        protected ProjectileSpec BuildSpec(in AttackParams p)
        {
            var spec = ProjectileSpec.Enemy(p.Damage * _statMultiplier, p.ProjectileSpeed * SpeedMultiplier, p.Lifetime, p.Color, p.ProjectileScale, IsBoss);
            spec.SplashRadius = p.SplashRadius;
            spec.SlowSeconds = p.SlowSeconds;
            if (p.SplashRadius > 0f) spec.Type = DamageType.Explosive;
            return spec;
        }

        public void FireSpread(Vector2 origin, Vector2 direction, in AttackParams p, int count, float spreadAngle)
        {
            if (_ctx == null) return;
            var spec = BuildSpec(p);
            ProjectileLauncher.FireSpread(_ctx.Pools, ProjectilePrefab, spec, origin, direction, Mathf.Max(1, count), spreadAngle);
            AudioManager.PlaySfx(SfxId.EnemyShot, 0.5f);
        }

        public void FireRing(Vector2 origin, in AttackParams p, int count, float phase)
        {
            if (_ctx == null) return;
            var spec = BuildSpec(p);
            ProjectileLauncher.FireRing(_ctx.Pools, ProjectilePrefab, spec, origin, count, phase);
            AudioManager.PlaySfx(SfxId.EnemyShot, 0.6f);
        }

        /// <summary>Widow: slow, large web projectile that slows the player.</summary>
        public void FireWeb(Vector2 origin, Vector2 direction, in AttackParams p)
        {
            if (_ctx == null) return;
            var spec = BuildSpec(p);
            spec.Scale = Mathf.Max(spec.Scale, 2.2f);
            spec.SlowSeconds = p.SlowSeconds > 0f ? p.SlowSeconds : 3f;
            spec.Color = new Color(0.8f, 0.95f, 1f, 0.85f);
            spec.Sprite = _spawner != null ? _spawner.WebSprite : null;
            ProjectileLauncher.Fire(_ctx.Pools, ProjectilePrefab, spec, origin, direction);
            AudioManager.PlaySfx(SfxId.WebShot, 0.8f);
        }

        public Projectile FireProjectile(Vector2 origin, Vector2 direction, in ProjectileSpec spec)
        {
            if (_ctx == null) return null;
            return ProjectileLauncher.Fire(_ctx.Pools, ProjectilePrefab, spec, origin, direction);
        }

        /// <summary>Hive Queen: spawns minions next to this enemy.</summary>
        public void Summon(EnemyDefinition minion, int count)
        {
            if (_spawner == null || minion == null) return;
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(-0.5f, 0.5f));
                var e = _spawner.Spawn(minion, (Vector2)transform.position + offset);
                if (e != null && _ctx != null && _ctx.Vfx != null) _ctx.Vfx.SpawnPickupBurst(e.transform.position, minion.Tint);
            }
            AudioManager.PlaySfx(SfxId.Summon, 0.8f);
        }

        // ---- Status (Cyber mark, EMP stun) ------------------------------------------------------------------

        /// <summary>Marked targets take extra damage for a while (plan §6, Cyber).</summary>
        public void ApplyMark(float seconds)
        {
            if (!IsActiveInstance || seconds <= 0f) return;
            _markTimer = Mathf.Max(_markTimer, seconds);
            _health.IncomingDamageMultiplier = FactionRules.MarkDamageMultiplier;
        }

        /// <summary>EMP: movement and attacks pause; bosses are stunned for half the time.</summary>
        public void Stun(float seconds)
        {
            if (!IsActiveInstance || seconds <= 0f) return;
            _stunTimer = Mathf.Max(_stunTimer, IsBoss ? seconds * 0.5f : seconds);
        }

        protected void TickStatus(float dt)
        {
            if (_markTimer > 0f)
            {
                _markTimer -= dt;
                if (_markTimer <= 0f) _health.IncomingDamageMultiplier = 1f;
                else if (body != null && _flashRoutine == null) body.color = Color.Lerp(CurrentTint, new Color(0.14f, 0.84f, 1f), 0.35f + 0.25f * Mathf.Sin(Time.time * 14f));
            }
            if (_stunTimer > 0f)
            {
                _stunTimer -= dt;
                if (body != null && _flashRoutine == null) body.color = Color.Lerp(CurrentTint, Color.white, Mathf.PingPong(Time.time * 10f, 1f) * 0.6f);
                if (_stunTimer <= 0f && body != null && _flashRoutine == null) body.color = CurrentTint;
            }
            else if (_markTimer <= 0f && body != null && _flashRoutine == null && body.color != CurrentTint && !_dying) body.color = CurrentTint;
        }

        // ---- Visuals -----------------------------------------------------------------------------------

        /// <summary>Boss transformations: swap sprite/tint/scale in place.</summary>
        public void ApplyVisual(Sprite sprite, Color tint, float scale)
        {
            if (body != null)
            {
                if (sprite != null) body.sprite = sprite;
                body.color = tint;
            }
            if (scale > 0f) transform.localScale = Vector3.one * scale;
        }

        private void RefreshShieldVisual()
        {
            if (shieldVisual == null) return;
            bool show = _health.MaxShield > 0f && _health.Shield > 0f;
            shieldVisual.enabled = show;
            if (!show) return;
            var c = _definition.ShieldColor;
            c.a = Mathf.Lerp(0.25f, 0.75f, _health.Shield / _health.MaxShield);
            shieldVisual.color = c;
            shieldVisual.sortingOrder = SortingOrders.EnemyShield;
        }

        private void PulseShield()
        {
            if (shieldVisual == null) return;
            shieldVisual.transform.localScale = Vector3.one * 1.3f;
        }

        protected void Flash(Color color, float seconds = 0.06f)
        {
            if (body == null || !gameObject.activeInHierarchy) return;
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine(color, seconds));
        }

        private IEnumerator FlashRoutine(Color color, float seconds)
        {
            var original = CurrentTint;
            body.color = Color.Lerp(original, color, 0.85f);
            yield return new WaitForSeconds(seconds);
            body.color = CurrentTint;
            if (shieldVisual != null) shieldVisual.transform.localScale = Vector3.one;
            _flashRoutine = null;
        }

        /// <summary>Tint to restore after flashes (bosses override per phase).</summary>
        protected virtual Color CurrentTint => _definition != null ? _definition.Tint : Color.white;

        void IPoolable.OnSpawned() { }

        void IPoolable.OnDespawned()
        {
            _initialized = false;
            _dying = false;
            _definition = null;
            if (shieldVisual != null) { shieldVisual.enabled = false; shieldVisual.transform.localScale = Vector3.one; }
            if (thruster != null) thruster.SetActive(false);
        }
    }
}
