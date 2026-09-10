using System.Collections.Generic;
using Starfall.Core;
using Starfall.Logic;
using Starfall.Player;
using Starfall.Pooling;
using Starfall.VFX;
using UnityEngine;

namespace Starfall.Combat
{
    /// <summary>
    /// Generic pooled projectile used by the player, enemies and bosses. Visuals and behaviour come from
    /// <see cref="ProjectileSpec"/> so a single prefab covers every bullet type.
    /// </summary>
    [RequireComponent(typeof(PooledObject))]
    public sealed class Projectile : MonoBehaviour, IPoolable
    {
        private static readonly List<Projectile> ActiveEnemyProjectiles = new List<Projectile>(128);
        private static readonly Collider2D[] SplashBuffer = new Collider2D[24];

        [SerializeField] internal SpriteRenderer body;
        [SerializeField] internal SpriteRenderer glow;
        [SerializeField] internal Collider2D hitCollider;

        private PooledObject _pooled;
        private ProjectileSpec _spec;
        private Vector2 _direction;
        private float _age;
        private int _pierceLeft;
        private PlayArea _area;
        private VfxSpawner _vfx;
        private bool _launched;
        private bool _hitAnything;
        private Sprite _defaultSprite;

        public Faction Faction => _spec.Faction;
        public ProjectileSpec Spec => _spec;
        public bool IsLaunched => _launched;
        /// <summary>Set once by the RiskSensor so a projectile never grazes twice (plan §5.3).</summary>
        public bool Grazed { get; set; }
        /// <summary>True after this projectile damaged (or was blocked by) the player; cancels a pending graze.</summary>
        public bool HitPlayer { get; private set; }

        private void Awake()
        {
            _pooled = GetComponent<PooledObject>();
            if (body == null) body = GetComponentInChildren<SpriteRenderer>();
            if (hitCollider == null) hitCollider = GetComponent<Collider2D>();
            if (body != null) _defaultSprite = body.sprite;
        }

        public void Launch(in ProjectileSpec spec, Vector2 position, Vector2 direction)
        {
            var ctx = GameplayContext.Current;
            _area = ctx != null ? ctx.PlayArea : null;
            _vfx = ctx != null ? ctx.Vfx : null;

            _spec = spec;
            _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;
            _age = 0f;
            _pierceLeft = Mathf.Max(0, spec.Pierce);
            _launched = true;
            Grazed = false;
            HitPlayer = false;
            _hitAnything = false;

            transform.position = position;
            transform.rotation = Quaternion.FromToRotation(Vector3.up, _direction);
            transform.localScale = Vector3.one * Mathf.Max(0.05f, spec.Scale);
            gameObject.layer = GameLayers.ProjectileLayerFor(spec.Faction);

            if (body != null)
            {
                body.sprite = spec.Sprite != null ? spec.Sprite : _defaultSprite;
                body.color = spec.Color;
                body.sortingOrder = SortingOrders.Projectile;
            }
            if (glow != null)
            {
                glow.enabled = true;
                var c = spec.Color;
                c.a = 0.55f;
                glow.color = c;
                glow.sortingOrder = SortingOrders.Projectile - 1;
            }

            if (spec.Faction == Faction.Enemy) ActiveEnemyProjectiles.Add(this);
        }

        private void Update()
        {
            if (!_launched) return;
            float dt = Time.deltaTime;
            _age += dt;

            if (_spec.Homing)
            {
                if ((_spec.HomingTarget == null || !_spec.HomingTarget.gameObject.activeInHierarchy) && _spec.Faction == Faction.Player)
                    RetargetPlayerMissile();
                if (_spec.HomingTarget != null && _spec.HomingTarget.gameObject.activeInHierarchy)
                {
                    Vector2 toTarget = ((Vector2)_spec.HomingTarget.position - (Vector2)transform.position).normalized;
                    float maxRad = _spec.TurnRateDegrees * Mathf.Deg2Rad * dt;
                    _direction = Vector3.RotateTowards(_direction, toTarget, maxRad, 0f);
                    transform.rotation = Quaternion.FromToRotation(Vector3.up, _direction);
                }
            }

            bool moving = _spec.StopAfterSeconds <= 0f || _age < _spec.StopAfterSeconds;
            if (moving) transform.position += (Vector3)(_direction * (_spec.Speed * dt));
            else if (body != null) body.transform.localScale = Vector3.one * (1f + 0.15f * Mathf.Sin(_age * 18f));

            if (_age >= _spec.Lifetime)
            {
                if (_spec.DetonateOnExpire && _spec.SplashRadius > 0f) ApplySplash(transform.position, null);
                Despawn();
                return;
            }
            if (_area != null && _area.IsOutside(transform.position, _area.DespawnMargin))
                Despawn();
        }

