// Features/Requester/Scripts/RequesterController.cs

using System;
using System.Collections.Generic;
using MessagePipe;
using PaintFlow.Core.EventSystem.Events;
using PaintFlow.Features.Belt;
using UnityEngine;
using VContainer;

namespace PaintFlow.Features.Requester
{
    public class RequesterController : MonoBehaviour, IRequester
    {
        private readonly List<BlockController> _candidates = new();
        private IPublisher<AllRequestsCompletedEvent> _allRequestsCompletedPublisher;
        private IPublisher<BlockMatchedEvent> _blockMatchedPublisher;
        private ISubscriber<BlockPositionUpdatedEvent> _blockPositionSubscriber;
        private int _currentRequestIndex;
        private IDisposable _disposable;
        private IPublisher<RequestCompletedEvent> _requestCompletedPublisher;

        private List<ColorRequest> _requests;

        private void OnDestroy()
        {
            _disposable.Dispose();
        }

        public int Id { get; private set; }
        public float PositionT { get; private set; }
        public ColorRequest CurrentRequest => _requests[_currentRequestIndex];
        public bool IsCompleted { get; private set; }

        // ─── IRequester ──────────────────────────────────────────
        public void Initialize(int id, float positionT, List<ColorRequest> requests)
        {
            Id = id;
            PositionT = positionT;
            _requests = requests;
            _currentRequestIndex = 0;
            IsCompleted = false;
            _candidates.Clear();

            _disposable = _blockPositionSubscriber
                .Subscribe(OnBlockPositionUpdated);
        }

        [Inject]
        public void Construct(
            IPublisher<BlockMatchedEvent> blockMatchedPublisher,
            IPublisher<RequestCompletedEvent> requestCompletedPublisher,
            IPublisher<AllRequestsCompletedEvent> allRequestsCompletedPublisher,
            ISubscriber<BlockPositionUpdatedEvent> blockPositionSubscriber)
        {
            _blockMatchedPublisher = blockMatchedPublisher;
            _requestCompletedPublisher = requestCompletedPublisher;
            _allRequestsCompletedPublisher = allRequestsCompletedPublisher;
            _blockPositionSubscriber = blockPositionSubscriber;
        }

        // ─── Private ─────────────────────────────────────────────
        private void OnBlockPositionUpdated(BlockPositionUpdatedEvent e)
        {
            if (IsCompleted) return;

            if (e.Block.T < PositionT)
                AddCandidate(e.Block);
            else
                CheckCandidate(e.Block);
        }

        private void AddCandidate(BlockController block)
        {
            if (block.ColorIndex != CurrentRequest.ColorIndex) return;
            if (_candidates.Contains(block)) return;

            _candidates.Add(block);
        }

        private void CheckCandidate(BlockController block)
        {
            if (!_candidates.Contains(block)) return;

            _candidates.Remove(block);
            HandleMatch(block);
        }

        private void HandleMatch(BlockController block)
        {
            block.JumpToTarget(transform.position);

            ColorRequest current = _requests[_currentRequestIndex];
            current.Amount--;
            _requests[_currentRequestIndex] = current;

            _blockMatchedPublisher.Publish(new(Id, current.ColorIndex));

            if (current.Amount <= 0)
            {
                _requestCompletedPublisher.Publish(new(Id));
                AdvanceRequest();
            }
        }

        private void AdvanceRequest()
        {
            _currentRequestIndex++;
            _candidates.Clear();

            if (_currentRequestIndex >= _requests.Count)
            {
                IsCompleted = true;
                _allRequestsCompletedPublisher.Publish(new(Id));
            }
        }
    }
}