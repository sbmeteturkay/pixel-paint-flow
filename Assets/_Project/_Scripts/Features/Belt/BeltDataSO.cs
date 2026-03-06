using UnityEngine;

namespace PaintFlow.Features.Belt
{
    [CreateAssetMenu(fileName = "SO_BeltData", menuName = "PaintFlow/Belt/Belt Data")]
    public class BeltDataSO : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField] private float _beltSpeed = 2f;

        [Header("Capacity")]
        [SerializeField] private int _maxCapacity = 10;

        public float BeltSpeed => _beltSpeed;
        public int MaxCapacity => _maxCapacity;
    }
}