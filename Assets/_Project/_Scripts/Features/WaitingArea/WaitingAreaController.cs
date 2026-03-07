using System.Collections.Generic;
using MessagePipe;
using PaintFlow.Features.Belt;
using PaintFlow.Features.EventSystem.Events;
using UnityEngine;
using VContainer;
using Random = System.Random;

namespace PaintFlow.Features.WaitingArea
{
    public class WaitingAreaController : MonoBehaviour
    {
        [SerializeField] private Color[] _palette;

        [SerializeField] private WaitingAreaColumn _columnPrefab;
        [SerializeField] private float _columnSpacing = 1.5f;

        private readonly List<WaitingAreaColumn> _columns = new();
        private IBeltController _beltController;

        private IPublisher<BlockSentToBeltEvent> _blockSentPublisher;
        private BlockSpawner _blockSpawner;
        private int _remainingBlocks;
        private IPublisher<WaitingAreaEmptyEvent> _waitingAreaEmptyPublisher;

        [Inject]
        public void Construct(
            IPublisher<BlockSentToBeltEvent> blockSentPublisher,
            IPublisher<WaitingAreaEmptyEvent> waitingAreaEmptyPublisher,
            IBeltController beltController,
            BlockSpawner blockSpawner)
        {
            _blockSentPublisher = blockSentPublisher;
            _waitingAreaEmptyPublisher = waitingAreaEmptyPublisher;
            _beltController = beltController;
            _blockSpawner = blockSpawner;
        }

        public void Initialize(WaitingAreaData data)
        {
            Cleanup();

            Queue<int> colorQueue = BuildShuffledQueue(data);
            _remainingBlocks = colorQueue.Count;

            for (int i = 0; i < data.ColumnCount; i++)
            {
                Vector3 position = new(i * _columnSpacing, 0f, 0f);
                WaitingAreaColumn column = Instantiate(_columnPrefab, position, Quaternion.identity, transform);
                column.Initialize(_beltController, _blockSpawner, OnBlockSent);
                _columns.Add(column);
            }

            DistributeBlocks(colorQueue, data.ColumnCount);
        }

        // ─── Private ─────────────────────────────────────────────
        private Queue<int> BuildShuffledQueue(WaitingAreaData data)
        {
            List<int> list = new(data.ColorIndices);
            Random rng = new(data.ShuffleSeed);

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            return new(list);
        }

        private void DistributeBlocks(Queue<int> colorQueue, int columnCount)
        {
            int[] slotCounters = new int[columnCount];
            int columnIndex = 0;

            while (colorQueue.Count > 0)
            {
                int colorIndex = colorQueue.Dequeue();
                BlockController block = _blockSpawner.GetFromPool();
                Color color = _palette[colorIndex];
                block.SetColor(colorIndex);
                block.GetComponent<Renderer>().material.color = color;
                block.SetTapCallback(OnBlockSent);

                _columns[columnIndex].Enqueue(block, slotCounters[columnIndex]);
                slotCounters[columnIndex]++;
                columnIndex = (columnIndex + 1) % columnCount;
            }
        }

        private void OnBlockSent(BlockController blockView)
        {
            _blockSentPublisher.Publish(new(blockView.ColorIndex));

            _remainingBlocks--;
            if (_remainingBlocks <= 0)
                _waitingAreaEmptyPublisher.Publish(new());
        }

        private void Cleanup()
        {
            foreach (WaitingAreaColumn column in _columns)
            {
                Destroy(column.gameObject);
            }

            _columns.Clear();
        }
    }
}