// Features/Belt/Scripts/BlockController.cs

using System;
using Cysharp.Threading.Tasks;
using PaintFlow.Shared;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PaintFlow.Features.Belt
{
    public enum BlockState
    {
        Idle,
        OnBelt,
        Jumping
    }

    public class BlockController : MonoBehaviour, IPoolable, IPointerClickHandler
    {
        [SerializeField] private BeltMover _beltMover;
        private Action<BlockController> _onTapped;

        public Action<BlockController> OnJumpComplete;

        public int ColorIndex { get; private set; }
        public float T => _beltMover.GetCurrentT();
        public BlockState CurrentState { get; private set; }

        // ─── IPointerClickHandler ────────────────────────────────
        public void OnPointerClick(PointerEventData eventData)
        {
            if (CurrentState != BlockState.Idle) return;
            _onTapped?.Invoke(this);
        }

        // ─── IPoolable ───────────────────────────────────────────
        public void OnSpawn()
        {
            CurrentState = BlockState.Idle;
            _beltMover.enabled = false;
            _onTapped = null;
        }

        public void OnDespawn()
        {
            CurrentState = BlockState.Idle;
            _beltMover.enabled = false;
            OnJumpComplete = null;
            _onTapped = null;
        }

        // ─── Public API ──────────────────────────────────────────
        public void SetColor(int colorIndex)
        {
            ColorIndex = colorIndex;
            // TODO: palette[colorIndex] → renderer'a uygula
        }

        public void SetTapCallback(Action<BlockController> onTapped)
        {
            _onTapped = onTapped;
        }

        public void EnterBelt()
        {
            if (CurrentState != BlockState.Idle) return;
            _onTapped = null;
            CurrentState = BlockState.OnBelt;
            _beltMover.enabled = true;
        }

        public void JumpToTarget(Vector3 targetPosition)
        {
            if (CurrentState != BlockState.OnBelt) return;
            CurrentState = BlockState.Jumping;
            _beltMover.enabled = false;
            ExecuteJump(targetPosition).Forget();
        }

        // ─── Private ─────────────────────────────────────────────
        private async UniTaskVoid ExecuteJump(Vector3 targetPosition)
        {
            await Tween.Position(transform, targetPosition, 0.4f);
            OnJumpComplete?.Invoke(this);
        }
    }
}