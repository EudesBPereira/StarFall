namespace Starfall.Pooling
{
    /// <summary>Optional hooks for pooled components. Called by <see cref="PoolService"/>.</summary>
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}
