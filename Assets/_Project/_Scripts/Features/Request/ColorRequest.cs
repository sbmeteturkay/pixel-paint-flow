using System.Collections.Generic;
using UnityEngine;

namespace PaintFlow.Features.Requester
{
    public struct ColorRequest
    {
        public int ColorIndex;
        public int Amount;
        public List<Vector2Int> TargetPixels;
        private int _pixelIndex;

        public ColorRequest(int colorIndex, int amount, List<Vector2Int> targetPixels)
        {
            ColorIndex = colorIndex;
            Amount = amount;
            TargetPixels = targetPixels;
            _pixelIndex = 0;
        }

        public Vector2Int ConsumeNextPixel()
        {
            return TargetPixels[_pixelIndex++];
        }
    }
}