using Starfall.Combat;
using Starfall.Core;
using Starfall.Player;
using Starfall.Pooling;
using UnityEngine;

namespace Starfall.PowerUps
{
    /// <summary>Pooled collectible. Drifts down with a gentle sway and is consumed by the player ship.</summary>
    [RequireComponent(typeof(PooledObject))]
    public sealed class PowerUpPickup : MonoBehaviour, IPoolable
    {
        [SerializeField] internal SpriteRenderer body;
        [SerializeField] internal SpriteRenderer ring;

        private PooledObject _pooled;
        private PowerUpDefinition _definition;
        private PlayArea _area;
        private float _age;
        private float _startX;
        private bool _collected;

        public PowerUpDefinition Definition => _definition;

        private void Awake()
        {
            _pooled = GetComponent<PooledObject>();
        }

        public void Initialize(PowerUpDefinition definition, PlayArea area)
        {
            _definition = definition;
            _area = area;
            _age = 0f;
            _startX = transform.position.x;
            _collected = false;
            gameObject.layer = GameLayers.PowerUp;
            if (body != null)
            {
                body.color = definition.Color;
                body.sortingOrder = SortingOrders.PowerUp;
            }
            if (ring != null)
            {
                ring.color = new Color(1f, 1f, 1f, 0.9f);
                ring.sortingOrder = SortingOrders.PowerUp + 1;
            }
        }

        private void Update()
        {
            if (_definition == null) return;
            _age += Time.deltaTime;
            var p = transform.position;
            var ctx = GameplayContext.Current;
            var ship = ctx != null ? ctx.Player : null;
            // Overdrive attracts pickups (plan §5.2 "atração de itens").
            if (ship != null && ship.IsAlive && ((ship.Risk != null && ship.Risk.Overdrive.IsActive) || ship.MagnetActive))
            {
                p = Vector3.MoveTowards(p, ship.transform.position, 9f * Time.deltaTime);
            }
            else
            {
                p.y -= _definition.FallSpeed * Time.deltaTime;
                p.x = _startX + Mathf.Sin(_age * 2.2f) * _definition.SwayAmplitude;
            }
            transform.position = p;
            if (ring != null) ring.transform.localScale = Vector3.one * (1f + 0.12f * Mathf.Sin(_age * 6f));

            if (_area != null && _area.IsOutside(p, 1f)) _pooled.Release();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected || _definition == null) return;
            if (!other.TryGetComponent<PlayerShip>(out var ship) || !ship.IsAlive) return;
            _collected = true;
            ship.CollectPowerUp(_definition);
            _pooled.Release();
        }

        void IPoolable.OnSpawned() { }

        void IPoolable.OnDespawned()
        {
            _definition = null;
            _collected = false;
        }
    }
}
