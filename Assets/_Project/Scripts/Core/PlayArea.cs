using UnityEngine;

namespace Starfall.Core
{
    /// <summary>
    /// World-space rectangle visible by the gameplay camera. All spawn/clamp/despawn math goes through here
    /// so the game works on any aspect ratio (portrait phones, tablets, editor windows).
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public sealed class PlayArea : MonoBehaviour
    {
        [SerializeField] internal Camera targetCamera;
        [SerializeField] internal float targetWidth = 10f;
        [SerializeField] internal float despawnMargin = 2.5f;

        private Rect _bounds;
        private int _lastWidth, _lastHeight;
        private float _lastSize;

        public Rect Bounds
        {
            get
            {
                RefreshIfNeeded();
                return _bounds;
            }
        }

        public float DespawnMargin => despawnMargin;
        public float Top => Bounds.yMax;
        public float Bottom => Bounds.yMin;
        public float Left => Bounds.xMin;
        public float Right => Bounds.xMax;
        public float Width => Bounds.width;
        public float Height => Bounds.height;
        public Vector2 Center => Bounds.center;

        private void Awake()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            ApplyCameraSize();
            Refresh();
        }

        public void Configure(float width, float margin)
        {
            targetWidth = width;
            despawnMargin = margin;
            ApplyCameraSize();
            Refresh();
        }

        private void Update()
        {
            RefreshIfNeeded();
        }

        private void RefreshIfNeeded()
        {
            if (targetCamera == null) return;
            if (Screen.width != _lastWidth || Screen.height != _lastHeight || !Mathf.Approximately(_lastSize, targetCamera.orthographicSize))
            {
                ApplyCameraSize();
                Refresh();
            }
        }

        private void ApplyCameraSize()
        {
            if (targetCamera == null || !targetCamera.orthographic) return;
            float aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 1f;
            if (aspect <= 0f) aspect = 1f;
            targetCamera.orthographicSize = targetWidth * 0.5f / aspect;
        }

        private void Refresh()
        {
            if (targetCamera == null)
            {
                _bounds = new Rect(-5f, -8f, 10f, 16f);
                return;
            }
            float halfH = targetCamera.orthographicSize;
            float aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 1f;
            float halfW = halfH * aspect;
            Vector3 c = targetCamera.transform.position;
            _bounds = new Rect(c.x - halfW, c.y - halfH, halfW * 2f, halfH * 2f);
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            _lastSize = targetCamera.orthographicSize;
        }

        public Vector2 Clamp(Vector2 position, float padding)
        {
            var b = Bounds;
            return new Vector2(
                Mathf.Clamp(position.x, b.xMin + padding, b.xMax - padding),
                Mathf.Clamp(position.y, b.yMin + padding, b.yMax - padding));
        }

        public bool IsOutside(Vector2 position, float margin)
        {
            var b = Bounds;
            return position.x < b.xMin - margin || position.x > b.xMax + margin ||
                   position.y < b.yMin - margin || position.y > b.yMax + margin;
        }

        public bool IsInside(Vector2 position, float padding = 0f)
        {
            var b = Bounds;
            return position.x >= b.xMin + padding && position.x <= b.xMax - padding &&
                   position.y >= b.yMin + padding && position.y <= b.yMax - padding;
        }

        /// <summary>Horizontal position for a normalized value in [0,1], with padding from both edges.</summary>
        public float LerpX(float t, float padding = 0.5f)
        {
            var b = Bounds;
            return Mathf.Lerp(b.xMin + padding, b.xMax - padding, Mathf.Clamp01(t));
        }

        /// <summary>Vertical position for a normalized value in [0,1] measured from the bottom.</summary>
        public float LerpY(float t, float padding = 0f)
        {
            var b = Bounds;
            return Mathf.Lerp(b.yMin + padding, b.yMax - padding, Mathf.Clamp01(t));
        }

        public Vector2 PlayerSpawnPosition(float heightFraction)
        {
            var b = Bounds;
            return new Vector2(b.center.x, Mathf.Lerp(b.yMin, b.yMax, heightFraction));
        }
    }
}
