using UnityEngine;

namespace PaintFlow.Features.Grid
{
    public class GridCell : MonoBehaviour
    {
        private static readonly int _colorProperty = Shader.PropertyToID("_Color");

        [SerializeField] private Renderer _renderer;
        private Material _material;

        private Color _targetColor;

        public Vector2Int Coordinates { get; private set; }
        public bool IsPainted { get; private set; }

        public void Initialize(Vector2Int coordinates, Color targetColor)
        {
            Coordinates = coordinates;
            _targetColor = targetColor;
            IsPainted = false;

            _material = _renderer.material;
            _material.color = Color.Lerp(targetColor, Color.white, 0.7f);
        }

        public void Paint()
        {
            if (IsPainted) return;
            IsPainted = true;
            _material.color = _targetColor;
        }
    }
}