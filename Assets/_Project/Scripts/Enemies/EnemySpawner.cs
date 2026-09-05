using Starfall.Core;
using Starfall.Pooling;
using Starfall.PowerUps;
using UnityEngine;

namespace Starfall.Enemies
{
    /// <summary>Instantiates enemies and pickups through the pool and wires them to the gameplay context.</summary>
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] internal PooledObject defaultEnemyPrefab;
        [SerializeField] internal PooledObject enemyProjectilePrefab;
        [SerializeField] internal PooledObject defaultPickupPrefab;
        [SerializeField] internal Sprite webSprite;

        /// <summary>Hull/damage multiplier for endless modes (set by the director per wave).</summary>
        public float StatMultiplier { get; set; } = 1f;
        /// <summary>Speed multiplier (daily modifiers).</summary>
        public float SpeedMultiplier { get; set; } = 1f;
        /// <summary>Drop chance multiplier (daily modifiers).</summary>
        public float DropChanceMultiplier { get; set; } = 1f;

        public PooledObject EnemyProjectilePrefab => enemyProjectilePrefab;
        public Sprite WebSprite => webSprite;

        public Enemy Spawn(EnemyDefinition definition, Vector2 position)
        {
            var ctx = GameplayContext.Current;
            if (definition == null || ctx == null) return null;
            var prefab = definition.Prefab != null ? definition.Prefab : defaultEnemyPrefab;
            if (prefab == null)
            {
                Debug.LogError($"[Starfall] No prefab for enemy '{definition.name}'.");
                return null;
            }
            var enemy = ctx.Pools.Spawn<Enemy>(prefab, position, Quaternion.identity);
            if (enemy == null) return null;
            enemy.Initialize(definition, ctx, this, definition.IsBoss ? 1f + (StatMultiplier - 1f) * 0.5f : StatMultiplier, SpeedMultiplier);
            return enemy;
        }

        public PowerUpPickup SpawnPickup(PowerUpDefinition definition, Vector2 position)
        {
            var ctx = GameplayContext.Current;
            if (definition == null || ctx == null) return null;
            var prefab = definition.PickupPrefab != null ? definition.PickupPrefab : defaultPickupPrefab;
            if (prefab == null) return null;
            var pickup = ctx.Pools.Spawn<PowerUpPickup>(prefab, position, Quaternion.identity);
            if (pickup == null) return null;
            pickup.Initialize(definition, ctx.PlayArea);
            return pickup;
        }
    }
}
