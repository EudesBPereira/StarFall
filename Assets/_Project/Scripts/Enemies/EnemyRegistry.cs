using System;
using System.Collections.Generic;
using Starfall.Logic;
using UnityEngine;

namespace Starfall.Enemies
{
    /// <summary>Tracks live enemies so the wave director and the Ultimate never search the scene.</summary>
    public sealed class EnemyRegistry : MonoBehaviour
    {
        private readonly List<Enemy> _active = new List<Enemy>(64);

        public event Action Changed;

        public int Count => _active.Count;

        /// <summary>Enemies that block wave completion (everything except obstacles).</summary>
        public int BlockingCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < _active.Count; i++)
                {
                    var e = _active[i];
                    if (e != null && e.Definition != null && !e.Definition.IsObstacle) n++;
                }
                return n;
            }
        }

        public int BossCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < _active.Count; i++)
                    if (_active[i] != null && _active[i].IsBoss) n++;
                return n;
            }
        }

        public void Register(Enemy enemy)
        {
            if (enemy == null || _active.Contains(enemy)) return;
            _active.Add(enemy);
            Changed?.Invoke();
        }

        public void Unregister(Enemy enemy)
        {
            if (_active.Remove(enemy)) Changed?.Invoke();
        }

        public void CopyTo(List<Enemy> buffer)
        {
            buffer.Clear();
            buffer.AddRange(_active);
        }

        /// <summary>Removes every enemy without rewards (restart, return to menu).</summary>
        public void DespawnAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var e = _active[i];
                if (e != null) e.Despawn();
            }
            _active.Clear();
            Changed?.Invoke();
        }

        /// <summary>Kills every non-boss enemy with the given source (e.g. stage clear).</summary>
        public void KillAllCommon(DamageSource source)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var e = _active[i];
                if (e != null && !e.IsBoss && e.Health != null) e.Health.Kill(source);
            }
        }
    }
}
