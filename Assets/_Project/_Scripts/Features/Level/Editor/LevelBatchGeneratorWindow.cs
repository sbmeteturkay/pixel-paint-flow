#if UNITY_EDITOR
using PaintFlow.Features.Level;
using UnityEditor;
using UnityEngine;

namespace PaintFlow.Features.Level.Editor
{
    public class LevelBatchGeneratorWindow : EditorWindow
    {
        private const string LevelsFolderPath = "Assets/_Project/Level/Data/Resources/Levels";

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
                levelData.RegenerateFromSeed();
                EditorUtility.SetDirty(levelData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Generated levels at: {LevelsFolderPath}");
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
