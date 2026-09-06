using System.Collections;
using Starfall.Audio;
using Starfall.Combat;
using Starfall.Core;
using Starfall.Logic;
using UnityEngine;

namespace Starfall.Waves
{
    /// <summary>
    /// Environmental events (plan §7.5). Every hazard is telegraphed before it can hurt, so it is never unfair:
    /// solar flare = visible band that becomes lethal after a warning; meteor shower = fast rocks; nebula pulse = fog.
    /// </summary>
    public sealed class HazardController : MonoBehaviour
    {
        [SerializeField] internal SpriteRenderer band;
        [SerializeField] internal SpriteRenderer fogOverlay;

        private const float FlareDamagePerSecond = 22f;
        private const float FlareTelegraph = 1.4f;
        private const float FlareActive = 1.8f;
        private const float FlareHeight = 2.2f;

        private Coroutine _routine;

        public bool IsRunning => _routine != null;

        /// <summary>Runs one hazard; the director waits for it. <paramref name="value"/> is a seeded [0,1) number.</summary>
        public IEnumerator Run(HazardKind kind, float value, GameplayContext ctx)
        {
            switch (kind)
            {
                case HazardKind.SolarFlare: yield return SolarFlare(value, ctx); break;
                case HazardKind.MeteorShower: yield return MeteorShower(value, ctx); break;
                case HazardKind.NebulaPulse: yield return NebulaPulse(value, ctx); break;
            }
        }

        private IEnumerator SolarFlare(float value, GameplayContext ctx)
        {
            if (band == null || ctx == null || ctx.PlayArea == null) yield break;
            var area = ctx.PlayArea;
            // Band position: never the very bottom (spawn line) so the player always has a lane.
            float y = Mathf.Lerp(area.Bottom + area.Height * 0.3f, area.Top - area.Height * 0.15f, value);
            var size = band.sprite != null ? band.sprite.bounds.size : Vector3.one;
            band.transform.position = new Vector3(area.Center.x, y, 0f);
            band.transform.localScale = new Vector3((area.Width + 2f) / Mathf.Max(0.01f, size.x), FlareHeight / Mathf.Max(0.01f, size.y), 1f);
            band.sortingOrder = SortingOrders.Fog + 1;
            band.enabled = true;
            GameSignals.RaiseStageMessage("SOLAR FLARE - CLEAR THE BAND", 1.4f);
            AudioManager.PlaySfx(SfxId.Alarm, 0.7f);

            float t = 0f;
            while (t < FlareTelegraph)
            {
                t += Time.deltaTime;
                float blink = 0.15f + 0.25f * Mathf.PingPong(t * 6f, 1f);
                band.color = new Color(1f, 0.82f, 0.4f, blink);
                yield return null;
            }

            t = 0f;
            float tick = 0f;
            if (ctx.CameraShake != null) ctx.CameraShake.Shake(0.15f, 0.3f);
            while (t < FlareActive)
            {
                t += Time.deltaTime;
                tick -= Time.deltaTime;
                band.color = new Color(1f, 0.55f, 0.2f, 0.55f + 0.15f * Mathf.Sin(t * 30f));
                var ship = ctx.Player;
                if (tick <= 0f && ship != null && ship.IsAlive && Mathf.Abs(ship.transform.position.y - y) <= FlareHeight * 0.5f)
                {
                    tick = 0.2f;
                    ship.Health.ApplyDamage(new DamageInfo(FlareDamagePerSecond * 0.2f, DamageSource.Environment, DamageType.Energy), ship.transform.position);
                }
                yield return null;
            }
            band.enabled = false;
        }

        private IEnumerator MeteorShower(float value, GameplayContext ctx)
        {
            if (ctx == null || ctx.CurrentStage == null || ctx.CurrentStage.AsteroidDefinition == null)
            {
                // Endless modes use the config survival look; fall back to the first asteroid we can find.
                yield break;
            }
            var def = ctx.CurrentStage.AsteroidDefinition;
            GameSignals.RaiseStageMessage("METEOR SHOWER", 1.4f);
            AudioManager.PlaySfx(SfxId.Alarm, 0.5f);
            yield return new WaitForSeconds(1f);
            int count = 8 + Mathf.RoundToInt(value * 6f);
            float speed = ctx.Spawner.SpeedMultiplier;
            for (int i = 0; i < count; i++)
            {
                var pos = SpawnPositionResolver.Resolve(ctx.PlayArea, SpawnPattern.TopRandom, 0f, 0, 1, 1.5f);
                ctx.Spawner.SpeedMultiplier = speed * 2.2f;
                var rock = ctx.Spawner.Spawn(def, pos);
                ctx.Spawner.SpeedMultiplier = speed;
                if (rock != null) rock.transform.localScale = Vector3.one * (def.Scale * Random.Range(0.5f, 1.1f));
                yield return new WaitForSeconds(0.28f);
            }
        }

        private IEnumerator NebulaPulse(float value, GameplayContext ctx)
        {
            if (fogOverlay == null || ctx == null || ctx.PlayArea == null) yield break;
            var area = ctx.PlayArea;
            var size = fogOverlay.sprite != null ? fogOverlay.sprite.bounds.size : Vector3.one;
            fogOverlay.transform.position = new Vector3(area.Center.x, area.Center.y, 0f);
            fogOverlay.transform.localScale = new Vector3((area.Width + 2f) / Mathf.Max(0.01f, size.x), (area.Height + 2f) / Mathf.Max(0.01f, size.y), 1f);
            fogOverlay.sortingOrder = SortingOrders.Fog;
            fogOverlay.enabled = true;
            GameSignals.RaiseStageMessage("NEBULA PULSE - LOW VISIBILITY", 1.4f);
            float peak = 0.28f + 0.12f * value;
            float duration = 6f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
                fogOverlay.color = new Color(0.55f, 0.25f, 0.85f, peak * k);
                yield return null;
            }
            fogOverlay.enabled = false;
        }
    }
}
