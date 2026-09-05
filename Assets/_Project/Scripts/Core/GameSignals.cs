using System;
using Starfall.Enemies;
using Starfall.Logic;
using Starfall.PowerUps;
using UnityEngine;

namespace Starfall.Core
{
    /// <summary>Payload for <see cref="GameSignals.EnemyDestroyed"/>.</summary>
    public readonly struct EnemyKilledInfo
    {
        public readonly EnemyDefinition Definition;
        public readonly Vector2 Position;
        public readonly DamageSource Source;
        public readonly bool IsBoss;

        public EnemyKilledInfo(EnemyDefinition definition, Vector2 position, DamageSource source, bool isBoss)
        {
            Definition = definition;
            Position = position;
            Source = source;
            IsBoss = isBoss;
        }

        /// <summary>Kills caused by the player (weapons or Ultimate) count for scoring.</summary>
        public bool CountsForScore => Source == DamageSource.Player || Source == DamageSource.Ultimate;
    }

    /// <summary>
    /// Static, typed event bus that decouples gameplay producers (enemies, player) from consumers
    /// (score, HUD, audio). Subscribers MUST unsubscribe in OnDisable/OnDestroy.
    /// Justification in docs/ARCHITECTURE.md.
    /// </summary>
    public static class GameSignals
    {
        public static event Action<EnemyKilledInfo> EnemyDestroyed;
        public static event Action<DamageInfo, DamageResult> PlayerDamaged;
        public static event Action PlayerDied;
        public static event Action PlayerRespawned;
        public static event Action<PowerUpDefinition> PowerUpCollected;
        public static event Action UltimateActivated;
        public static event Action<Bosses.BossController> BossSpawned;
        public static event Action<Bosses.BossController> BossDefeated;
        public static event Action<string, float> StageMessage;
        public static event Action StageCompleted;
        public static event Action<GameState, GameState> GameStateChanged;
        public static event Action<int> LivesChanged;
        public static event Action<int> WeaponLevelChanged;

        public static void RaiseEnemyDestroyed(in EnemyKilledInfo info) => EnemyDestroyed?.Invoke(info);
        public static void RaisePlayerDamaged(in DamageInfo info, in DamageResult result) => PlayerDamaged?.Invoke(info, result);
        public static void RaisePlayerDied() => PlayerDied?.Invoke();
        public static void RaisePlayerRespawned() => PlayerRespawned?.Invoke();
        public static void RaisePowerUpCollected(PowerUpDefinition def) => PowerUpCollected?.Invoke(def);
        public static void RaiseUltimateActivated() => UltimateActivated?.Invoke();
        public static void RaiseBossSpawned(Bosses.BossController boss) => BossSpawned?.Invoke(boss);
        public static void RaiseBossDefeated(Bosses.BossController boss) => BossDefeated?.Invoke(boss);
        public static void RaiseStageMessage(string message, float duration) => StageMessage?.Invoke(message, duration);
        public static void RaiseStageCompleted() => StageCompleted?.Invoke();
        public static void RaiseGameStateChanged(GameState from, GameState to) => GameStateChanged?.Invoke(from, to);
        public static void RaiseLivesChanged(int lives) => LivesChanged?.Invoke(lives);
        public static void RaiseWeaponLevelChanged(int level) => WeaponLevelChanged?.Invoke(level);

        /// <summary>Drops every subscriber. Used by the Boot scene and by tests.</summary>
        public static void ClearAll()
        {
            EnemyDestroyed = null;
            PlayerDamaged = null;
            PlayerDied = null;
            PlayerRespawned = null;
            PowerUpCollected = null;
            UltimateActivated = null;
            BossSpawned = null;
            BossDefeated = null;
            StageMessage = null;
            StageCompleted = null;
            GameStateChanged = null;
            LivesChanged = null;
            WeaponLevelChanged = null;
        }
    }
}
