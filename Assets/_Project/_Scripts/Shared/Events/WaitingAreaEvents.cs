namespace PaintFlow.Features.EventSystem.Events
{
    public readonly struct BlockSentToBeltEvent
    {
        public readonly int ColorIndex;

        public BlockSentToBeltEvent(int colorIndex)
        {
            ColorIndex = colorIndex;
        }
    }

    public readonly struct WaitingAreaEmptyEvent
    {
    }
}