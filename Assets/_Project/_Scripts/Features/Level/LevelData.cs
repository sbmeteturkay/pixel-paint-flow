using System;
using System.Collections.Generic;
using PaintFlow.Core.Gameplay;
using PaintFlow.Features.QueueLane;
using UnityEngine;

namespace PaintFlow.Features.Level
{
    [CreateAssetMenu(fileName = "SO_LevelData", menuName = "PaintFlow/Level/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Identity")]
        public int levelId;
        public int seed = 1;

        [Header("Defaults")]
        [Min(1)] public int laneCount = 2;
        [Min(1)] public int requesterCount = 2;

        [Header("Gameplay Data")]
        public List<LaneDefinition> lanes = new();
        public List<RequesterDefinition> requesters = new();
        public List<ItemPrefabBinding> itemPrefabs = new();

        [Header("Generation")]
        [Min(1)] public int totalBlockCount = 20;
        [Range(0.01f, 0.49f)] public float requesterWindowSize = 0.04f;

        [ContextMenu("Regenerate From Seed")]
        public void RegenerateFromSeed()
        {
            LevelDataGenerator.GenerateInPlace(this);
        }
    }

    [Serializable]
    public class LaneDefinition
    {
        public List<QueueLaneItemData> items = new();
    }

    [Serializable]
    public class RequesterDefinition
    {
        [Range(0f, 1f)] public float minMove01;
        [Range(0f, 1f)] public float maxMove01;
        public List<ItemRequestDefinition> requests = new();
    }

    [Serializable]
    public class ItemRequestDefinition
    {
        public ItemType itemType;
        [Min(2)] public int count = 2;
    }

    [Serializable]
    public class ItemPrefabBinding
    {
        public ItemType itemType;
        public QueueItem prefab;
        [Min(1)] public int defaultCapacity = 8;
        [Min(1)] public int maxSize = 32;
    }
}
