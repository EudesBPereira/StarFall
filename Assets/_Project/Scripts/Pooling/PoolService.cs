using System.Collections.Generic;
using UnityEngine;

namespace Starfall.Pooling
{
    /// <summary>
    /// Scene-scoped object pool keyed by prefab. Projectiles, enemies, explosions and pickups go through here
    /// so gameplay never instantiates/destroys during play.
    /// </summary>
    public sealed class PoolService : MonoBehaviour
    {
        internal sealed class Pool
        {
            private readonly PooledObject _prefab;
            private readonly Transform _root;
            private readonly Stack<PooledObject> _inactive = new Stack<PooledObject>(32);
            private readonly List<PooledObject> _active = new List<PooledObject>(32);

            public int ActiveCount => _active.Count;

            public Pool(PooledObject prefab, Transform parent)
            {
                _prefab = prefab;
                _root = new GameObject($"Pool_{prefab.name}").transform;
                _root.SetParent(parent, false);
            }

            private PooledObject Create()
            {
                bool wasActive = _prefab.gameObject.activeSelf;
                _prefab.gameObject.SetActive(false);
                var instance = Object.Instantiate(_prefab, _root);
                _prefab.gameObject.SetActive(wasActive);
                instance.name = _prefab.name;
                instance.Pool = this;
                instance.Poolables = instance.GetComponentsInChildren<IPoolable>(true);
                return instance;
            }

            public void Prewarm(int count)
            {
                for (int i = 0; i < count; i++)
                    _inactive.Push(Create());
            }

            public PooledObject Get(Vector3 position, Quaternion rotation)
            {
                var obj = _inactive.Count > 0 ? _inactive.Pop() : Create();
                var t = obj.transform;
                t.SetPositionAndRotation(position, rotation);
                obj.IsSpawned = true;
                _active.Add(obj);
                obj.gameObject.SetActive(true);
                var poolables = obj.Poolables;
                for (int i = 0; i < poolables.Length; i++) poolables[i].OnSpawned();
                return obj;
            }

            public void Release(PooledObject obj)
            {
                if (!obj.IsSpawned) return;
                obj.IsSpawned = false;
                var poolables = obj.Poolables;
                for (int i = 0; i < poolables.Length; i++) poolables[i].OnDespawned();
                obj.gameObject.SetActive(false);
                obj.transform.SetParent(_root, false);
                _active.Remove(obj);
                _inactive.Push(obj);
            }

            public void ReleaseAll()
            {
                for (int i = _active.Count - 1; i >= 0; i--)
                    Release(_active[i]);
            }
        }

        private readonly Dictionary<int, Pool> _pools = new Dictionary<int, Pool>(16);

        public PooledObject Spawn(PooledObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                Debug.LogError("[Starfall] PoolService.Spawn called with a null prefab.");
                return null;
            }
            return GetPool(prefab).Get(position, rotation);
        }

        public PooledObject Spawn(PooledObject prefab, Vector3 position) => Spawn(prefab, position, Quaternion.identity);

        /// <summary>Spawns a prefab and returns the requested component from its root.</summary>
        public T Spawn<T>(PooledObject prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            var obj = Spawn(prefab, position, rotation);
            if (obj == null) return null;
            if (obj.TryGetComponent<T>(out var component)) return component;
            Debug.LogError($"[Starfall] Pooled prefab '{prefab.name}' has no {typeof(T).Name} component.");
            obj.Release();
            return null;
        }

        public void Prewarm(PooledObject prefab, int count)
        {
            if (prefab == null || count <= 0) return;
            GetPool(prefab).Prewarm(count);
        }

        public void ReleaseAll()
        {
            foreach (var pool in _pools.Values) pool.ReleaseAll();
        }

        private Pool GetPool(PooledObject prefab)
        {
            int key = prefab.GetInstanceID();
            if (!_pools.TryGetValue(key, out var pool))
            {
                pool = new Pool(prefab, transform);
                _pools.Add(key, pool);
            }
            return pool;
        }
    }
}
