using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>
    /// "2D animated" ship preview for the Hangar: hovers, tilts and flickers like a hologram. Uses the hologram
    /// material when assigned; falls back to a plain colour flicker.
    /// </summary>
    public sealed class HologramPreview : MonoBehaviour
    {
        [SerializeField] internal Image image;
        [SerializeField] internal Image glow;
        [SerializeField] internal float hoverAmplitude = 14f;
        [SerializeField] internal float tiltDegrees = 6f;

        private RectTransform _target;
        private Vector2 _origin;

        public void SetTarget(RectTransform target)
        {
            _target = target;
            if (_target != null) _origin = _target.anchoredPosition;
        }

        private void Update()
        {
            var t = _target != null ? _target : (image != null ? image.rectTransform : null);
            if (t == null) return;
            float time = Time.unscaledTime;
            t.anchoredPosition = _origin + new Vector2(0f, Mathf.Sin(time * 1.6f) * hoverAmplitude);
            t.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(time * 1.1f) * tiltDegrees);
            float flicker = 0.85f + 0.15f * Mathf.PerlinNoise(time * 6f, 0.5f);
            if (image != null && image.material != null) image.material.SetFloat("_Flicker", flicker);
            if (glow != null)
            {
                var c = glow.color;
                c.a = 0.25f + 0.2f * Mathf.Sin(time * 2.4f);
                glow.color = c;
                glow.rectTransform.localScale = Vector3.one * (1f + 0.05f * Mathf.Sin(time * 2.4f));
            }
        }
    }
}
