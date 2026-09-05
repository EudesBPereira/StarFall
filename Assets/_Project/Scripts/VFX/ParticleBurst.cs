using Starfall.Pooling;
using UnityEngine;

namespace Starfall.VFX
{
    /// <summary>Pooled one-shot ParticleSystem (sparks, smoke puffs). Returns to the pool when finished.</summary>
    [RequireComponent(typeof(PooledObject), typeof(ParticleSystem))]
    public sealed class ParticleBurst : MonoBehaviour, IPoolable
    {
        private PooledObject _pooled;
        private ParticleSystem _system;
        private bool _playing;

        private void Awake()
        {
            _pooled = GetComponent<PooledObject>();
            _system = GetComponent<ParticleSystem>();
        }

        public void Play(Color color, int count)
        {
            var main = _system.main;
            main.startColor = color;
            _system.Clear(true);
            _system.Emit(Mathf.Clamp(count, 1, 64));
            _playing = true;
        }

        private void Update()
        {
            if (!_playing) return;
            if (!_system.IsAlive(true))
            {
                _playing = false;
                _pooled.Release();
            }
        }

        void IPoolable.OnSpawned() { }

        void IPoolable.OnDespawned()
        {
            _playing = false;
            if (_system != null) _system.Clear(true);
        }
    }
}
