using Starfall.Combat;
using Starfall.Logic;
using Starfall.Player;
using UnityEngine;

namespace Starfall.Bosses
{
    /// <summary>
    /// Front laser used by The Destroyer. Shows a thin telegraph line first, then a wide damaging beam.
    /// Damage is applied in ticks so the player can escape with a short exposure.
    /// </summary>
    public sealed class LaserBeam : MonoBehaviour
    {
        [SerializeField] internal SpriteRenderer beamSprite;
        [SerializeField] internal BoxCollider2D beamCollider;

        private float _damagePerSecond;
        private float _tickTimer;
        private bool _active;
        private float _width = 1f;
        private float _length = 20f;
        private Color _color = new Color(1f, 0.3f, 0.5f, 1f);

        private const float TickSeconds = 0.2f;

        public bool IsActive => _active;

        public void Configure(float width, float length, Color color)
        {
            _width = width;
            _length = length;
            _color = color;
            gameObject.layer = GameLayers.EnemyProjectile;
            if (beamSprite != null) beamSprite.sortingOrder = SortingOrders.Laser;
            if (beamCollider != null)
            {
                beamCollider.isTrigger = true;
                beamCollider.size = new Vector2(width, length);
                beamCollider.offset = new Vector2(0f, -length * 0.5f);
            }
            Hide();
        }

        public void ShowTelegraph(float progress)
        {
            if (beamSprite == null) return;
            beamSprite.enabled = true;
            float w = Mathf.Lerp(0.06f, _width * 0.25f, progress);
            beamSprite.transform.localScale = new Vector3(w, _length, 1f);
            beamSprite.transform.localPosition = new Vector3(0f, -_length * 0.5f, 0f);
            var c = _color;
            c.a = 0.35f + 0.35f * Mathf.PingPong(Time.time * 8f, 1f);
            beamSprite.color = c;
            if (beamCollider != null) beamCollider.enabled = false;
            _active = false;
        }

        public void Fire(float damagePerSecond)
        {
            _damagePerSecond = damagePerSecond;
            _tickTimer = 0f;
            _active = true;
            if (beamSprite != null)
            {
                beamSprite.enabled = true;
                beamSprite.transform.localScale = new Vector3(_width, _length, 1f);
                beamSprite.transform.localPosition = new Vector3(0f, -_length * 0.5f, 0f);
                beamSprite.color = _color;
            }
            if (beamCollider != null) beamCollider.enabled = true;
        }

        public void Hide()
        {
            _active = false;
            if (beamSprite != null) beamSprite.enabled = false;
            if (beamCollider != null) beamCollider.enabled = false;
        }

        private void Update()
        {
            if (!_active) return;
            _tickTimer -= Time.deltaTime;
            if (beamSprite != null)
            {
                float pulse = 0.85f + 0.15f * Mathf.Sin(Time.time * 40f);
                beamSprite.transform.localScale = new Vector3(_width * pulse, _length, 1f);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!_active || _tickTimer > 0f) return;
            if (!other.TryGetComponent<PlayerShip>(out var ship) || !ship.IsAlive) return;
            _tickTimer = TickSeconds;
            var info = new DamageInfo(_damagePerSecond * TickSeconds, DamageSource.Boss, DamageType.Laser);
            ship.Health.ApplyDamage(info, other.ClosestPoint(transform.position));
        }
    }
}
