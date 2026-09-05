using System.Collections;
using Starfall.Combat;
using Starfall.Pooling;
using UnityEngine;

namespace Starfall.VFX
{
    /// <summary>Central place to spawn pooled visual feedback. Every method is null-safe.</summary>
    public sealed class VfxSpawner : MonoBehaviour
    {
        [SerializeField] internal PoolService pools;
        [SerializeField] internal PooledObject explosionPrefab;
        [SerializeField] internal PooledObject floatingTextPrefab;
        [SerializeField] internal SpriteRenderer screenFlash;

        private Coroutine _flashRoutine;

        private void Start()
        {
            // Stretch the flash sprite over the visible area so it works on any aspect ratio.
            var ctx = Core.GameplayContext.Current;
            if (screenFlash == null || ctx == null || ctx.PlayArea == null || screenFlash.sprite == null) return;
            var area = ctx.PlayArea;
            var size = screenFlash.sprite.bounds.size;
            screenFlash.transform.position = new Vector3(area.Center.x, area.Center.y, 0f);
            screenFlash.transform.localScale = new Vector3(size.x > 0f ? (area.Width + 2f) / size.x : 1f, size.y > 0f ? (area.Height + 2f) / size.y : 1f, 1f);
            screenFlash.sortingOrder = SortingOrders.Vfx + 5;
            screenFlash.enabled = false;
        }

        private ExplosionEffect SpawnBurst(Vector2 position)
        {
            if (pools == null || explosionPrefab == null) return null;
            return pools.Spawn<ExplosionEffect>(explosionPrefab, position, Quaternion.identity);
        }

        public void SpawnExplosion(Vector2 position, float scale, Color color)
        {
            var fx = SpawnBurst(position);
            if (fx != null) fx.Play(scale * 1.4f, color, 0.5f, true);
        }

        public void SpawnImpact(Vector2 position, Color color)
        {
            var fx = SpawnBurst(position);
            if (fx != null) fx.Play(0.35f, color, 0.18f, false);
        }

        public void SpawnShieldHit(Vector2 position, Color color)
        {
            var fx = SpawnBurst(position);
            if (fx != null) fx.Play(0.5f, new Color(color.r, color.g, color.b, 0.8f), 0.22f, true);
        }

        public void SpawnShieldBreak(Vector2 position, Color color)
        {
            var fx = SpawnBurst(position);
            if (fx != null) fx.Play(1.3f, new Color(color.r, color.g, color.b, 0.7f), 0.35f, true);
        }

        public void SpawnMuzzleFlash(Vector2 position, Color color)
        {
            var fx = SpawnBurst(position);
            if (fx != null) fx.Play(0.28f, color, 0.08f, false);
        }

        public void SpawnPickupBurst(Vector2 position, Color color)
        {
            var fx = SpawnBurst(position);
            if (fx != null) fx.Play(1.1f, new Color(color.r, color.g, color.b, 0.6f), 0.4f, true);
        }

        public void SpawnFloatingText(Vector2 position, string text, Color color)
        {
            if (pools == null || floatingTextPrefab == null || string.IsNullOrEmpty(text)) return;
            var ft = pools.Spawn<FloatingText>(floatingTextPrefab, position, Quaternion.identity);
            if (ft != null) ft.Show(text, color);
        }

        public void SpawnExplosionChain(Vector2 center, int count, float radius, float seconds, Color color)
        {
            StartCoroutine(ExplosionChainRoutine(center, count, radius, seconds, color));
        }

        private IEnumerator ExplosionChainRoutine(Vector2 center, int count, float radius, float seconds, Color color)
        {
            float interval = seconds / Mathf.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Random.insideUnitCircle * radius;
                SpawnExplosion(center + offset, Random.Range(0.8f, 1.6f), color);
                yield return new WaitForSeconds(interval);
            }
            SpawnExplosion(center, 3f, Color.white);
        }

        public void FlashScreen(Color color, float seconds)
        {
            if (screenFlash == null) return;
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine(color, seconds));
        }

        private IEnumerator FlashRoutine(Color color, float seconds)
        {
            screenFlash.enabled = true;
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float k = 1f - Mathf.Clamp01(t / seconds);
                screenFlash.color = new Color(color.r, color.g, color.b, color.a * k * k);
                yield return null;
            }
            screenFlash.enabled = false;
            _flashRoutine = null;
        }
    }
}
