using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PaintFlow.Features.Level
{
    public class LevelLoader : MonoBehaviour
    {
        [SerializeField] private bool _loadFromResources = true;
        [SerializeField] private string _resourcesPath = "Levels";
        [SerializeField] private List<LevelData> _levels = new();
        [SerializeField, Min(0)] private int _startLevelIndex;

        private LevelData _currentLevel;

        public LevelData CurrentLevel => _currentLevel;

        private void Awake()
        {
            EnsureLevelsLoaded();
            SetCurrentLevelByIndex(_startLevelIndex);
        }

        public void EnsureLevelsLoaded()
        {
            if (_loadFromResources)
            {
                _levels = Resources.LoadAll<LevelData>(_resourcesPath)
                    .OrderBy(level => level.levelId)
                    .ThenBy(level => level.name)
                    .ToList();
            }
        }

        public void SetCurrentLevelByIndex(int levelIndex)
        {
            if (_levels == null || _levels.Count == 0)
            {
                _currentLevel = null;
                return;
            }

            int clampedIndex = Mathf.Clamp(levelIndex, 0, _levels.Count - 1);
            _currentLevel = _levels[clampedIndex];
        }

        public void SetCurrentLevelById(int levelId)
        {
            if (_levels == null || _levels.Count == 0)
            {
                _currentLevel = null;
                return;
            }

            LevelData match = _levels.Find(level => level.levelId == levelId);
            _currentLevel = match != null ? match : _levels[0];
        }
    }
}
