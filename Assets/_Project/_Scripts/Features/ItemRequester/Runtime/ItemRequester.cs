using System.Collections.Generic;
using PaintFlow.Core.Gameplay;
using PaintFlow.Features.Level;
using UnityEngine;

namespace PaintFlow.Features.ItemRequester
{
    public class ItemRequester : MonoBehaviour
    {
        private readonly Queue<ItemRequestRuntime> _pendingRequests = new();

        private float _minMove01;
        private float _maxMove01;
        private ItemRequestRuntime _activeRequest;

        public bool HasActiveRequest => _activeRequest != null;

        public void Initialize(RequesterDefinition definition)
        {
            _pendingRequests.Clear();
            _activeRequest = null;

            if (definition == null)
            {
                return;
            }

            _minMove01 = Mathf.Min(definition.minMove01, definition.maxMove01);
            _maxMove01 = Mathf.Max(definition.minMove01, definition.maxMove01);

            if (definition.requests != null)
            {
                for (int i = 0; i < definition.requests.Count; i++)
                {
                    ItemRequestDefinition request = definition.requests[i];
                    int clampedCount = ClampToAllowedRequestCount(request.count);
                    _pendingRequests.Enqueue(new ItemRequestRuntime(request.itemType, clampedCount));
                }
            }

            AdvanceRequest();
        }

        public bool TryConsume(ThrownItemBuffer buffer)
        {
            if (buffer == null || _activeRequest == null)
            {
                return false;
            }

            bool consumedAny = false;
            bool canContinue = true;

            while (canContinue && _activeRequest != null)
            {
                if (buffer.TryTake(IsMatch, out IQueueItem matchedItem))
                {
                    matchedItem.ReturnToPool();
                    _activeRequest.Remaining--;
                    consumedAny = true;

                    if (_activeRequest.Remaining <= 0)
                    {
                        AdvanceRequest();
                    }
                    else
                    {
                        canContinue = false;
                    }
                }
                else
                {
                    canContinue = false;
                }
            }

            return consumedAny;
        }

        private bool IsMatch(IQueueItem item)
        {
            if (_activeRequest == null || item == null)
            {
                return false;
            }

            return item.ItemType == _activeRequest.ItemType
                && item.OnMove01 >= _minMove01
                && item.OnMove01 <= _maxMove01;
        }

        private void AdvanceRequest()
        {
            _activeRequest = _pendingRequests.Count > 0 ? _pendingRequests.Dequeue() : null;
        }

        private static int ClampToAllowedRequestCount(int value)
        {
            if (value <= 3)
            {
                return 2;
            }

            if (value <= 5)
            {
                return 4;
            }

            return 6;
        }

        private sealed class ItemRequestRuntime
        {
            public readonly ItemType ItemType;
            public int Remaining;

            public ItemRequestRuntime(ItemType itemType, int remaining)
            {
                ItemType = itemType;
                Remaining = remaining;
            }
        }
    }
}
