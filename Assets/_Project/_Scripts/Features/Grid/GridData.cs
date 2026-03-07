using UnityEngine;

namespace PaintFlow.Features.Grid
{
    public class GridData
    {
        private readonly int[] _colorIndices;

        private readonly Color[] _palette;

        public GridData(int width, int height, Color[] palette, int[] colorIndices)
        {
            Width = width;
            Height = height;
            _palette = palette;
            _colorIndices = colorIndices;
        }

        public int Width { get; }
        public int Height { get; }

        public Color GetColor(Vector2Int coords)
        {
            return _palette[_colorIndices[coords.y * Width + coords.x]];
        }

        public int GetColorIndex(Vector2Int coords)
        {
            return _colorIndices[coords.y * Width + coords.x];
        }
    }
}