using UnityEngine;

namespace PaintFlow.Core.Gameplay
{
    public interface IQueueItem
    {
        ItemType ItemType { get; }
        float OnMove01 { get; }
        GameObject GameObject { get; }
        void ReturnToPool();
    }
}
