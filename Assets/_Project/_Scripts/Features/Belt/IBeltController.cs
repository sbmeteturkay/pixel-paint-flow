namespace PaintFlow.Features.Belt
{
    public interface IBeltController
    {
        bool TryAddBlock(BlockController block);
        void RemoveBlock(BlockController block);
        float SplineLength { get; }
        int CurrentBlockCount { get; }
        int MaxCapacity { get; }
    }
}