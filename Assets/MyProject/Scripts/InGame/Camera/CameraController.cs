using UnityEngine;

namespace TPSRoguelite.InGame.Camera
{
    public class CameraController : MonoBehaviour
    {
        const float LOOK_SENSITIVITY = 0.2f;

        const float DISTANCE = 5f;

        const float HEIGHT_OFFSET = 1.5f;

        const float MIN_PITCH = -10f;

        const float MAX_PITCH = 60f;

        [SerializeField] Transform _target;

        PlayerInputActions _inputActions;

        Vector2 _lookInput;

        float _currentYaw;

        float _currentPitch = 20f;

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

            _currentYaw += _lookInput.x * LOOK_SENSITIVITY;
            _currentPitch -= _lookInput.y * LOOK_SENSITIVITY;

            _currentPitch = Mathf.Clamp(_currentPitch, MIN_PITCH, MAX_PITCH);
        }

        private void LateUpdate()
        {
            if(_target == null)
            {
                return;
            }

            Vector3 targetPosition = _target.position + Vector3.up * HEIGHT_OFFSET;

            Quaternion rotate = Quaternion.Euler(_currentPitch, _currentYaw, 0f);

            Vector3 cameraPosition = targetPosition - (rotate * Vector3.forward * DISTANCE);

            transform.position = cameraPosition;
            transform.rotation = rotate;
        }
    }
}
