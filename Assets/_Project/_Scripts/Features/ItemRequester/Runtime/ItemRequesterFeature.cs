using System.Collections.Generic;
using PaintFlow.Core.Gameplay;
using PaintFlow.Features.Level;
using PaintFlow.Features.QueueLane;
using PrimeTween;
using UnityEngine;
using VContainer;

namespace PaintFlow.Features.ItemRequester
{
    public class ItemRequesterFeature : MonoBehaviour
    {
        private readonly bool _buildOnStart = true;

        [Header("Matched Item Animation")]
        private readonly float _matchedDuration = 0.5f;

        private readonly float _matchedJumpHeight = 3f;
        private readonly int _matchedSpinTurns = 3;
        private readonly List<ItemRequester> _runtimeRequesters = new();
        private readonly Vector3 _spinAxis = Vector3.up;
        private LevelLoader _levelLoader;
        [SerializeField] private PoppedItemSplineFlow _poppedItemSplineFlow;
        [SerializeField] private ItemRequester _requesterPrefab;
        [SerializeField] private Transform _requesterRoot;

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
            if (_poppedItemSplineFlow == null || _runtimeRequesters.Count == 0)
            {
                return;
            }

            bool consumed;
            do
            {
                consumed = ProcessAllMatchesOnce();
            } while (consumed);
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

        private bool ProcessAllMatchesOnce()
        {
            bool consumedAny = false;

            for (int i = 0; i < _runtimeRequesters.Count; i++)
            {
                if (!_runtimeRequesters[i].TryConsumeOne(_poppedItemSplineFlow, out IQueueItem matchedItem))
                {
                    continue;
                }

                consumedAny = true;
                PlayMatchedAnimation(matchedItem, _runtimeRequesters[i].transform.position);
            }

            return consumedAny;
        }

        private void PlayMatchedAnimation(IQueueItem matchedItem, Vector3 transformPosition)
        {
            if (matchedItem == null || matchedItem.GameObject == null)
            {
                return;
            }

            Transform itemTransform = matchedItem.GameObject.transform;
            Vector3 startPosition = itemTransform.position;
            Vector3 spinDelta = _spinAxis.normalized * (360f * Mathf.Max(1, _matchedSpinTurns));
            float duration = Mathf.Max(0.01f, _matchedDuration);
            float jumpHeight = Mathf.Max(0f, _matchedJumpHeight);

            Sequence.Create()
                .Group(Tween.Custom(
                    0f,
                    1f,
                    duration,
                    t =>
                    {
                        float yOffset = 4f * jumpHeight * t * (1f - t);
                        itemTransform.position = startPosition * (1 - t) + transformPosition * t + Vector3.up * yOffset;
                    },
                    Ease.OutQuad))
                .Group(Tween.Rotation(itemTransform, spinDelta, duration, Ease.Linear))
                .OnComplete(matchedItem, item => item.ReturnToPool());
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