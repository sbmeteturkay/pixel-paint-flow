using System;
using System.Collections.Generic;
using MessagePipe;
using PaintFlow.Shared.EventSystem.Events;
using UnityEngine;
using VContainer;

namespace PaintFlow.Features.Grid
{
    public class GridController : MonoBehaviour
    {
        [SerializeField] private GridCell _cellPrefab;
        [SerializeField] private float _cellSize = 1f;

        private readonly Dictionary<Vector2Int, GridCell> _cells = new();
        private readonly List<GridCell> _pool = new();

        private ISubscriber<BlockMatchedEvent> _blockMatchedSubscriber;
        private IDisposable _disposable;

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }

        [Inject]
        public void Construct(ISubscriber<BlockMatchedEvent> blockMatchedSubscriber)
        {
            _blockMatchedSubscriber = blockMatchedSubscriber;
        }

        public void Initialize(GridData gridData)
        {
            _disposable?.Dispose();
            _disposable = _blockMatchedSubscriber.Subscribe(OnBlockMatched);

            BuildGrid(gridData);
        }

        public void Cleanup()
        {
            foreach (GridCell cell in _cells.Values)
            {
                cell.gameObject.SetActive(false);
                _pool.Add(cell);
            }

            _cells.Clear();
        }

        // ─── Private ─────────────────────────────────────────────
        private void BuildGrid(GridData gridData)
        {
            Cleanup();

            for (int y = 0; y < gridData.Height; y++)
            {
                for (int x = 0; x < gridData.Width; x++)
                {
                    Vector2Int coordinates = new(x, y);
                    Color color = gridData.GetColor(coordinates);
                    Vector3 position = new(x * _cellSize, y * _cellSize, 0f);

                    GridCell cell = GetOrCreateCell();
                    cell.transform.localPosition = position;
                    cell.gameObject.SetActive(true);
                    cell.Initialize(coordinates, color);
                    _cells[coordinates] = cell;
                }
            }
        }

        private GridCell GetOrCreateCell()
        {
            if (_pool.Count > 0)
            {
                GridCell pooled = _pool[_pool.Count - 1];
                _pool.RemoveAt(_pool.Count - 1);
                return pooled;
            }

            return Instantiate(_cellPrefab, transform);
        }

        private void OnBlockMatched(BlockMatchedEvent e)
        {
            if (_cells.TryGetValue(e.TargetPixel, out GridCell cell))
                cell.Paint();
        }
    }
}