using System;
using System.Collections.Generic;
using MessagePipe;
using PaintFlow.Core.Gameplay;
using PrimeTween;
using UnityEngine;
using UnityEngine.Splines;
using VContainer;

namespace PaintFlow.Features.QueueLane
{
    public class PoppedItemSplineFlow : MonoBehaviour
    {
        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private float _loopDuration = 2.5f;
        [SerializeField] private int _loseItemCount = 32;

        private readonly List<IQueueItem> _activeItems = new();
        private readonly Dictionary<IQueueItem, Tween> _moveTweens = new();
        private bool _capacityReachedNotified;
        private IPublisher<SplineCapacityReachedEvent> _capacityReachedPublisher;

        private ISubscriber<QueueLaneItemPoppedEvent> _itemPoppedSubscriber;
        private IDisposable _subscription;

        public int ActiveCount => _activeItems.Count;

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

            foreach (Tween tween in _moveTweens.Values)
            {
                if (tween.isAlive)
                {
                    tween.Stop();
                }
            }

            _moveTweens.Clear();
            _activeItems.Clear();
        }

        [Inject]
        public void Construct(
            ISubscriber<QueueLaneItemPoppedEvent> itemPoppedSubscriber,
            IPublisher<SplineCapacityReachedEvent> capacityReachedPublisher)
        {
            _itemPoppedSubscriber = itemPoppedSubscriber;
            _capacityReachedPublisher = capacityReachedPublisher;
        }

        public bool TryTake(Predicate<IQueueItem> predicate, out IQueueItem item)
        {
            for (int i = 0; i < _activeItems.Count; i++)
            {
                IQueueItem candidate = _activeItems[i];
                if (!predicate(candidate))
                {
                    continue;
                }

                _activeItems.RemoveAt(i);

                if (_moveTweens.TryGetValue(candidate, out Tween tween))
                {
                    if (tween.isAlive)
                    {
                        tween.Stop();
                    }

                    _moveTweens.Remove(candidate);
                }

                item = candidate;
                return true;
            }

            item = null;
            return false;
        }

        private void OnItemPopped(QueueLaneItemPoppedEvent poppedEvent)
        {
            if (_splineContainer == null || poppedEvent.Item == null)
            {
                return;
            }

            IQueueItem item = poppedEvent.Item;
            if (_moveTweens.ContainsKey(item))
            {
                return;
            }

            _activeItems.Add(item);
            Transform itemTransform = item.GameObject.transform;

            Sequence.Create()
                .Group(itemTransform.JumpTo((Vector3)_splineContainer.EvaluatePosition(0) + 0.04f * Vector3.up, 2, .4f))
                .Group(Tween.Rotation(itemTransform, (Vector3)_splineContainer.EvaluateTangent(0) + Vector3.up * 90,
                    .4f,
                    Ease.InBack).OnComplete(() =>
                {
                    Tween tween = Tween.Custom(
                        0f,
                        1f,
                        Mathf.Max(0.01f, _loopDuration),
                        normalized => UpdateItemAlongSpline(item, itemTransform, normalized),
                        Ease.Linear,
                        -1);
                    _moveTweens[item] = tween;
                }));

            if (!_capacityReachedNotified && _activeItems.Count >= _loseItemCount)
            {
                _capacityReachedNotified = true;
                _capacityReachedPublisher?.Publish(new(_activeItems.Count));
            }
        }

        private void UpdateItemAlongSpline(IQueueItem item, Transform itemTransform, float normalized)
        {
            Vector3 position = _splineContainer.EvaluatePosition(normalized);
            Vector3 tangent = _splineContainer.EvaluateTangent(normalized);

            itemTransform.position = position;

            if (tangent != Vector3.zero)
            {
                itemTransform.rotation = Quaternion.LookRotation(tangent, Vector3.up) * Quaternion.Euler(0f, 90f, 0f);
            }

            item.SetOnMove01(normalized);
        }
    }
}