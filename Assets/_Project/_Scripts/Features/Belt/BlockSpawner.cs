using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace PaintFlow.Features.Belt
{
    public class BlockSpawner : MonoBehaviour
    {
        [SerializeField] private BlockController _blockPrefab;
        [SerializeField] private int _defaultCapacity = 20;
        [SerializeField] private int _maxPoolSize = 40;

        private IBeltController _beltController;
        private ObjectPool<BlockController> _pool;

        private void Awake()
        {
            _pool = new(
                CreateBlock,
                OnBlockGet,
                OnBlockRelease,
                OnBlockDestroy,
                false,
                _defaultCapacity,
                _maxPoolSize
            );
        }

        [Inject]
        public void Construct(IBeltController beltController)
        {
            _beltController = beltController;
        }

        // ─── Public API ──────────────────────────────────────────
        public bool SpawnBlock(int colorIndex, Vector3 spawnPosition)
        {
            BlockController block = _pool.Get();
            block.transform.position = spawnPosition;
            block.SetColor(colorIndex);
            block.OnJumpComplete += HandleJumpComplete;

            bool added = _beltController.TryAddBlock(block);

            if (!added)
            {
                block.OnJumpComplete -= HandleJumpComplete;
                _pool.Release(block);
            }

            return added;
        }

        // ─── Pool Callbacks ──────────────────────────────────────
        private BlockController CreateBlock()
        {
            BlockController block = Instantiate(_blockPrefab, transform);
            return block;
        }

        private void OnBlockGet(BlockController block)
        {
            block.gameObject.SetActive(true);
            block.OnSpawn();
        }

        private void OnBlockRelease(BlockController block)
        {
            block.OnDespawn();
            block.gameObject.SetActive(false);
        }

        private void OnBlockDestroy(BlockController block)
        {
            Destroy(block.gameObject);
        }

        // ─── Private ─────────────────────────────────────────────
        private void HandleJumpComplete(BlockController block)
        {
            block.OnJumpComplete -= HandleJumpComplete;
            _pool.Release(block);
        }

        public BlockController GetFromPool()
        {
            return _pool.Get();
        }
    }
}