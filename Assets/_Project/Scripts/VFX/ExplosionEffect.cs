using Starfall.Combat;
using Starfall.Pooling;
using UnityEngine;

namespace Starfall.VFX
{
    /// <summary>
    /// Code-driven sprite burst (grow + fade) used for explosions, impacts, shield hits and muzzle flashes.
    /// No particle assets required; a ParticleSystem child is played when present.
    /// </summary>
    [RequireComponent(typeof(PooledObject))]
    public sealed class ExplosionEffect : MonoBehaviour, IPoolable
    {
        [SerializeField] internal SpriteRenderer sprite;
        [SerializeField] internal SpriteRenderer ring;
        [SerializeField] internal ParticleSystem particles;

        private PooledObject _pooled;
        private float _duration;
        private float _age;
        private float _startScale;
        private float _endScale;
        private Color _color;
        private bool _playing;

        private void Awake()
        {
            _pooled = GetComponent<PooledObject>();
        }

        public void Play(float scale, Color color, float duration = 0.45f, bool withRing = true)
        {
            _duration = Mathf.Max(0.05f, duration);
            _age = 0f;
            _startScale = scale * 0.35f;
            _endScale = scale;
            _color = color;
            _playing = true;

            if (sprite != null)
            {
                sprite.enabled = true;
                sprite.color = color;
                sprite.sortingOrder = SortingOrders.Vfx;
                sprite.transform.localScale = Vector3.one * _startScale;
                sprite.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            }
            if (ring != null)
            {
                ring.enabled = withRing;
                ring.color = new Color(1f, 1f, 1f, 0.9f);
                ring.sortingOrder = SortingOrders.Vfx + 1;
                ring.transform.localScale = Vector3.one * _startScale;
            }
            if (particles != null)
            {
                var main = particles.main;
                main.startColor = color;
                particles.Play(true);
            }
        }

        private void Update()
        {
            if (!_playing) return;
            _age += Time.deltaTime;
            float k = Mathf.Clamp01(_age / _duration);
            float ease = 1f - (1f - k) * (1f - k);

            if (sprite != null)
            {
                sprite.transform.localScale = Vector3.one * Mathf.Lerp(_startScale, _endScale, ease);
                var c = _color;
                c.a = _color.a * (1f - k);
                sprite.color = c;
            }
            if (ring != null && ring.enabled)
            {
                ring.transform.localScale = Vector3.one * Mathf.Lerp(_startScale, _endScale * 1.6f, ease);
                ring.color = new Color(1f, 1f, 1f, 0.9f * (1f - k) * (1f - k));
            }

            if (k >= 1f && (particles == null || !particles.IsAlive(true)))
            {
                _playing = false;
                _pooled.Release();
            }
        }

        void IPoolable.OnSpawned() { }

        void IPoolable.OnDespawned()
        {
            _playing = false;
            if (particles != null) particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
