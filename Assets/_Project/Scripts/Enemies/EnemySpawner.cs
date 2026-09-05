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

        public PooledObject EnemyProjectilePrefab => enemyProjectilePrefab;

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
            enemy.Initialize(definition, ctx, this);
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
