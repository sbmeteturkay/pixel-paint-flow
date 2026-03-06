using System.Collections.Generic;
using MessagePipe;
using PaintFlow.Core.EventSystem.Events;
using PaintFlow.Features.Requester;
using UnityEngine;
using UnityEngine.Splines;
using VContainer;

namespace PaintFlow.Features.Belt
{
    public class BeltController : MonoBehaviour, IBeltController
    {
        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private BeltDataSO _beltData;

        private readonly List<BlockController> _activeBlocks = new();

        private IPublisher<BeltFullEvent> _beltFullPublisher;
        private IPublisher<BlockAddedToBeltEvent> _blockAddedPublisher;
        private IPublisher<BlockPositionUpdatedEvent> _blockPositionPublisher;
        private IPublisher<BlockRemovedFromBeltEvent> _blockRemovedPublisher;

        private List<IRequester> _requesters;

        private void Awake()
        {
            SplineLength = _splineContainer.CalculateLength();
        }

        private void Update()
        {
            foreach (BlockController block in _activeBlocks)
                // Her aktif blok için
            {
                _blockPositionPublisher.Publish(new(block, block.T));
            }
        }

        public float SplineLength { get; private set; }
        public int CurrentBlockCount => _activeBlocks.Count;

        public int MaxCapacity => _beltData.MaxCapacity;

        // ─── IBeltController ─────────────────────────────────────
        public bool TryAddBlock(BlockController block)
        {
            if (_activeBlocks.Count >= _beltData.MaxCapacity)
            {
                _beltFullPublisher.Publish(new());
                return false;
            }

            _activeBlocks.Add(block);
            block.OnJumpComplete += HandleJumpComplete;
            block.GetComponent<BeltMover>().Initialize(_splineContainer, _beltData.BeltSpeed, SplineLength);
            block.EnterBelt();

            _blockAddedPublisher.Publish(new(_activeBlocks.Count, _beltData.MaxCapacity));
            return true;
        }

        public void RemoveBlock(BlockController block)
        {
            if (!_activeBlocks.Contains(block)) return;

            _activeBlocks.Remove(block);
            block.OnJumpComplete -= HandleJumpComplete;

            _blockRemovedPublisher.Publish(new(_activeBlocks.Count, _beltData.MaxCapacity));
        }

        public void RegisterRequester(IRequester requester)
        {
            _requesters.Add(requester);
        }

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

        // ─── Private ─────────────────────────────────────────────
        private void HandleJumpComplete(BlockController block)
        {
            RemoveBlock(block);
        }
    }
}