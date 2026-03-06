using UnityEngine;
using UnityEngine.Splines;

namespace PaintFlow.Features.Belt
{
    [RequireComponent(typeof(BlockController))]
    public class BeltMover : MonoBehaviour
    {
        private SplineContainer _splineContainer;
        private float _speed;
        private float _splineLength;
        private float _t;

        public void Initialize(SplineContainer splineContainer, float speed, float splineLength)
        {
            _splineContainer = splineContainer;
            _speed = speed;
            _splineLength = splineLength;
            _t = 0f;
            enabled = false;
        }

        public void ResetT() => _t = 0f;

        private void Update()
        {
            if (_splineContainer == null) return;

            _t += _speed * Time.deltaTime / _splineLength;

            if (_t >= 1f) _t -= 1f;

            var position = (Vector3)_splineContainer.EvaluatePosition(_t);
            var forward = (Vector3)_splineContainer.EvaluateTangent(_t);

            transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward));
        }
    }
}