using System;
using System.Collections.Generic;
using System.Linq;
using PaintFlow.Core.Gameplay;
using UnityEngine;
using Random = System.Random;

namespace PaintFlow.Features.Level
{
    public static class LevelDataGenerator
    {
        private static readonly int[] AllowedCounts = { 2, 4, 6 };

        public static void GenerateInPlace(LevelData levelData)
        {
            if (levelData == null)
            {
                return;
            }

            int laneCount = Mathf.Max(1, levelData.laneCount);
            int requesterCount = Mathf.Max(1, levelData.requesterCount);
            int totalBlockCount = Mathf.Max(requesterCount, levelData.totalBlockCount);
            float halfWindow = Mathf.Clamp(levelData.requesterWindowSize * 0.5f, 0.005f, 0.245f);

            List<ItemType> allowedTypes = GetAllowedItemTypes(levelData);
            if (allowedTypes.Count == 0)
            {
                return;
            }

            Random random = new(levelData.seed);
            List<Block> blocks = CreateBlocks(totalBlockCount, allowedTypes, random);

            List<LaneDefinition> lanes = CreateLanes(laneCount);
            for (int i = 0; i < blocks.Count; i++)
            {
                LaneDefinition lane = lanes[i % laneCount];
                AddBlockToLane(lane, blocks[i]);
            }

            List<Block> requestBlocks = new(blocks);
            Shuffle(requestBlocks, random);

            if (HasSameOrder(blocks, requestBlocks))
            {
                Shuffle(requestBlocks, random);
            }

            List<RequesterDefinition> requesters = CreateRequesters(requesterCount, halfWindow);
            for (int i = 0; i < requestBlocks.Count; i++)
            {
                Block block = requestBlocks[i];
                RequesterDefinition requester = requesters[i % requesterCount];
                requester.requests.Add(new()
                {
                    itemType = block.ItemType,
                    icon = block.Icon,
                    count = block.Count
                });
            }

            levelData.lanes = lanes;
            levelData.requesters = requesters;
            levelData.laneCount = laneCount;
            levelData.requesterCount = requesterCount;
        }

        private static List<ItemType> GetAllowedItemTypes(LevelData levelData)
        {
            if (levelData.itemPrefabs != null && levelData.itemPrefabs.Count > 0)
            {
                return levelData.itemPrefabs
                    .Where(binding => binding != null && binding.prefab != null)
                    .Select(binding => binding.itemType)
                    .Distinct()
                    .ToList();
            }

            return Enum.GetValues(typeof(ItemType)).Cast<ItemType>().ToList();
        }

        private static List<Block> CreateBlocks(int totalBlockCount, List<ItemType> allowedTypes, Random random)
        {
            List<Block> blocks = new(totalBlockCount);

            for (int i = 0; i < totalBlockCount; i++)
            {
                ItemType type = allowedTypes[random.Next(allowedTypes.Count)];
                int count = AllowedCounts[random.Next(AllowedCounts.Length)];
                blocks.Add(new(type, count));
            }

            return blocks;
        }

        private static List<LaneDefinition> CreateLanes(int laneCount)
        {
            List<LaneDefinition> lanes = new(laneCount);
            for (int i = 0; i < laneCount; i++)
            {
                lanes.Add(new());
            }

            return lanes;
        }

        private static void AddBlockToLane(LaneDefinition lane, Block block)
        {
            for (int i = 0; i < block.Count; i++)
            {
                lane.items.Add(new() { itemType = block.ItemType });
            }
        }

        private static List<RequesterDefinition> CreateRequesters(int requesterCount, float halfWindow)
        {
            List<RequesterDefinition> requesters = new(requesterCount);

            for (int i = 0; i < requesterCount; i++)
            {
                float t = requesterCount == 1 ? 0.5f : i / (requesterCount - 1f);
                float center = Mathf.Lerp(0.25f, 0.75f, t);
                requesters.Add(new()
                {
                    minMove01 = Mathf.Clamp01(center - halfWindow),
                    maxMove01 = Mathf.Clamp01(center + halfWindow),
                    requests = new()
                });
            }

            return requesters;
        }

        private static bool HasSameOrder(List<Block> a, List<Block> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (a[i].ItemType != b[i].ItemType || a[i].Count != b[i].Count)
                {
                    return false;
                }
            }

            return true;
        }

        private static void Shuffle<T>(IList<T> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private readonly struct Block
        {
            public readonly ItemType ItemType;
            public readonly Sprite Icon;
            public readonly int Count;

            public Block(ItemType itemType, int count)
            {
                ItemType = itemType;
                Icon = Resources.Load<Sprite>("Sprites/Items/" + itemType);
                Count = count;
            }
        }
    }
}