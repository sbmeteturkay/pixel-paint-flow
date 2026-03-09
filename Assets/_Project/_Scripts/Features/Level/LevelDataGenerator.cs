using System;
using System.Collections.Generic;
using PaintFlow.Core.Gameplay;
using PaintFlow.Features.QueueLane;
using UnityEngine;

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

            System.Random random = new(levelData.seed);
            List<Block> blocks = CreateBlocks(totalBlockCount, random);

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
                requester.requests.Add(new ItemRequestDefinition
                {
                    itemType = block.ItemType,
                    count = block.Count
                });
            }

            levelData.lanes = lanes;
            levelData.requesters = requesters;
            levelData.laneCount = laneCount;
            levelData.requesterCount = requesterCount;
        }

        private static List<Block> CreateBlocks(int totalBlockCount, System.Random random)
        {
            List<Block> blocks = new(totalBlockCount);
            Array itemTypes = Enum.GetValues(typeof(ItemType));

            for (int i = 0; i < totalBlockCount; i++)
            {
                ItemType type = (ItemType)itemTypes.GetValue(random.Next(itemTypes.Length));
                int count = AllowedCounts[random.Next(AllowedCounts.Length)];
                blocks.Add(new Block(type, count));
            }

            return blocks;
        }

        private static List<LaneDefinition> CreateLanes(int laneCount)
        {
            List<LaneDefinition> lanes = new(laneCount);
            for (int i = 0; i < laneCount; i++)
            {
                lanes.Add(new LaneDefinition());
            }

            return lanes;
        }

        private static void AddBlockToLane(LaneDefinition lane, Block block)
        {
            for (int i = 0; i < block.Count; i++)
            {
                lane.items.Add(new QueueLaneItemData { itemType = block.ItemType });
            }
        }

        private static List<RequesterDefinition> CreateRequesters(int requesterCount, float halfWindow)
        {
            List<RequesterDefinition> requesters = new(requesterCount);

            for (int i = 0; i < requesterCount; i++)
            {
                float t = requesterCount == 1 ? 0.5f : i / (requesterCount - 1f);
                float center = Mathf.Lerp(0.25f, 0.75f, t);
                requesters.Add(new RequesterDefinition
                {
                    minMove01 = Mathf.Clamp01(center - halfWindow),
                    maxMove01 = Mathf.Clamp01(center + halfWindow),
                    requests = new List<ItemRequestDefinition>()
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

        private static void Shuffle<T>(IList<T> list, System.Random random)
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
            public readonly int Count;

            public Block(ItemType itemType, int count)
            {
                ItemType = itemType;
                Count = count;
            }
        }
    }
}
