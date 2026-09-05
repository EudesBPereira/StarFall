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
        private Coroutine _flashRoutine;

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
        public virtual void Initialize(EnemyDefinition definition, GameplayContext ctx, EnemySpawner spawner)
        {
            _definition = definition;
            _ctx = ctx;
            _spawner = spawner;
            _age = 0f;
            _contactCooldown = 0f;
            _dying = false;
            _initialized = true;

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

            _health.Configure(Faction.Enemy, definition.MaxHull, definition.MaxShield);
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

            _movement.Tick(this, dt);
            _attack?.Tick(this, dt);

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
            var info = new DamageInfo(_definition.ContactDamage, DamageSource.Enemy, DamageType.Contact);
            ship.Health.ApplyDamage(info, other.ClosestPoint(transform.position));

            if (_definition.SelfDestructOnContact)
                _health.Kill(DamageSource.Environment);
        }

        private void OnDamaged(DamageInfo info, DamageResult result, Vector2 hitPoint)
        {
            if (result.Killed) return;
            Flash(result.ShieldDamage > 0f && result.HullDamage <= 0f ? _definition.ShieldColor : Color.white);
            if (result.ShieldDamage > 0f) PulseShield();
            RefreshShieldVisual();
            if (result.ShieldBroken && _ctx != null && _ctx.Vfx != null)
                _ctx.Vfx.SpawnShieldBreak(transform.position, _definition.ShieldColor);
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
                _ctx.Vfx.SpawnExplosion(transform.position, _definition.ExplosionScale, _definition.ExplosionColor);

            GameSignals.RaiseEnemyDestroyed(new EnemyKilledInfo(_definition, transform.position, source, IsBoss));

            if ((source == DamageSource.Player || source == DamageSource.Ultimate) && _definition.DropTable != null && _spawner != null)
            {
                var drop = _definition.DropTable.Roll();
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
            return ProjectileSpec.Enemy(p.Damage, p.ProjectileSpeed, p.Lifetime, p.Color, p.ProjectileScale, IsBoss);
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

        public Projectile FireProjectile(Vector2 origin, Vector2 direction, in ProjectileSpec spec)
        {
            if (_ctx == null) return null;
            return ProjectileLauncher.Fire(_ctx.Pools, ProjectilePrefab, spec, origin, direction);
        }

        // ---- Visuals -----------------------------------------------------------------------------------

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

        protected void Flash(Color color)
        {
            if (body == null || !gameObject.activeInHierarchy) return;
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine(color));
        }

        private IEnumerator FlashRoutine(Color color)
        {
            var original = _definition != null ? _definition.Tint : Color.white;
            body.color = Color.Lerp(original, color, 0.85f);
            yield return new WaitForSeconds(0.06f);
            body.color = original;
            if (shieldVisual != null) shieldVisual.transform.localScale = Vector3.one;
            _flashRoutine = null;
        }

        void IPoolable.OnSpawned() { }

        void IPoolable.OnDespawned()
        {
            _initialized = false;
            _dying = false;
            _definition = null;
            if (shieldVisual != null) { shieldVisual.enabled = false; shieldVisual.transform.localScale = Vector3.one; }
        }
    }
}
