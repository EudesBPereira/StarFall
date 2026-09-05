using UnityEngine;

namespace Starfall.VFX
{
    /// <summary>Small positional shake on the camera. Uses scaled time so pausing freezes it.</summary>
    public sealed class CameraShake : MonoBehaviour
    {
        private Vector3 _origin;
        private float _amplitude;
        private float _remaining;
        private float _duration;

        private void Awake()
        {
            _origin = transform.localPosition;
        }

        public void Shake(float amplitude, float seconds)
        {
            if (amplitude <= 0f || seconds <= 0f) return;
            _amplitude = Mathf.Max(_amplitude, amplitude);
            _remaining = Mathf.Max(_remaining, seconds);
            _duration = _remaining;
        }

        private void LateUpdate()
        {
            if (_remaining <= 0f)
            {
                if (transform.localPosition != _origin) transform.localPosition = _origin;
                return;
            }
            _remaining -= Time.deltaTime;
            float k = _duration > 0f ? Mathf.Clamp01(_remaining / _duration) : 0f;
            Vector2 offset = Random.insideUnitCircle * (_amplitude * k);
            transform.localPosition = _origin + new Vector3(offset.x, offset.y, 0f);
            if (_remaining <= 0f)
            {
                _amplitude = 0f;
                transform.localPosition = _origin;
            }
        }
    }
}
