using System;
using PaintFlow.Core.Gameplay;

namespace PaintFlow.Features.QueueLane
{
    [Serializable]
    public class QueueLanePrefabBinding
    {
        public ItemType itemType;
        public QueueItem prefab;
        public int defaultCapacity = 8;
        public int maxSize = 32;
    }
}