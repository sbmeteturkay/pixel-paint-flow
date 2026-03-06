using System;
using Cysharp.Threading.Tasks;
using PaintFlow.Shared;
using PrimeTween;
using UnityEngine;

namespace PaintFlow.Features.Belt
{
    public enum BlockState
    {
        Idle,
        OnBelt,
        Jumping
    }

    public class BlockController : MonoBehaviour, IPoolable
    {
        [SerializeField] private BeltMover _beltMover;

        public Action<BlockController> OnJumpComplete;

        public int ColorIndex { get; private set; }
        public float T => _beltMover.GetCurrentT();
        public BlockState CurrentState { get; private set; }

        // ─── IPoolable ───────────────────────────────────────────
        public void OnSpawn()
        {
            CurrentState = BlockState.Idle;
            _beltMover.enabled = false;
        }

        public void OnDespawn()
        {
            CurrentState = BlockState.Idle;
            _beltMover.enabled = false;
            OnJumpComplete = null;
        }

        // ─── Public API ──────────────────────────────────────────
        public void SetColor(Color color)
        {
            // TODO: palette[colorIndex] → renderer'a uygula
        }

        public void EnterBelt()
        {
            if (CurrentState != BlockState.Idle) return;
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