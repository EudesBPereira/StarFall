using System.Collections;
using System.Collections.Generic;
using Starfall.Bosses;
using Starfall.Core;
using Starfall.Enemies;
using Starfall.Logic;
using UnityEngine;

namespace Starfall.Waves
{
    /// <summary>
    /// Executes a stage. Campaign: the ordered <see cref="StageDefinition"/> events. Survival / Daily: endless
    /// procedural waves from <see cref="ProceduralWaves"/>. Boss Rush: every main boss in sequence.
    /// Raises <see cref="GameSignals.StageCompleted"/> when a finite run ends.
    /// </summary>
    public sealed class StageDirector : MonoBehaviour
    {
        private StageDefinition _stage;
        private GameplayContext _ctx;
        private Coroutine _routine;
        private Coroutine _asteroidRoutine;
        private bool _running;
        private int _eventIndex;
        private int _wave;
        private BossController _activeBoss;
        private readonly List<ProceduralSpawn> _spawnBuffer = new List<ProceduralSpawn>(8);

        public bool IsRunning => _running;
        public int EventIndex => _eventIndex;
        public int EventCount => _stage != null && _stage.Events != null ? _stage.Events.Length : 0;
        /// <summary>1-based wave number in endless modes (0 in campaign).</summary>
        public int Wave => _wave;
        public BossController ActiveBoss => _activeBoss;

        public void Begin(StageDefinition stage, GameplayContext ctx)
        {
            Stop();
            _stage = stage;
            _ctx = ctx;
            _wave = 0;
            if (ctx == null)
            {
                Debug.LogError("[Starfall] StageDirector.Begin called without a context.");
                return;
            }
            _running = true;
            switch (GameSession.Mode)
            {
                case GameModeId.Survival:
                case GameModeId.DailyChallenge:
                    _routine = StartCoroutine(RunEndless());
                    break;
                case GameModeId.BossRush:
                    _routine = StartCoroutine(RunBossRush());
                    break;
                default:
                    if (stage == null)
                    {
                        Debug.LogError("[Starfall] Campaign stage missing.");
                        _running = false;
                        return;
                    }
                    _routine = StartCoroutine(RunCampaign());
                    break;
            }
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

        // ---- Campaign ------------------------------------------------------------------------------------

        private IEnumerator RunCampaign()
        {
            var events = _stage.Events;
            for (_eventIndex = 0; _eventIndex < events.Length; _eventIndex++)
            {
                var ev = events[_eventIndex];
                if (ev == null) continue;
                switch (ev.Type)
                {
                    case StageEventType.Wave:
                        if (ev.Wave != null)
                        {
                            _wave++;
                            GameSignals.RaiseWaveStarted(_wave);
                            yield return RunWave(ev.Wave);
                        }
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
            yield return WaitForClear(wave.MaxDuration);
        }

        private IEnumerator WaitForClear(float timeout)
        {
            while (timeout > 0f && _ctx.Enemies.BlockingCount > 0)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator RunBoss(BossDefinition boss, string warning)
        {
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

        // ---- Endless (Survival / Daily) -----------------------------------------------------------------

        private IEnumerator RunEndless()
        {
            var config = _ctx.Config;
            bool daily = GameSession.Mode == GameModeId.DailyChallenge;
            int seed = GameSession.Seed;
            if (daily)
            {
                _ctx.Spawner.SpeedMultiplier = config.DailyEnemySpeedMultiplier;
                _ctx.Spawner.DropChanceMultiplier = config.DailyDropChanceMultiplier;
                GameSignals.RaiseStageMessage("DAILY CHALLENGE  -  FASTER ENEMIES, FEWER DROPS", 3f);
            }
            else
            {
                GameSignals.RaiseStageMessage("SURVIVAL  -  ENDLESS WAVES", 2.5f);
            }
            yield return new WaitForSeconds(2f);

            for (int waveIndex = 0; ; waveIndex++)
            {
                _wave = waveIndex + 1;
                _ctx.Spawner.StatMultiplier = ProceduralWaves.DifficultyMultiplier(waveIndex);
                GameSignals.RaiseWaveStarted(_wave);
                GameSignals.RaiseStageMessage($"WAVE {_wave}", 1.2f);

                if (config.SurvivalMiniBossEvery > 0 && _wave % config.SurvivalMiniBossEvery == 0 && config.SurvivalMiniBosses != null && config.SurvivalMiniBosses.Length > 0)
                {
                    var mini = config.SurvivalMiniBosses[(_wave / config.SurvivalMiniBossEvery - 1) % config.SurvivalMiniBosses.Length];
                    yield return RunBoss(mini, null);
                }

                ProceduralWaves.Generate(seed, waveIndex, _spawnBuffer);
                for (int s = 0; s < _spawnBuffer.Count; s++)
                {
                    var spawn = _spawnBuffer[s];
                    var def = config.GetProceduralEnemy(spawn.Enemy);
                    if (def == null) continue;
                    if (spawn.DelayBefore > 0f) yield return new WaitForSeconds(spawn.DelayBefore);
                    var pattern = (SpawnPattern)(spawn.Pattern % ProceduralWaves.PatternCount);
                    for (int i = 0; i < spawn.Count; i++)
                    {
                        var pos = SpawnPositionResolver.Resolve(_ctx.PlayArea, pattern, 0.5f, i, spawn.Count);
                        _ctx.Spawner.Spawn(def, pos);
                        if (spawn.Interval > 0f && i < spawn.Count - 1) yield return new WaitForSeconds(spawn.Interval);
                    }
                }

                yield return WaitForClear(45f);
                yield return new WaitForSeconds(config.SurvivalWavePause);
            }
        }

        // ---- Boss Rush -------------------------------------------------------------------------------------

        private IEnumerator RunBossRush()
        {
            var bosses = _ctx.Config.Bosses;
            GameSignals.RaiseStageMessage("BOSS RUSH", 2.5f);
            yield return new WaitForSeconds(2f);
            if (bosses != null)
            {
                for (int i = 0; i < bosses.Length; i++)
                {
                    if (bosses[i] == null) continue;
                    _wave = i + 1;
                    GameSignals.RaiseWaveStarted(_wave);
                    yield return RunBoss(bosses[i], null);
                    yield return new WaitForSeconds(1.5f);
                }
            }
            _running = false;
            GameSignals.RaiseStageCompleted();
        }

        // ---- Asteroids -------------------------------------------------------------------------------------

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