        private void RetargetPlayerMissile()
        {
            var ctx = GameplayContext.Current;
            if (ctx == null || ctx.Enemies == null) return;
            var enemy = ctx.Enemies.Nearest(transform.position, true);
            _spec.HomingTarget = enemy != null ? enemy.transform : null;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_launched) return;
            if (!other.TryGetComponent<IDamageable>(out var target)) return;
            if (target.Faction == _spec.Faction || !target.IsAlive) return;

            var info = new DamageInfo(_spec.Damage, _spec.Source, _spec.Type, critical: _spec.Critical);
            Vector2 hit = other.ClosestPoint(transform.position);
            var result = target.ApplyDamage(info, hit);
            if (target.Faction == Faction.Player) HitPlayer = true;
            if (result.Applied) _hitAnything = true;
            if (_spec.MarkSeconds > 0f && result.Applied && other.TryGetComponent<Enemies.Enemy>(out var markedEnemy))
                markedEnemy.ApplyMark(_spec.MarkSeconds);

            if (_spec.SlowSeconds > 0f && result.Applied && other.TryGetComponent<PlayerShip>(out var ship))
                ship.Effects.ApplyDebuff(PowerUpKind.Slowed, _spec.SlowSeconds);

            if (_vfx != null)
            {
                if (result.Applied && result.ShieldDamage > 0f && result.HullDamage <= 0f)
                    _vfx.SpawnShieldHit(hit, _spec.Color);
                else
                    _vfx.SpawnImpact(hit, _spec.Color);
                if (_spec.Critical && result.Applied)
                    _vfx.SpawnFloatingText(hit + Vector2.up * 0.3f, Loc.T("CRIT"), new Color(1f, 0.9f, 0.3f), 0.6f);
            }

            if (_spec.SplashRadius > 0f) ApplySplash(hit, target);

            if (_pierceLeft > 0 && result.Applied && !result.Killed)
            {
                _pierceLeft--;
                return;
            }
            Despawn();
        }

        private void ApplySplash(Vector2 center, IDamageable primary)
        {
            int layerMask = _spec.Faction == Faction.Player
                ? (1 << GameLayers.Enemy) | (1 << GameLayers.Obstacle)
                : 1 << GameLayers.Player;
            int n = Physics2D.OverlapCircleNonAlloc(center, _spec.SplashRadius, SplashBuffer, layerMask);
            var info = new DamageInfo(_spec.Damage * 0.5f, _spec.Source, DamageType.Explosive);
            for (int i = 0; i < n; i++)
            {
                if (!SplashBuffer[i].TryGetComponent<IDamageable>(out var d) || (primary != null && ReferenceEquals(d, primary)) || d.Faction == _spec.Faction) continue;
                d.ApplyDamage(info, SplashBuffer[i].ClosestPoint(center));
            }
            if (_vfx != null) _vfx.SpawnExplosion(center, _spec.SplashRadius * 0.9f, _spec.Color);
        }

        public void Despawn()
        {
            if (!_launched) return;
            _launched = false;
            // Federation precision passive: the weapon learns whether this shot connected.
            if (_spec.Faction == Faction.Player && _spec.Source == DamageSource.Player)
            {
                var ctx = GameplayContext.Current;
                if (ctx != null && ctx.Player != null && ctx.Player.Weapon != null) ctx.Player.Weapon.ReportShot(_hitAnything);
            }
            _pooled.Release();
        }

        void IPoolable.OnSpawned() { }

        void IPoolable.OnDespawned()
        {
            _launched = false;
            if (body != null) body.transform.localScale = Vector3.one;
            if (_spec.Faction == Faction.Enemy) ActiveEnemyProjectiles.Remove(this);
            _spec.HomingTarget = null;
            if (body != null) body.sprite = _defaultSprite;
        }

        /// <summary>Despawns every active enemy projectile (Ultimate, boss death, player death).</summary>
        public static void ClearEnemyProjectiles()
        {
            for (int i = ActiveEnemyProjectiles.Count - 1; i >= 0; i--)
            {
                var p = ActiveEnemyProjectiles[i];
                if (p != null) p.Despawn();
                else ActiveEnemyProjectiles.RemoveAt(i);
            }
            ActiveEnemyProjectiles.Clear();
        }

        public static int ActiveEnemyProjectileCount => ActiveEnemyProjectiles.Count;
    }
}
