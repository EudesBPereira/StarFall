using Starfall.Combat;
using Starfall.Pooling;
using TMPro;
using UnityEngine;

namespace Starfall.VFX
{
    /// <summary>World-space text that rises and fades (power-up feedback, bonuses).</summary>
    [RequireComponent(typeof(PooledObject))]
    public sealed class FloatingText : MonoBehaviour, IPoolable
    {
        [SerializeField] internal TextMeshPro label;

        private PooledObject _pooled;
        private float _age;
        private float _duration = 0.9f;
        private Color _color;
        private bool _playing;

        private void Awake()
        {
            _pooled = GetComponent<PooledObject>();
        }

        public void Show(string text, Color color, float duration = 0.9f)
        {
            _age = 0f;
            _duration = Mathf.Max(0.1f, duration);
            _color = color;
            _playing = true;
            if (label != null)
            {
                label.text = text;
                label.color = color;
                label.sortingOrder = SortingOrders.FloatingText;
            }
        }

        private void Update()
        {
            if (!_playing) return;
            _age += Time.deltaTime;
            float k = Mathf.Clamp01(_age / _duration);
            transform.position += Vector3.up * (1.2f * Time.deltaTime);
            if (label != null)
            {
                var c = _color;
                c.a = 1f - k * k;
                label.color = c;
            }
            if (k >= 1f)
            {
                _playing = false;
                _pooled.Release();
            }
        }

        void IPoolable.OnSpawned() { }
        void IPoolable.OnDespawned() { _playing = false; }
    }
}
