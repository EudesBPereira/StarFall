using Starfall.Core;
using Starfall.Input;
using UnityEngine;

namespace Starfall.Player
{
    /// <summary>
    /// Eight-direction movement with acceleration/deceleration for axis input, plus 1:1 drag movement for touch.
    /// Always clamped to the visible play area.
    /// </summary>
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] internal Transform bankTarget;

        private float _speed = 9f;
        private float _acceleration = 60f;
        private float _deceleration = 80f;
        private float _bankAngle = 18f;
        private float _padding = 0.45f;
        private PlayArea _area;
        private Vector2 _velocity;

        public Vector2 Velocity => _velocity;

        public void Configure(ShipDefinition ship, PlayArea area, float edgePadding)
        {
            _area = area;
            _padding = edgePadding;
            if (ship != null)
            {
                _speed = ship.MoveSpeed;
                _acceleration = ship.Acceleration;
                _deceleration = ship.Deceleration;
                _bankAngle = ship.BankAngle;
            }
        }

        public void Tick(IGameInput input, float speedMultiplier, float dragSensitivity)
        {
            float dt = Time.deltaTime;
            Vector2 pos = transform.position;

            Vector2 targetVelocity = input.MoveAxis * (_speed * speedMultiplier);
            float rate = input.MoveAxis.sqrMagnitude > 0.001f ? _acceleration : _deceleration;
            _velocity = Vector2.MoveTowards(_velocity, targetVelocity, rate * dt);
            pos += _velocity * dt;

            if (input.DragActive && _area != null && Screen.height > 0)
            {
                float unitsPerPixel = _area.Height / Screen.height;
                pos += input.DragDeltaScreen * (unitsPerPixel * dragSensitivity);
            }

            if (_area != null) pos = _area.Clamp(pos, _padding);
            transform.position = new Vector3(pos.x, pos.y, transform.position.z);

            if (bankTarget != null)
            {
                float horizontal = _speed > 0f ? Mathf.Clamp(_velocity.x / _speed, -1f, 1f) : 0f;
                if (input.DragActive && dt > 0f)
                    horizontal = Mathf.Clamp(input.DragDeltaScreen.x * 0.05f, -1f, 1f);
                float targetAngle = -horizontal * _bankAngle;
                float current = bankTarget.localEulerAngles.z;
                if (current > 180f) current -= 360f;
                float angle = Mathf.Lerp(current, targetAngle, 12f * dt);
                bankTarget.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        public void ResetMotion()
        {
            _velocity = Vector2.zero;
            if (bankTarget != null) bankTarget.localRotation = Quaternion.identity;
        }
    }
}
