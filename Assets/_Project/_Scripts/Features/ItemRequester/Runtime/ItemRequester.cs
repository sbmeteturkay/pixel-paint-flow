using System.Collections.Generic;
using PaintFlow.Core.Gameplay;
using PaintFlow.Features.Level;
using PaintFlow.Features.QueueLane;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PaintFlow.Features.ItemRequester
{
    public class ItemRequester : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text remainingText;

        [SerializeField] private Image requestIcon;
        [SerializeField] private CanvasGroup canvasFadeAnimate;

        private readonly Queue<ItemRequestRuntime> _pendingRequests = new();
        private ItemRequestRuntime _activeRequest;
        private bool _isFirstInit = true;

        private bool _isTransitioning; // Animasyon devam ediyor mu?
        private float _maxMove01;

        private float _minMove01;

        public bool HasActiveRequest => _activeRequest != null;

        public void Initialize(RequesterDefinition definition)
        {
            // Reset state
            _pendingRequests.Clear();
            _activeRequest = null;
            _isFirstInit = true;

            // Mevcut animasyonları durdur (Üst üste binmeyi önler)
            Tween.StopAll(canvasFadeAnimate);

            if (definition == null || definition.requests == null) return;

            // Değerleri garantiye al
            _minMove01 = Mathf.Min(definition.minMove01, definition.maxMove01);
            _maxMove01 = Mathf.Max(definition.minMove01, definition.maxMove01);

            foreach (ItemRequestDefinition req in definition.requests)
            {
                _pendingRequests.Enqueue(new(req.itemType, req.icon, req.count));
            }

            AdvanceRequest();
        }

        public bool TryConsumeOne(PoppedItemSplineFlow splineFlow, out IQueueItem matchedItem)
        {
            matchedItem = null;

            // Eğer şu an animasyon (geçiş) varsa veya aktif istek yoksa hiçbir şey yapma
            if (_isTransitioning || splineFlow == null || _activeRequest == null)
                return false;

            if (splineFlow.TryTake(IsMatch, out matchedItem))
            {
                _activeRequest.Remaining--;

                if (_activeRequest.Remaining <= 0)
                {
                    AdvanceRequest();
                }
                else
                {
                    UpdateRemainingUI();
                }

                return true;
            }

            return false;
        }

        private void AdvanceRequest()
        {
            _activeRequest = _pendingRequests.Count > 0 ? _pendingRequests.Dequeue() : null;

            if (_activeRequest != null)
            {
                if (_isFirstInit)
                {
                    _isFirstInit = false;
                    RefreshFullUI();
                    canvasFadeAnimate.alpha = 1f;
                    _isTransitioning = false; // İlk açılışta direkt hazır
                }
                else
                {
                    _isTransitioning = true; // Geçiş başladı, "take" engellendi

                    Sequence.Create()
                        .Chain(Tween.Alpha(canvasFadeAnimate, 0f, 0.25f, startDelay: .2f))
                        .ChainCallback(RefreshFullUI)
                        .Chain(Tween.Alpha(canvasFadeAnimate, 1f, 0.25f))
                        .OnComplete(() => _isTransitioning = false); // Animasyon bitti, artık alınabilir!
                }
            }
            else
            {
                _isTransitioning = true; // Artık istek yok, etkileşimi tamamen kapat
                Tween.Alpha(canvasFadeAnimate, 0f, 0.3f);
            }
        }

        private bool IsMatch(IQueueItem item)
        {
            if (_activeRequest == null || item == null) return false;

            return item.ItemType == _activeRequest.ItemType &&
                   item.OnMove01 >= _minMove01 &&
                   item.OnMove01 <= _maxMove01;
        }

        private void RefreshFullUI()
        {
            if (_activeRequest == null) return;
            if (requestIcon != null) requestIcon.sprite = _activeRequest.ItemIcon;
            UpdateRemainingUI();
        }

        private void UpdateRemainingUI()
        {
            if (_activeRequest != null && remainingText != null)
            {
                remainingText.text = _activeRequest.Remaining.ToString();
            }
        }

        private sealed class ItemRequestRuntime
        {
            public readonly Sprite ItemIcon;
            public readonly ItemType ItemType;
            public int Remaining;

            public ItemRequestRuntime(ItemType itemType, Sprite icon, int remaining)
            {
                ItemType = itemType;
                ItemIcon = icon;
                Remaining = remaining;
            }
        }
    }
}