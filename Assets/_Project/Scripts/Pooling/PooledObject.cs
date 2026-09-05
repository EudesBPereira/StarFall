using UnityEngine;

namespace Starfall.Pooling
{
    /// <summary>
    /// Marker + handle for pooled prefabs. Any object spawned through <see cref="PoolService"/> must have
    /// this component on its root so it can be returned with <see cref="Release"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PooledObject : MonoBehaviour
    {
        internal PoolService.Pool Pool;
        internal IPoolable[] Poolables;
        public bool IsSpawned { get; internal set; }

        /// <summary>Returns the object to its pool (or destroys it when it was not pooled).</summary>
        public void Release()
        {
            if (!IsSpawned) return;
            if (Pool != null)
                Pool.Release(this);
            else
            {
                IsSpawned = false;
                Destroy(gameObject);
            }
        }
    }
}
