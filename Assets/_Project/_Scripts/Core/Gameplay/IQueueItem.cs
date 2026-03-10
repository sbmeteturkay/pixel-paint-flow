using UnityEngine;

namespace PaintFlow.Core.Gameplay
{
    public interface IQueueItem
    {
        ItemType ItemType { get; }
        float OnMove01 { get; }
        GameObject GameObject { get; }
        void SetOnMove01(float onMove01);
        void ReturnToPool();
    }
}
