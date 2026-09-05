using UnityEngine;

namespace Starfall.VFX
{
    /// <summary>Cheap engine glow: a sprite whose length flickers every frame, plus an optional particle trail.</summary>
    public sealed class ThrusterFlicker : MonoBehaviour
    {
        [SerializeField] internal SpriteRenderer flame;
        [SerializeField] internal ParticleSystem trail;
        [SerializeField] internal float baseLength = 0.6f;
        [SerializeField] internal float flicker = 0.25f;

        private bool _active = true;

        public void SetActive(bool active)
        {
            _active = active;
            if (flame != null) flame.enabled = active;
            if (trail != null)
            {
                if (active) trail.Play(true);
                else trail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public void SetColor(Color color)
        {
            if (flame != null) flame.color = color;
            if (trail != null)
            {
                var main = trail.main;
                main.startColor = color;
            }
        }

        private void Update()
        {
            if (!_active || flame == null) return;
            float len = baseLength + Random.Range(-flicker, flicker) * baseLength;
            flame.transform.localScale = new Vector3(1f, Mathf.Max(0.1f, len), 1f);
        }
    }
}
