using System;
using System.Collections.Generic;
using PaintFlow.Core.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace PaintFlow.Features.QueueLane
{
    public class QueueLane : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Transform _itemRoot;
        [SerializeField] private float _sameTypeSpacing = 0.5f;
        [SerializeField] private float _differentTypeSpacing = 1.5f;

        private readonly List<QueueItem> _activeItems = new();
        private readonly Dictionary<QueueItem, ObjectPool<QueueItem>> _ownerPoolByItem = new();
        private readonly Dictionary<ItemType, ObjectPool<QueueItem>> _poolsByType = new();

        private int _laneIndex;
        private Action<QueueLaneItemPoppedEvent> _onItemPopped;

        private void Awake()
        {
            if (_itemRoot == null)
            {
                _itemRoot = transform;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            TryPopFront();
        }

        public void Initialize(
            int laneIndex,
            IReadOnlyList<QueueLaneItemData> laneItems,
            IReadOnlyList<QueueLanePrefabBinding> prefabBindings,
            Action<QueueLaneItemPoppedEvent> onItemPopped)
        {
            _laneIndex = laneIndex;
            _onItemPopped = onItemPopped;

            BuildPools(prefabBindings);
            ClearLane();
            BuildItems(laneItems);
        }

        private void TryPopFront()
        {
            if (_activeItems.Count == 0)
            {
                return;
            }

            QueueItem poppedItem = _activeItems[0];
            QueueItem nextItem = _activeItems.Count > 1 ? _activeItems[1] : null;

            _activeItems.RemoveAt(0);
            poppedItem.transform.SetParent(null, true);

            if (nextItem != null)
            {
                float shiftDistance = GetSpacing(poppedItem.ItemType, nextItem.ItemType);
                _itemRoot.localPosition += Vector3.forward * shiftDistance;
            }

            _onItemPopped?.Invoke(new(_laneIndex, poppedItem));
        }

        private void BuildItems(IReadOnlyList<QueueLaneItemData> laneItems)
        {
            _itemRoot.localPosition = Vector3.zero;

            if (laneItems == null || laneItems.Count == 0)
            {
                return;
            }

            float distanceFromFront = 0f;

            for (int i = 0; i < laneItems.Count; i++)
            {
                QueueLaneItemData itemData = laneItems[i];
                QueueItem queueItem = RentItem(itemData.itemType);
                queueItem.Initialize(itemData, ReturnToPool);
                queueItem.transform.SetParent(_itemRoot, false);
                queueItem.transform.localPosition = Vector3.back * distanceFromFront;
                queueItem.transform.localRotation = Quaternion.identity;
                _activeItems.Add(queueItem);

                if (i < laneItems.Count - 1)
                {
                    QueueLaneItemData nextData = laneItems[i + 1];
                    distanceFromFront += GetSpacing(itemData.itemType, nextData.itemType);
                }
            }
        }

        private void ClearLane()
        {
            for (int i = 0; i < _activeItems.Count; i++)
            {
                ReturnToPool(_activeItems[i]);
            }

            _activeItems.Clear();
            _itemRoot.localPosition = Vector3.zero;
        }

        private float GetSpacing(ItemType currentType, ItemType nextType)
        {
            return currentType == nextType ? _sameTypeSpacing : _differentTypeSpacing;
        }

        private QueueItem RentItem(ItemType itemType)
        {
            if (!_poolsByType.TryGetValue(itemType, out ObjectPool<QueueItem> pool))
            {
                throw new InvalidOperationException($"QueueLane missing prefab binding for itemType '{itemType}'.");
            }

            QueueItem item = pool.Get();
            _ownerPoolByItem[item] = pool;
            return item;
        }

        private void ReturnToPool(QueueItem item)
        {
            if (item == null)
            {
                return;
            }

            if (_ownerPoolByItem.TryGetValue(item, out ObjectPool<QueueItem> pool))
            {
                _ownerPoolByItem.Remove(item);
                pool.Release(item);
            }
        }

        private void BuildPools(IReadOnlyList<QueueLanePrefabBinding> prefabBindings)
        {
            _poolsByType.Clear();

            if (prefabBindings == null)
            {
                return;
            }

            foreach (QueueLanePrefabBinding binding in prefabBindings)
            {
                if (binding == null || binding.prefab == null)
                {
                    continue;
                }

                if (_poolsByType.ContainsKey(binding.itemType))
                {
                    continue;
                }

                QueueLanePrefabBinding localBinding = binding;

                ObjectPool<QueueItem> pool = new(
                    () => Instantiate(localBinding.prefab, _itemRoot),
                    item => { item.gameObject.SetActive(true); },
                    item =>
                    {
                        item.transform.SetParent(_itemRoot, false);
                        item.gameObject.SetActive(false);
                    },
                    item =>
                    {
                        if (item != null)
                        {
                            Destroy(item.gameObject);
                        }
                    },
                    false,
                    Mathf.Max(1, localBinding.defaultCapacity),
                    Mathf.Max(1, localBinding.maxSize));

                _poolsByType.Add(localBinding.itemType, pool);
            }
        }
    }
}