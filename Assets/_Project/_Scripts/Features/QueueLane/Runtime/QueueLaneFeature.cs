using System.Collections.Generic;
using MessagePipe;
using PaintFlow.Features.Level;
using UnityEngine;
using VContainer;

namespace PaintFlow.Features.QueueLane
{
    public class QueueLaneFeature : MonoBehaviour
    {
        [SerializeField] private QueueLane _lanePrefab;
        [SerializeField] private Transform _laneRoot;
        [SerializeField] private float _laneSpacing = 2.5f;
        [SerializeField] private bool _buildOnStart = true;

        private readonly List<QueueLane> _runtimeLanes = new();
        private IPublisher<QueueLaneItemPoppedEvent> _itemPoppedPublisher;
        private LevelLoader _levelLoader;

        private void Start()
        {
            if (_buildOnStart)
            {
                BuildCurrentLevel();
            }
        }

        [Inject]
        public void Construct(
            IPublisher<QueueLaneItemPoppedEvent> itemPoppedPublisher,
            LevelLoader levelLoader)
        {
            _itemPoppedPublisher = itemPoppedPublisher;
            _levelLoader = levelLoader;
        }

        public void BuildCurrentLevel()
        {
            Build(_levelLoader != null ? _levelLoader.CurrentLevel : null);
        }

        public void Build(LevelData levelData)
        {
            ClearLanes();

            if (levelData == null || _lanePrefab == null || levelData.lanes == null || levelData.lanes.Count == 0)
            {
                return;
            }

            List<QueueLanePrefabBinding> bindings = ConvertBindings(levelData.itemPrefabs);
            Transform parent = _laneRoot != null ? _laneRoot : transform;
            float levelDataLaneCount = _laneSpacing / 2 * (levelData.lanes.Count - 1);

            for (int i = 0; i < levelData.lanes.Count; i++)
            {
                QueueLane lane = Instantiate(_lanePrefab, parent);
                lane.transform.localPosition =
                    new(i * _laneSpacing - levelDataLaneCount, 0f, 0f);
                lane.transform.localRotation = Quaternion.identity;
                lane.Initialize(i, levelData.lanes[i].items, bindings, PublishPoppedItem);
                _runtimeLanes.Add(lane);
            }
        }

        private List<QueueLanePrefabBinding> ConvertBindings(List<ItemPrefabBinding> levelBindings)
        {
            List<QueueLanePrefabBinding> bindings = new();

            if (levelBindings == null)
            {
                return bindings;
            }

            for (int i = 0; i < levelBindings.Count; i++)
            {
                ItemPrefabBinding source = levelBindings[i];
                if (source == null || source.prefab == null)
                {
                    continue;
                }

                bindings.Add(new()
                {
                    itemType = source.itemType,
                    prefab = source.prefab,
                    defaultCapacity = source.defaultCapacity,
                    maxSize = source.maxSize
                });
            }

            return bindings;
        }

        private void PublishPoppedItem(QueueLaneItemPoppedEvent poppedEvent)
        {
            _itemPoppedPublisher?.Publish(poppedEvent);
        }

        private void ClearLanes()
        {
            for (int i = 0; i < _runtimeLanes.Count; i++)
            {
                if (_runtimeLanes[i] != null)
                {
                    Destroy(_runtimeLanes[i].gameObject);
                }
            }

            _runtimeLanes.Clear();
        }
    }
}