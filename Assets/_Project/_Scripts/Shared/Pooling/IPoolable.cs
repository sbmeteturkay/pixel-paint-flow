namespace PaintFlow.Shared
{
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }
}