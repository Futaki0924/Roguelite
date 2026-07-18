using UnityEngine;

namespace TPSRoguelite.InGame.Camera
{
    public class CameraController : MonoBehaviour
    {
        [Header("カメラの基本設定")]
        [SerializeField] float _lookSensitivity = 0.2f;
        [SerializeField] float _minPitch = -10f;
        [SerializeField] float _maxPitch = 60f;
        [SerializeField] float _zoomSpeed = 5f;

        [Header("カメラの視点")]
        [SerializeField] float _targetDistance = -5f;
        [SerializeField] float _targetHeightOffset = 1.2f;
        [SerializeField] float _targetShoulderOffset = 0.8f;

        [SerializeField] Transform _target;

        PlayerInputActions _inputActions;

        Vector2 _lookInput;

        float _currentYaw;

        float _currentPitch = 20f;

        float _currentDistance;

        float _currentHeightOffset;

        float _currentShoulderOffset;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnEnable()
        {
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        private void Update()
        {
            _lookInput = _inputActions.Player.Look.ReadValue<Vector2>();

            _currentYaw += _lookInput.x * _lookSensitivity;
            _currentPitch -= _lookInput.y * _lookSensitivity;

            _currentPitch = Mathf.Clamp(_currentPitch, _minPitch, _maxPitch);
        }

        private void LateUpdate()
        {
            if(_target == null)
            {
                return;
            }

            _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, _zoomSpeed * Time.deltaTime);
            _currentHeightOffset = Mathf.Lerp(_currentHeightOffset, _targetHeightOffset, _zoomSpeed * Time.deltaTime);
            _currentShoulderOffset = Mathf.Lerp(_currentShoulderOffset, _targetShoulderOffset, _zoomSpeed * Time.deltaTime);

            Quaternion rotate = Quaternion.Euler(_currentPitch, _currentYaw, 0f);

            Vector3 basePosition = _target.position + Vector3.up * _currentHeightOffset;

            Vector3 shoulderPosition = basePosition + rotate * Vector3.right * _currentShoulderOffset;

            Vector3 cameraPosition = shoulderPosition + rotate * Vector3.forward * _currentDistance;

            transform.position = cameraPosition;
            transform.rotation = rotate;
        }
    }
}
