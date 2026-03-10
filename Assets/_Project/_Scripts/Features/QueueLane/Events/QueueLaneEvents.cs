using PaintFlow.Core.Gameplay;
using UnityEngine;

namespace PaintFlow.Features.QueueLane
{
    public readonly struct QueueLaneItemPoppedEvent
    {
        public readonly int LaneIndex;
        public readonly IQueueItem Item;

        public GameObject GameObject => Item != null ? Item.GameObject : null;

        public QueueLaneItemPoppedEvent(int laneIndex, IQueueItem item)
        {
            LaneIndex = laneIndex;
            Item = item;
        }
    }
}