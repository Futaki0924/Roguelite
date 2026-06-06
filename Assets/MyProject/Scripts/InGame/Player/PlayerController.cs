using Core.Interface;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System;

namespace TPSRoguelite.InGame.Player
{
    public class PlayerController : MonoBehaviour
    {
        const float MOVE_SPEED = 5f;

        const float ROTATE_SPEED = 10f;

        const float LASER_MAX_DISTANCE = 50f;

        const int ATTACK_DAMAGE = 20;

        const float ATTACK_RANGE = 50f;

        const int MAX_AMMO = 30;

        const float RELOAD_TIME = 1.5f;

        [SerializeField] Rigidbody _rb;

        [SerializeField] Transform _weaponOrigin;

        [SerializeField] LineRenderer _laserLineRendrer;

        Vector3 _moveDirection;

        PlayerInputActions _inputActions;

        Vector2 _moveInput;

        Transform _mainCameraTransform;

        bool isReloading;

        public Vector3 CurrentVelocity { get; private set; }

        public int CurrentAmmo { get; private set; }

        private void Awake()
        {
            CurrentAmmo = MAX_AMMO;

            _inputActions = new PlayerInputActions();
            _inputActions.Player.Fire.performed += OnFire;
            _inputActions.Player.Reload.performed += OnReload;

            if(UnityEngine.Camera.main != null)
            {
                _mainCameraTransform = UnityEngine.Camera.main.transform;
            }
            else
            {
                Debug.LogError("Main Cameraが見つかりませんでした");
            }
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
            _moveInput = _inputActions.Player.Move.ReadValue<Vector2>();

            DrawLaserPointer();
        }

        private void FixedUpdate()
        {
            OnMove();
        }

        private void OnMove()
        {
            if (_rb == null)
            {
                Debug.LogError("Rigidbodyがないよ！！");
                return;
            }

            if (_moveInput == Vector2.zero)
            {
                _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
                CurrentVelocity = Vector3.zero;
                return;
            }

            Vector3 cameraForward = _mainCameraTransform.forward;
            Vector3 cameraRight = _mainCameraTransform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward * _moveInput.y + cameraRight * _moveInput.x).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            _rb.rotation = Quaternion.Slerp(_rb.rotation, targetRotation, ROTATE_SPEED * Time.deltaTime);

            Vector3 targetVelocity = moveDirection * MOVE_SPEED;
            _rb.linearVelocity = new Vector3(targetVelocity.x, _rb.linearVelocity.y, targetVelocity.z);

            CurrentVelocity = _rb.linearVelocity;
        }

        private void OnFire(InputAction.CallbackContext context)
        {
            Ray ray = new Ray(_mainCameraTransform.position, _mainCameraTransform.forward);

            if(Physics.Raycast(ray, out RaycastHit hitInfo, ATTACK_RANGE))
            {
                Debug.Log($"{hitInfo.collider.name}に命中！");

                IDamageable target = hitInfo.collider.GetComponent<IDamageable>();

                if(target != null)
                {
                    target.TakeDamage(ATTACK_DAMAGE);
                }
            }
        }

        private void OnReload(InputAction.CallbackContext context)
        {
            if(isReloading || CurrentAmmo == MAX_AMMO)
            {
                return;
            }

            ReloadAsync().Forget();
        }

        private async UniTask ReloadAsync()
        {
            isReloading = true;
            Debug.Log("リロード中");

            await UniTask.Delay(TimeSpan.FromSeconds(RELOAD_TIME), cancellationToken: this.GetCancellationTokenOnDestroy());

            CurrentAmmo = MAX_AMMO;
            isReloading = false;
            Debug.Log("リロード完了");
        }

        private void DrawLaserPointer()
        {
            if(_laserLineRendrer == null || _weaponOrigin == null || _mainCameraTransform == null)
            {
                return;
            }

            _laserLineRendrer.SetPosition(0, _weaponOrigin.position);
            Ray ray = new Ray(_mainCameraTransform.position, _mainCameraTransform.forward);
            if(Physics.Raycast(ray, out RaycastHit hitInfo, LASER_MAX_DISTANCE))
            {
                _laserLineRendrer.SetPosition(1, hitInfo.point);
            }
            else
            {
                _laserLineRendrer.SetPosition(1, ray.GetPoint(LASER_MAX_DISTANCE));
            }
        }
    }
}
