using System;
using PaintFlow.Core.Gameplay;
using UnityEngine;

namespace PaintFlow.Features.QueueLane
{
    public class QueueItem : MonoBehaviour, IQueueItem
    {
        [SerializeField] private ItemType _itemType;
        [SerializeField, Range(0f, 1f)] private float _onMove01;

        private Action<QueueItem> _returnToPool;

        public ItemType ItemType => _itemType;
        public float OnMove01 => _onMove01;
        public GameObject GameObject => gameObject;

        public void Initialize(QueueLaneItemData itemData, Action<QueueItem> returnToPool)
        {
            _itemType = itemData.itemType;
            _returnToPool = returnToPool;
            _onMove01 = 0f;
        }

        public void SetOnMove01(float onMove01)
        {
            _onMove01 = Mathf.Clamp01(onMove01);
        }

        public void ReturnToPool()
        {
            _returnToPool?.Invoke(this);
        }
    }
}
