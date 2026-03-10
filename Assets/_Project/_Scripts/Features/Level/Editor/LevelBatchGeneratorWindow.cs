#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using PaintFlow.Core.Gameplay;
using PaintFlow.Features.Level;
using PaintFlow.Features.QueueLane;
using UnityEditor;
using UnityEngine;

namespace PaintFlow.Features.Level.Editor
{
    public class LevelBatchGeneratorWindow : EditorWindow
    {
        private const string LevelsFolderPath = "Assets/_Project/Level/Data/Resources/Levels";
        private const string ItemResourcesPath = "Items";

        private int _startLevelId = 1;
        private int _levelCount = 100;
        private int _baseSeed = 1000;
        private bool _overwriteExisting = false;

        [MenuItem("PaintFlow/Level/Batch Generate Levels")]
        private static void Open()
        {
            GetWindow<LevelBatchGeneratorWindow>("Level Batch Generator");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Batch Level Generator", EditorStyles.boldLabel);
            _startLevelId = EditorGUILayout.IntField("Start Level Id", _startLevelId);
            _levelCount = EditorGUILayout.IntField("Level Count", _levelCount);
            _baseSeed = EditorGUILayout.IntField("Base Seed", _baseSeed);
            _overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing", _overwriteExisting);

            if (GUILayout.Button("Generate"))
            {
                GenerateLevels();
            }
        }

        private void GenerateLevels()
        {
            if (_levelCount <= 0)
            {
                Debug.LogWarning("Level count must be greater than zero.");
                return;
            }

            List<ItemPrefabBinding> allPrefabs = LoadItemPrefabsFromResources();
            if (allPrefabs.Count == 0)
            {
                Debug.LogWarning("No item prefabs found in Resources/Items. Ensure prefab names match ItemType enum values.");
                return;
            }

            EnsureFolderExists(LevelsFolderPath);

            for (int i = 0; i < _levelCount; i++)
            {
                int levelId = _startLevelId + i;
                string assetPath = $"{LevelsFolderPath}/L_{levelId:000}.asset";

                LevelData levelData = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
                bool shouldCreate = levelData == null;

                if (shouldCreate)
                {
                    levelData = CreateInstance<LevelData>();
                    levelData.laneCount = 2;
                    levelData.requesterCount = 2;
                    AssetDatabase.CreateAsset(levelData, assetPath);
                }
                else if (!_overwriteExisting)
                {
                    continue;
                }

                levelData.levelId = levelId;
                levelData.seed = _baseSeed + levelId;
                levelData.itemPrefabs = BuildPrefabSubset(allPrefabs, levelId);
                levelData.RegenerateFromSeed();
                EditorUtility.SetDirty(levelData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Generated levels at: {LevelsFolderPath}");
        }

        private static List<ItemPrefabBinding> LoadItemPrefabsFromResources()
        {
            QueueItem[] prefabs = Resources.LoadAll<QueueItem>(ItemResourcesPath);
            List<ItemPrefabBinding> bindings = new();

            foreach (QueueItem prefab in prefabs)
            {
                if (prefab == null)
                {
                    continue;
                }

                if (!Enum.TryParse(prefab.name, out ItemType itemType))
                {
                    continue;
                }

                bindings.Add(new ItemPrefabBinding
                {
                    itemType = itemType,
                    prefab = prefab,
                    defaultCapacity = 8,
                    maxSize = 32
                });
            }

            return bindings.OrderBy(b => b.itemType).ToList();
        }

        private static List<ItemPrefabBinding> BuildPrefabSubset(List<ItemPrefabBinding> allPrefabs, int levelId)
        {
            int totalTypes = allPrefabs.Count;
            int allowedCount = Mathf.Clamp(2 + levelId / 5, 1, totalTypes);
            return allPrefabs.Take(allowedCount).ToList();
        }

        private static void EnsureFolderExists(string fullFolderPath)
        {
            string[] segments = fullFolderPath.Split('/');
            string current = segments[0];

            for (int i = 1; i < segments.Length; i++)
            {
                string next = $"{current}/{segments[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[i]);
                }

                current = next;
            }
        }
    }
}
#endif
