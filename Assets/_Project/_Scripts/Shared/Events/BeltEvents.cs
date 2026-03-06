using PaintFlow.Features.Belt;

namespace PaintFlow.Core.EventSystem.Events
{
    public readonly struct BeltFullEvent
    {
    }

    public readonly struct BlockAddedToBeltEvent
    {
        public readonly int CurrentCount;
        public readonly int MaxCapacity;

        public BlockAddedToBeltEvent(int currentCount, int maxCapacity)
        {
            CurrentCount = currentCount;
            MaxCapacity = maxCapacity;
        }
    }

    public readonly struct BlockRemovedFromBeltEvent
    {
        public readonly int CurrentCount;
        public readonly int MaxCapacity;

        public BlockRemovedFromBeltEvent(int currentCount, int maxCapacity)
        {
            CurrentCount = currentCount;
            MaxCapacity = maxCapacity;
        }
    }

    public readonly struct BlockPositionUpdatedEvent
    {
        public readonly BlockController Block;
        public readonly float T;

        public BlockPositionUpdatedEvent(BlockController block, float t)
        {
            Block = block;
            T = t;
        }
    }
}