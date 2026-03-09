using System;
using System.Collections.Generic;
using MessagePipe;
using PaintFlow.Core.Gameplay;
using PaintFlow.Features.QueueLane;
using UnityEngine;
using VContainer;

namespace PaintFlow.Features.ItemRequester
{
    public class ThrownItemBuffer : MonoBehaviour
    {
        private readonly List<IQueueItem> _items = new();
        private IDisposable _subscription;
        private ISubscriber<QueueLaneItemPoppedEvent> _itemPoppedSubscriber;

        public int Count => _items.Count;

        [Inject]
        public void Construct(ISubscriber<QueueLaneItemPoppedEvent> itemPoppedSubscriber)
        {
            _itemPoppedSubscriber = itemPoppedSubscriber;
        }

        private void Start()
        {
            if (_itemPoppedSubscriber != null)
            {
                _subscription = _itemPoppedSubscriber.Subscribe(OnItemPopped);
            }
        }

        private void OnDestroy()
        {
            _subscription?.Dispose();
        }

        public bool TryTake(Predicate<IQueueItem> predicate, out IQueueItem item)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                IQueueItem candidate = _items[i];
                if (!predicate(candidate))
                {
                    continue;
                }

                _items.RemoveAt(i);
                item = candidate;
                return true;
            }

            item = null;
            return false;
        }

        private void OnItemPopped(QueueLaneItemPoppedEvent poppedEvent)
        {
            if (poppedEvent.Item != null)
            {
                _items.Add(poppedEvent.Item);
            }
        }
    }
}
