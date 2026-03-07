using UnityEngine;

namespace PaintFlow.Shared.EventSystem.Events
{
    public readonly struct RequestCompletedEvent
    {
        public readonly int RequesterId;

        public RequestCompletedEvent(int requesterId)
        {
            RequesterId = requesterId;
        }
    }

    public readonly struct AllRequestsCompletedEvent
    {
        public readonly int RequesterId;

        public AllRequestsCompletedEvent(int requesterId)
        {
            RequesterId = requesterId;
        }
    }

    public readonly struct BlockMatchedEvent
    {
        public readonly int RequesterId;
        public readonly int ColorIndex;
        public readonly Vector2Int TargetPixel;

        public BlockMatchedEvent(int requesterId, int colorIndex, Vector2Int targetPixel)
        {
            RequesterId = requesterId;
            ColorIndex = colorIndex;
            TargetPixel = targetPixel;
        }
    }
}