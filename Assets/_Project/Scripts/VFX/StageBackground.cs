using System.Collections.Generic;
using Starfall.Combat;
using Starfall.Core;
using Starfall.Waves;
using UnityEngine;

namespace Starfall.VFX
{
    /// <summary>
    /// Procedural background: gradient, scrolling star layers, optional drifting debris and a fog overlay.
    /// Everything is built from a single white sprite so no art assets are required (placeholder look).
    /// </summary>
    public sealed class StageBackground : MonoBehaviour
    {
        [SerializeField] internal PlayArea area;
        [SerializeField] internal Sprite dotSprite;
        [SerializeField] internal Sprite gradientSprite;
        [SerializeField] internal Camera targetCamera;
        [SerializeField] internal int starCount = 90;
        [Tooltip("Optional look applied automatically on Start (used by the main menu).")]
        [SerializeField] internal StageDefinition autoApply;

        private struct Star
        {
            public Transform T;
            public float Speed;
        }

        private readonly List<Star> _stars = new List<Star>(128);
        private readonly List<Transform> _debris = new List<Transform>(8);
        private SpriteRenderer _gradient;
        private SpriteRenderer _fog;
        private Transform _starRoot;
        private StageDefinition _stage;
        private float _debrisTimer;

        private void Start()
        {
            if (_stage == null && autoApply != null) Apply(autoApply);
        }

        public void Apply(StageDefinition stage)
        {
            _stage = stage;
            if (area == null) area = GameplayContext.Current != null ? GameplayContext.Current.PlayArea : null;
            if (targetCamera == null) targetCamera = Camera.main;
            if (stage == null || area == null) return;

            if (targetCamera != null)
            {
                targetCamera.clearFlags = CameraClearFlags.SolidColor;
                targetCamera.backgroundColor = stage.BackgroundBottom;
            }

            EnsureLayer(ref _gradient, "Gradient", SortingOrders.Background, gradientSprite);
            _gradient.color = stage.BackgroundTop;
            FitToScreen(_gradient.transform, gradientSprite);

            EnsureLayer(ref _fog, "Fog", SortingOrders.Fog, dotSprite);
            _fog.color = stage.Fog;
            _fog.enabled = stage.Fog.a > 0.001f;
            FitToScreen(_fog.transform, dotSprite);

            BuildStars(stage);
        }

        private void EnsureLayer(ref SpriteRenderer sr, string name, int order, Sprite sprite)
        {
            if (sr != null) return;
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
        }

        private void FitToScreen(Transform t, Sprite sprite)
        {
            if (sprite == null || area == null) return;
            var size = sprite.bounds.size;
            float sx = size.x > 0f ? (area.Width + 1f) / size.x : 1f;
            float sy = size.y > 0f ? (area.Height + 1f) / size.y : 1f;
            t.localScale = new Vector3(sx, sy, 1f);
            t.position = new Vector3(area.Center.x, area.Center.y, 5f);
        }

        private void BuildStars(StageDefinition stage)
        {
            if (_starRoot == null)
            {
                _starRoot = new GameObject("Stars").transform;
                _starRoot.SetParent(transform, false);
            }
            for (int i = 0; i < _stars.Count; i++) if (_stars[i].T != null) Destroy(_stars[i].T.gameObject);
            _stars.Clear();
            if (dotSprite == null) return;

            int count = Mathf.RoundToInt(starCount * Mathf.Clamp01(stage.StarDensity));
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Star");
                go.transform.SetParent(_starRoot, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = dotSprite;
                float depth = Random.value; // 0 far, 1 near
                float size = Mathf.Lerp(0.03f, 0.09f, depth);
                sr.transform.localScale = Vector3.one * size;
                var c = stage.StarTint;
                c.a = Mathf.Lerp(0.25f, 0.9f, depth);
                sr.color = c;
                sr.sortingOrder = SortingOrders.Stars;
                go.transform.position = new Vector3(Random.Range(area.Left, area.Right), Random.Range(area.Bottom, area.Top), 4f);
                _stars.Add(new Star { T = go.transform, Speed = Mathf.Lerp(0.6f, 3.2f, depth) });
            }
        }

        private void Update()
        {
            if (area == null) return;
            float dt = Time.deltaTime;
            float bottom = area.Bottom - 0.5f;
            float top = area.Top + 0.5f;

            for (int i = 0; i < _stars.Count; i++)
            {
                var s = _stars[i];
                var p = s.T.position;
                p.y -= s.Speed * dt;
                if (p.y < bottom)
                {
                    p.y = top;
                    p.x = Random.Range(area.Left, area.Right);
                }
                s.T.position = p;
            }

            for (int i = _debris.Count - 1; i >= 0; i--)
            {
                var t = _debris[i];
                t.position += Vector3.down * (0.9f * dt);
                t.Rotate(0f, 0f, 12f * dt);
                if (t.position.y < bottom - 2f)
                {
                    Destroy(t.gameObject);
                    _debris.RemoveAt(i);
                }
            }

            if (_stage != null && _stage.DebrisInterval > 0f && _stage.DebrisSprites != null && _stage.DebrisSprites.Length > 0)
            {
                _debrisTimer -= dt;
                if (_debrisTimer <= 0f)
                {
                    _debrisTimer = _stage.DebrisInterval * Random.Range(0.7f, 1.4f);
                    SpawnDebris();
                }
            }
        }

        private void SpawnDebris()
        {
            var sprite = _stage.DebrisSprites[Random.Range(0, _stage.DebrisSprites.Length)];
            if (sprite == null) return;
            var go = new GameObject("Debris");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = SortingOrders.Debris;
            sr.color = new Color(0.6f, 0.65f, 0.75f, 0.55f);
            go.transform.localScale = Vector3.one * Random.Range(0.8f, 1.8f);
            go.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            go.transform.position = new Vector3(Random.Range(area.Left, area.Right), area.Top + 2f, 3f);
            _debris.Add(go.transform);
        }
    }
}
