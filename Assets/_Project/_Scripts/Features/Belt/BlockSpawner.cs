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

        [Inject]
        public void Construct(IBeltController beltController)
        {
            _beltController = beltController;
        }

        private void Awake()
        {
            _pool = new ObjectPool<BlockController>(
                createFunc: CreateBlock,
                actionOnGet: OnBlockGet,
                actionOnRelease: OnBlockRelease,
                actionOnDestroy: OnBlockDestroy,
                collectionCheck: false,
                defaultCapacity: _defaultCapacity,
                maxSize: _maxPoolSize
            );
        }

        // ─── Public API ──────────────────────────────────────────
        public void SpawnBlock(Color color, Vector3 spawnPosition)
        {
            var block = _pool.Get();
            block.transform.position = spawnPosition;
            block.SetColor(color);
            block.OnJumpComplete += HandleJumpComplete;

            _beltController.TryAddBlock(block);
        }

        // ─── Pool Callbacks ──────────────────────────────────────
        private BlockController CreateBlock()
        {
            var block = Instantiate(_blockPrefab, transform);
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
    }
}