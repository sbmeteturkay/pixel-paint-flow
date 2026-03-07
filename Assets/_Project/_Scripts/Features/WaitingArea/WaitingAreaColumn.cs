using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PaintFlow.Features.Belt;
using PrimeTween;
using UnityEngine;

namespace PaintFlow.Features.WaitingArea
{
    public class WaitingAreaColumn : MonoBehaviour
    {
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private float _slideDownDuration = 0.2f;
        private readonly Queue<BlockController> _queue = new();

        private IBeltController _beltController;
        private BlockSpawner _blockSpawner;
        private bool _isSliding;
        private Action<BlockController> _onBlockSent;

        public void Initialize(IBeltController beltController, BlockSpawner blockSpawner,
            Action<BlockController> onBlockSent)
        {
            _beltController = beltController;
            _blockSpawner = blockSpawner;
            _onBlockSent = onBlockSent;
        }

        public void Enqueue(BlockController block, int slotIndex)
        {
            block.transform.SetParent(transform);
            block.transform.localPosition = new(0f, slotIndex * _cellSize, 0f);
            block.SetTapCallback(OnBlockTapped);
            _queue.Enqueue(block);
        }

        // ─── Private ─────────────────────────────────────────────
        private void OnBlockTapped(BlockController block)
        {
            if (_isSliding) return;
            if (_queue.Count == 0) return;
            if (_queue.Peek() != block) return;

            SendFrontBlock().Forget();
        }

        private async UniTaskVoid SendFrontBlock()
        {
            _isSliding = true;

            BlockController front = _queue.Dequeue();
            _onBlockSent?.Invoke(front);

            bool added = _blockSpawner.SpawnBlock(front.ColorIndex, front.transform.position);

            if (added)
                await SlideQueueDown();

            _isSliding = false;
        }

        private async UniTask SlideQueueDown()
        {
            List<UniTask> tasks = new();

            int index = 0;
            foreach (BlockController block in _queue)
            {
                Vector3 targetPos = new(0f, index * _cellSize, 0f);
                tasks.Add(Tween.LocalPosition(block.transform, targetPos, _slideDownDuration).ToUniTask());
                index++;
            }

            await UniTask.WhenAll(tasks);
        }
    }
}