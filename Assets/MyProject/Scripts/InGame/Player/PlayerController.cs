using Core.Interface;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Core.MasterData;
using TPSRoguelite.InGame.Enum;

namespace TPSRoguelite.InGame.Player
{
    public class PlayerController : MonoBehaviour
    {
        const float MOVE_SPEED = 8f;

        const float ROTATE_SPEED = 10f;

        const float LASER_MAX_DISTANCE = 50f;

        const float ATTACK_RANGE = 50f;

        [SerializeField] Rigidbody _rb;

        [SerializeField] Transform _weaponOrigin;

        [SerializeField] LineRenderer _laserLineRendrer;

        [SerializeField] ulong _weaponId = 1;

        WeaponDataRecord _currentWeapon;

        Vector3 _moveDirection;

        PlayerInputActions _inputActions;

        Vector2 _moveInput;

        Transform _mainCameraTransform;

        bool _isReloading;

        bool _canShoot = true;

        CancellationTokenSource _fireCts;

        public Vector3 CurrentVelocity { get; private set; }

        public int CurrentAmmo { get; private set; }

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void SetUp()
        {
            _currentWeapon = MasterDataAccessor.Instance.GetById<WeaponDataRecord>(_weaponId);

            if (_currentWeapon != null)
            {
                CurrentAmmo = _currentWeapon.MaxAmmo;
            }
            else
            {
                Debug.LogError("WeaponDataがありません。");
                return;
            }

            _inputActions = new PlayerInputActions();
            _inputActions.Player.Fire.performed += OnFire;
            _inputActions.Player.Fire.canceled += OnFire;
            _inputActions.Player.Reload.performed += OnReload;

            if (UnityEngine.Camera.main != null)
            {
                _mainCameraTransform = UnityEngine.Camera.main.transform;
            }
            else
            {
                Debug.LogError("Main Cameraが見つかりませんでした");
            }

            gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            _inputActions?.Enable();
        }

        private void OnDisable()
        {
            _inputActions?.Disable();
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
            if (_rb == null  || _mainCameraTransform == null)
            {
                Debug.LogError("Rigidbodyがないよ！！");
                return;
            }

            Vector3 cameraForward = _mainCameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            if(cameraForward != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
                _rb.rotation = Quaternion.Slerp(_rb.rotation, targetRotation, ROTATE_SPEED * Time.fixedDeltaTime);
            }

            if (_moveInput == Vector2.zero)
            {
                _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
                CurrentVelocity = Vector3.zero;
                return;
            }

            Vector3 cameraRight = _mainCameraTransform.right;

            cameraRight.y = 0f;
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward * _moveInput.y + cameraRight * _moveInput.x).normalized;

            Vector3 targetVelocity = moveDirection * MOVE_SPEED;
            _rb.linearVelocity = new Vector3(targetVelocity.x, _rb.linearVelocity.y, targetVelocity.z);

            CurrentVelocity = _rb.linearVelocity;
        }

        private void OnFire(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if(!_canShoot || _isReloading || _currentWeapon == null)
                {
                    return;
                }

                _fireCts = new CancellationTokenSource();
                CancellationTokenSource linkedCts =
                    CancellationTokenSource.CreateLinkedTokenSource(_fireCts.Token, this.GetCancellationTokenOnDestroy());

                switch ((FireType)_currentWeapon.WeaponFireType)
                {
                    case FireType.SemiAuto:
                        ShootSemiAutoAsync(this.GetCancellationTokenOnDestroy()).Forget();
                        break;
                    case FireType.Burst:
                        ShootBurstAsync(this.GetCancellationTokenOnDestroy()).Forget();
                        break;
                    case FireType.FullAuto:
                        ShootFullAutoAsync(linkedCts.Token).Forget();
                        break;
                    default:
                        Debug.LogWarning($"割り当てていない射撃タイプがあります({_currentWeapon.WeaponFireType})。");
                        break;
                }
            }

            if (context.canceled)
            {
                _fireCts?.Cancel();
                _fireCts?.Dispose();
                _fireCts = null;
            }
        }

        private async UniTaskVoid ShootSemiAutoAsync(CancellationToken token)
        {
            if(CurrentAmmo == 0)
            {
                ReloadAsync().Forget();
                return;
            }

            _canShoot = false;

            CurrentAmmo--;
            Debug.Log($"セミオートで撃った！残弾数: {CurrentAmmo}");
            Shoot();

            await UniTask.Delay(TimeSpan.FromSeconds(_currentWeapon.FireRate), cancellationToken: token);

            _canShoot = true;
        }

        private async UniTaskVoid ShootBurstAsync(CancellationToken token)
        {
            _canShoot = false;

            for(int i = 0; i < 3; i++)
            {
                if(CurrentAmmo <= 0)
                {
                    ReloadAsync().Forget();
                    break;
                }

                CurrentAmmo--;
                Shoot();
                Debug.Log($"バースト！残弾数: {CurrentAmmo}");

                await UniTask.Delay(TimeSpan.FromSeconds(_currentWeapon.FireInterval), cancellationToken: token);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_currentWeapon.FireRate), cancellationToken: token);
            _canShoot = true;
        }

        private async UniTaskVoid ShootFullAutoAsync(CancellationToken token)
        {
            _canShoot = false;

            while (!token.IsCancellationRequested)
            {
                if(CurrentAmmo <= 0)
                {
                    ReloadAsync().Forget();
                    break;
                }

                CurrentAmmo--;
                Debug.Log($"フルオート！残弾数: {CurrentAmmo}");
                Shoot();

                bool isCanceled =
                    await UniTask.Delay(TimeSpan.FromSeconds(_currentWeapon.FireInterval), cancellationToken: token).SuppressCancellationThrow();

                if (isCanceled)
                {
                    break;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(_currentWeapon.FireRate), cancellationToken: this.GetCancellationTokenOnDestroy());

                _canShoot = true;
            }
        }

        private void Shoot()
        {
            Ray ray = new Ray(_mainCameraTransform.position, _mainCameraTransform.forward);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, ATTACK_RANGE))
            {
                Debug.Log($"{hitInfo.collider.name}に命中！");

                IDamageable target = hitInfo.collider.GetComponent<IDamageable>();

                if (target != null)
                {
                    target.TakeDamage(_currentWeapon.AttackPower);
                }
            }
        }

        private void OnReload(InputAction.CallbackContext context)
        {
            if(_isReloading || CurrentAmmo == _currentWeapon.MaxAmmo)
            {
                return;
            }

            ReloadAsync().Forget();
        }

        private async UniTask ReloadAsync()
        {
            _isReloading = true;
            Debug.Log("リロード中");

            await UniTask.Delay(TimeSpan.FromSeconds(_currentWeapon.ReloadTime), cancellationToken: this.GetCancellationTokenOnDestroy());

            CurrentAmmo = _currentWeapon.MaxAmmo;
            _isReloading = false;
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
