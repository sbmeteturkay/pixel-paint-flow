using PaintFlow.Core.Gameplay;
using UnityEngine;

namespace PaintFlow.Features.QueueLane
{
    public readonly struct SplineCapacityReachedEvent
    {
        public readonly int ActiveItemCount;

        public SplineCapacityReachedEvent(int activeItemCount)
        {
            ActiveItemCount = activeItemCount;
        }
    }
}
