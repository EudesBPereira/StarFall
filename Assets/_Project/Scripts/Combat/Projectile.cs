using System.Collections.Generic;
using Starfall.Core;
using Starfall.Logic;
using Starfall.Pooling;
using Starfall.VFX;
using UnityEngine;

namespace Starfall.Combat
{
    /// <summary>
    /// Generic pooled projectile used by the player, enemies and bosses. Visuals and behaviour come from
    /// <see cref="ProjectileSpec"/> so a single prefab covers every bullet type in the MVP.
    /// </summary>
    [RequireComponent(typeof(PooledObject))]
    public sealed class Projectile : MonoBehaviour, IPoolable
    {
        private static readonly List<Projectile> ActiveEnemyProjectiles = new List<Projectile>(128);

        [SerializeField] internal SpriteRenderer body;
        [SerializeField] internal Collider2D hitCollider;

        private PooledObject _pooled;
        private ProjectileSpec _spec;
        private Vector2 _direction;
        private float _age;
        private int _pierceLeft;
        private PlayArea _area;
        private VfxSpawner _vfx;
        private bool _launched;

        public Faction Faction => _spec.Faction;

        private void Awake()
        {
            _pooled = GetComponent<PooledObject>();
            if (body == null) body = GetComponentInChildren<SpriteRenderer>();
            if (hitCollider == null) hitCollider = GetComponent<Collider2D>();
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

            transform.position = position;
            transform.rotation = Quaternion.FromToRotation(Vector3.up, _direction);
            transform.localScale = Vector3.one * Mathf.Max(0.05f, spec.Scale);
            gameObject.layer = GameLayers.ProjectileLayerFor(spec.Faction);

            if (body != null)
            {
                body.color = spec.Color;
                body.sortingOrder = SortingOrders.Projectile;
            }

            if (spec.Faction == Faction.Enemy) ActiveEnemyProjectiles.Add(this);
        }

        private void Update()
        {
            if (!_launched) return;
            float dt = Time.deltaTime;
            _age += dt;

            if (_spec.Homing && _spec.HomingTarget != null && _spec.HomingTarget.gameObject.activeInHierarchy)
            {
                Vector2 toTarget = ((Vector2)_spec.HomingTarget.position - (Vector2)transform.position).normalized;
                float maxRad = _spec.TurnRateDegrees * Mathf.Deg2Rad * dt;
                _direction = Vector3.RotateTowards(_direction, toTarget, maxRad, 0f);
                transform.rotation = Quaternion.FromToRotation(Vector3.up, _direction);
            }

            transform.position += (Vector3)(_direction * (_spec.Speed * dt));

            if (_age >= _spec.Lifetime || (_area != null && _area.IsOutside(transform.position, _area.DespawnMargin)))
                Despawn();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_launched) return;
            if (!other.TryGetComponent<IDamageable>(out var target)) return;
            if (target.Faction == _spec.Faction || !target.IsAlive) return;

            var info = new DamageInfo(_spec.Damage, _spec.Source, _spec.Type);
            Vector2 hit = other.ClosestPoint(transform.position);
            var result = target.ApplyDamage(info, hit);

            if (_vfx != null)
            {
                if (result.Applied && result.ShieldDamage > 0f && result.HullDamage <= 0f)
                    _vfx.SpawnShieldHit(hit, _spec.Color);
                else
                    _vfx.SpawnImpact(hit, _spec.Color);
            }

            if (_pierceLeft > 0 && result.Applied && !result.Killed)
            {
                _pierceLeft--;
                return;
            }
            Despawn();
        }

        public void Despawn()
        {
            if (!_launched) return;
            _launched = false;
            _pooled.Release();
        }

        void IPoolable.OnSpawned() { }

        void IPoolable.OnDespawned()
        {
            _launched = false;
            if (_spec.Faction == Faction.Enemy) ActiveEnemyProjectiles.Remove(this);
            _spec.HomingTarget = null;
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
