using UnityEngine;
using UnityEngine.Splines;

namespace PaintFlow.Features.Belt
{
    [RequireComponent(typeof(BlockController))]
    public class BeltMover : MonoBehaviour
    {
        private float _speed;
        private SplineContainer _splineContainer;
        private float _splineLength;
        private float _t;

        private void Update()
        {
            if (_splineContainer == null) return;

            _t += _speed * Time.deltaTime / _splineLength;

            if (_t >= 1f) _t -= 1f;

            Vector3 position = _splineContainer.EvaluatePosition(_t);
            Vector3 forward = _splineContainer.EvaluateTangent(_t);

            transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward));
        }

        public void Initialize(SplineContainer splineContainer, float speed, float splineLength)
        {
            _splineContainer = splineContainer;
            _speed = speed;
            _splineLength = splineLength;
            _t = 0f;
            enabled = false;
        }

        public void ResetT()
        {
            _t = 0f;
        }

        public float GetCurrentT()
        {
            return _t;
        }
    }
}