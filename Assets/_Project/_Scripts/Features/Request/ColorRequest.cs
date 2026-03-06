namespace PaintFlow.Features.Requester
{
    public struct ColorRequest
    {
        public int ColorIndex;
        public int Amount;

        public ColorRequest(int colorIndex, int amount)
        {
            ColorIndex = colorIndex;
            Amount = amount;
        }
    }
}