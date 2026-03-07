using System.Collections.Generic;

namespace PaintFlow.Features.WaitingArea
{
    public class WaitingAreaData
    {
        public WaitingAreaData(int columnCount, int shuffleSeed, List<int> colorIndices)
        {
            ColumnCount = columnCount;
            ShuffleSeed = shuffleSeed;
            ColorIndices = colorIndices;
        }

        public int ColumnCount { get; }
        public int ShuffleSeed { get; }
        public List<int> ColorIndices { get; }
    }
}