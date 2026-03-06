using System.Collections.Generic;
using MessagePipe;
using UnityEngine;
using VContainer;
using PaintFlow.Core.EventSystem.Events;
using UnityEngine.Splines;

namespace PaintFlow.Features.Belt
{
    public class BeltController : MonoBehaviour, IBeltController
    {
        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private BeltDataSO _beltData;

        private IPublisher<BeltFullEvent> _beltFullPublisher;
        private IPublisher<BlockAddedToBeltEvent> _blockAddedPublisher;
        private IPublisher<BlockRemovedFromBeltEvent> _blockRemovedPublisher;

        private readonly List<BlockController> _activeBlocks = new();

        public float SplineLength { get; private set; }
        public int CurrentBlockCount => _activeBlocks.Count;
        public int MaxCapacity => _beltData.MaxCapacity;

        [Inject]
        public void Construct(
            IPublisher<BeltFullEvent> beltFullPublisher,
            IPublisher<BlockAddedToBeltEvent> blockAddedPublisher,
            IPublisher<BlockRemovedFromBeltEvent> blockRemovedPublisher)
        {
            _beltFullPublisher = beltFullPublisher;
            _blockAddedPublisher = blockAddedPublisher;
            _blockRemovedPublisher = blockRemovedPublisher;
        }

        private void Awake()
        {
            SplineLength = _splineContainer.CalculateLength();
        }

        // ─── IBeltController ─────────────────────────────────────
        public bool TryAddBlock(BlockController block)
        {
            if (_activeBlocks.Count >= _beltData.MaxCapacity)
            {
                _beltFullPublisher.Publish(new BeltFullEvent());
                return false;
            }

            _activeBlocks.Add(block);
            block.OnJumpComplete += HandleJumpComplete;
            block.GetComponent<BeltMover>().Initialize(_splineContainer, _beltData.BeltSpeed, SplineLength);
            block.EnterBelt();

            _blockAddedPublisher.Publish(new BlockAddedToBeltEvent(_activeBlocks.Count, _beltData.MaxCapacity));
            return true;
        }

        public void RemoveBlock(BlockController block)
        {
            if (!_activeBlocks.Contains(block)) return;

            _activeBlocks.Remove(block);
            block.OnJumpComplete -= HandleJumpComplete;

            _blockRemovedPublisher.Publish(new BlockRemovedFromBeltEvent(_activeBlocks.Count, _beltData.MaxCapacity));
        }

        // ─── Private ─────────────────────────────────────────────
        private void HandleJumpComplete(BlockController block)
        {
            RemoveBlock(block);
        }
    }
}