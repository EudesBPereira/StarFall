using System;
using Starfall.Combat;
using Starfall.Logic;
using UnityEngine;

namespace Starfall.Bosses
{
    /// <summary>
    /// Destructible boss component (turret, crystal, fabricator). Has its own health and collider, follows the boss,
    /// and tells the controller when it dies so an attack can be disabled or the core shield dropped.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public sealed class BossPart : MonoBehaviour
    {
        [SerializeField] internal SpriteRenderer body;
        [SerializeField] internal CircleCollider2D hitbox;

        private Health _health;
        private int _index;
        private BossPartSpec _spec;
        private Color _tint = Color.white;

        public int Index => _index;
        public BossPartSpec Spec => _spec;
        public bool IsAlive => _health != null && _health.IsAlive && gameObject.activeSelf;
        public event Action<BossPart> Destroyed;
        public event Action<BossPart, DamageResult> Damaged;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _health.Died += OnDied;
            _health.DamagedAt += OnDamaged;
        }

        private void OnDestroy()
        {
            if (_health == null) return;
            _health.Died -= OnDied;
            _health.DamagedAt -= OnDamaged;
        }

        public void Setup(int index, in BossPartSpec spec, float statMultiplier)
        {
            _index = index;
            _spec = spec;
            gameObject.layer = GameLayers.Enemy;
            transform.localPosition = new Vector3(spec.Offset.x, spec.Offset.y, 0f);
            transform.localScale = Vector3.one * (spec.Scale > 0f ? spec.Scale : 1f);
            if (hitbox != null) { hitbox.radius = spec.Radius; hitbox.isTrigger = true; hitbox.enabled = true; }
            _tint = spec.Tint.a > 0f ? spec.Tint : Color.white;
            if (body != null)
            {
                if (spec.Sprite != null) body.sprite = spec.Sprite;
                body.color = _tint;
                body.enabled = true;
                body.sortingOrder = SortingOrders.Enemy + 1;
            }
            _health.Configure(Faction.Enemy, Mathf.Max(1f, spec.Hull * statMultiplier), 0f);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnDamaged(DamageInfo info, DamageResult result, Vector2 hitPoint)
        {
            if (result.Killed) return;
            if (body != null)
            {
                float f = _health.MaxHull > 0f ? _health.Hull / _health.MaxHull : 0f;
                body.color = Color.Lerp(new Color(1f, 0.3f, 0.3f), _tint, f);
            }
            Damaged?.Invoke(this, result);
        }

        private void OnDied(DamageInfo info)
        {
            if (hitbox != null) hitbox.enabled = false;
            if (body != null) body.enabled = false;
            Destroyed?.Invoke(this);
        }
    }
}
