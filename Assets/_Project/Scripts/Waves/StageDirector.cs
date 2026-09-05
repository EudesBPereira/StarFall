using System.Collections;
using Starfall.Bosses;
using Starfall.Core;
using Starfall.Enemies;
using UnityEngine;

namespace Starfall.Waves
{
    /// <summary>
    /// Executes a <see cref="StageDefinition"/>: waves, delays, messages, asteroid fields and bosses, in order.
    /// Raises <see cref="GameSignals.StageCompleted"/> when the last event finishes.
    /// </summary>
    public sealed class StageDirector : MonoBehaviour
    {
        private StageDefinition _stage;
        private GameplayContext _ctx;
        private Coroutine _routine;
        private Coroutine _asteroidRoutine;
        private bool _running;
        private int _eventIndex;
        private BossController _activeBoss;

        public bool IsRunning => _running;
        public int EventIndex => _eventIndex;
        public int EventCount => _stage != null && _stage.Events != null ? _stage.Events.Length : 0;
        public BossController ActiveBoss => _activeBoss;

        public void Begin(StageDefinition stage, GameplayContext ctx)
        {
            Stop();
            _stage = stage;
            _ctx = ctx;
            if (stage == null || ctx == null)
            {
                Debug.LogError("[Starfall] StageDirector.Begin called without a stage or context.");
                return;
            }
            _running = true;
            _routine = StartCoroutine(Run());
        }

        public void Stop()
        {
            _running = false;
            if (_routine != null) StopCoroutine(_routine);
            if (_asteroidRoutine != null) StopCoroutine(_asteroidRoutine);
            _routine = null;
            _asteroidRoutine = null;
            _activeBoss = null;
        }

        private IEnumerator Run()
        {
            var events = _stage.Events;
            for (_eventIndex = 0; _eventIndex < events.Length; _eventIndex++)
            {
                var ev = events[_eventIndex];
                if (ev == null) continue;
                switch (ev.Type)
                {
                    case StageEventType.Wave:
                        if (ev.Wave != null) yield return RunWave(ev.Wave);
                        break;
                    case StageEventType.Delay:
                        yield return new WaitForSeconds(ev.Seconds);
                        break;
                    case StageEventType.Message:
                        GameSignals.RaiseStageMessage(ev.Message, ev.Seconds > 0f ? ev.Seconds : 2f);
                        break;
                    case StageEventType.AsteroidField:
                        SetAsteroidField(ev.Flag);
                        break;
                    case StageEventType.MiniBoss:
                    case StageEventType.Boss:
                        if (ev.Boss != null) yield return RunBoss(ev.Boss, ev.Message);
                        break;
                }
            }

            SetAsteroidField(false);
            _running = false;
            GameSignals.RaiseStageCompleted();
        }

        private IEnumerator RunWave(WaveDefinition wave)
        {
            var entries = wave.Entries;
            if (entries != null)
            {
                for (int e = 0; e < entries.Length; e++)
                {
                    var entry = entries[e];
                    if (entry == null || entry.Enemy == null) continue;
                    if (entry.DelayBefore > 0f) yield return new WaitForSeconds(entry.DelayBefore);
                    for (int i = 0; i < entry.Count; i++)
                    {
                        var pos = SpawnPositionResolver.Resolve(_ctx.PlayArea, entry.Pattern, entry.PatternValue, i, entry.Count);
                        _ctx.Spawner.Spawn(entry.Enemy, pos);
                        if (entry.Interval > 0f && i < entry.Count - 1) yield return new WaitForSeconds(entry.Interval);
                    }
                }
            }

            if (!wave.WaitForClear) yield break;
            float timeout = wave.MaxDuration;
            while (timeout > 0f && _ctx.Enemies.BlockingCount > 0)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator RunBoss(BossDefinition boss, string warning)
        {
            // Let the screen clear a little, then announce.
            float wait = 1.5f;
            while (wait > 0f && _ctx.Enemies.BlockingCount > 0) { wait -= Time.deltaTime; yield return null; }
            GameSignals.RaiseStageMessage(string.IsNullOrEmpty(warning) ? $"WARNING: {boss.Title}" : warning, 2.2f);
            yield return new WaitForSeconds(1.2f);

            var spawnPos = new Vector2(_ctx.PlayArea.Center.x, _ctx.PlayArea.Top + 3f);
            var enemy = _ctx.Spawner.Spawn(boss, spawnPos);
            _activeBoss = enemy as BossController;
            if (_activeBoss == null)
            {
                Debug.LogError($"[Starfall] Boss '{boss.name}' prefab has no BossController.");
                yield break;
            }

            while (_activeBoss != null && _activeBoss.IsActiveInstance)
                yield return null;
            _activeBoss = null;
            yield return new WaitForSeconds(boss.DeathSequenceSeconds + 0.5f);
        }

        private void SetAsteroidField(bool enabled)
        {
            if (_asteroidRoutine != null) { StopCoroutine(_asteroidRoutine); _asteroidRoutine = null; }
            if (!enabled || _stage == null || _stage.AsteroidDefinition == null) return;
            _asteroidRoutine = StartCoroutine(AsteroidRoutine());
        }

        private IEnumerator AsteroidRoutine()
        {
            var def = _stage.AsteroidDefinition;
            while (true)
            {
                float t = Random.Range(_stage.AsteroidInterval * 0.6f, _stage.AsteroidInterval * 1.4f);
                yield return new WaitForSeconds(t);
                var pos = SpawnPositionResolver.Resolve(_ctx.PlayArea, SpawnPattern.TopRandom, 0f, 0, 1, 1.5f);
                var asteroid = _ctx.Spawner.Spawn(def, pos);
                if (asteroid != null)
                {
                    float s = Random.Range(0.7f, 1.4f);
                    asteroid.transform.localScale = Vector3.one * (def.Scale * s);
                }
            }
        }
    }
}
