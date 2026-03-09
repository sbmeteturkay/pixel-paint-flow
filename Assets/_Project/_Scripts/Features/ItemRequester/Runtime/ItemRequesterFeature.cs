using System.Collections.Generic;
using PaintFlow.Features.Level;
using UnityEngine;
using VContainer;

namespace PaintFlow.Features.ItemRequester
{
    public class ItemRequesterFeature : MonoBehaviour
    {
        [SerializeField] private ItemRequester _requesterPrefab;
        [SerializeField] private Transform _requesterRoot;
        [SerializeField] private bool _buildOnStart = true;
        [SerializeField] private ThrownItemBuffer _thrownItemBuffer;

        private readonly List<ItemRequester> _runtimeRequesters = new();
        private LevelLoader _levelLoader;

        [Inject]
        public void Construct(LevelLoader levelLoader)
        {
            _levelLoader = levelLoader;
        }

        private void Start()
        {
            if (_buildOnStart)
            {
                BuildCurrentLevel();
            }
        }

        private void Update()
        {
            if (_thrownItemBuffer == null || _runtimeRequesters.Count == 0)
            {
                return;
            }

            bool consumed;
            do
            {
                consumed = false;
                for (int i = 0; i < _runtimeRequesters.Count; i++)
                {
                    if (_runtimeRequesters[i].TryConsume(_thrownItemBuffer))
                    {
                        consumed = true;
                    }
                }
            }
            while (consumed);
        }

        public void BuildCurrentLevel()
        {
            Build(_levelLoader != null ? _levelLoader.CurrentLevel : null);
        }

        public void Build(LevelData levelData)
        {
            ClearRequesters();

            if (levelData == null || _requesterPrefab == null || levelData.requesters == null)
            {
                return;
            }

            Transform parent = _requesterRoot != null ? _requesterRoot : transform;

            for (int i = 0; i < levelData.requesters.Count; i++)
            {
                ItemRequester requester = Instantiate(_requesterPrefab, parent);
                requester.Initialize(levelData.requesters[i]);
                _runtimeRequesters.Add(requester);
            }
        }

        private void ClearRequesters()
        {
            for (int i = 0; i < _runtimeRequesters.Count; i++)
            {
                if (_runtimeRequesters[i] != null)
                {
                    Destroy(_runtimeRequesters[i].gameObject);
                }
            }

            _runtimeRequesters.Clear();
        }
    }
}
