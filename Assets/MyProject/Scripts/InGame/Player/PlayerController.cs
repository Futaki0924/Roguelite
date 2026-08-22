using Core.Interface;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Core.MasterData;
using TPSRoguelite.InGame.Enum;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

namespace TPSRoguelite.InGame.Player
{
    public class PlayerController : MonoBehaviour
    {
        const float MOVE_SPEED = 8f;

        const float ROTATE_SPEED = 10f;

        const float LASER_MAX_DISTANCE = 50f;

        const float ATTACK_RANGE = 50f;

        const float LEVEL_UP_EFFECT_DURATION = 2f;

        [SerializeField] Rigidbody _rb;

        [SerializeField] Transform _weaponOrigin;

        [SerializeField] LineRenderer _laserLineRendrer;

        [SerializeField] ulong _weaponId = 1;

        [SerializeField] ParticleSystem _muzzleFlash;

        [SerializeField] TextMeshProUGUI _weaponName;

        [SerializeField] TextMeshProUGUI _ammoText;

        [SerializeField] GameObject _reloadUI;

        [SerializeField] Image _reloadCircleImage;

        [SerializeField] Slider _expBar;

        [SerializeField] TextMeshProUGUI _levelUpText;

        [SerializeField] ParticleSystem _levelUpEffect;

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

        public int CurrentExp { get; private set; }

        public int CurrentLevel { get; private set; }

        int RequiredExp => CurrentLevel * 5;

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
                UpdateWeaponUI();
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

            if(_reloadUI != null)
            {
                _reloadUI.SetActive(false);
            }

            CurrentExp = 0;
            CurrentLevel = 1;

            if(_levelUpText != null)
            {
                _levelUpText.enabled = false;
            }

            UpdateExpUI();

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
                Reload();
                return;
            }

            _canShoot = false;

            CurrentAmmo--;
            UpdateCurrentAmmoUI();
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
                    Reload();
                    break;
                }

                CurrentAmmo--;
                UpdateCurrentAmmoUI();
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
                    Reload();
                    break;
                }

                CurrentAmmo--;
                UpdateCurrentAmmoUI();
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
            if(_muzzleFlash != null)
            {
                _muzzleFlash.Play();
            }

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

            Reload();
        }

        private void Reload()
        {
            _isReloading = true;

            if(_reloadUI != null)
            {
                _reloadUI.SetActive(true);
            }

            if(_reloadCircleImage != null)
            {
                _reloadCircleImage.fillAmount = 0f;
            }

            DOVirtual.Float(0f, 1f, _currentWeapon.ReloadTime, UpdateReloadUI).SetEase(Ease.Linear).OnComplete(FinishReload);
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

        private void UpdateWeaponUI()
        {
            if(_weaponName != null)
            {
                _weaponName.SetText(_currentWeapon.WeaponName);

                switch ((FireType)_currentWeapon.WeaponFireType)
                {
                    case FireType.SemiAuto:
                        _weaponName.color = Color.plum;
                        break;
                    case FireType.Burst:
                        _weaponName.color = Color.blanchedAlmond;
                        break;
                    case FireType.FullAuto:
                        _weaponName.color = Color.aliceBlue;
                        break;
                }
            }

            UpdateCurrentAmmoUI();
        }

        private void UpdateCurrentAmmoUI()
        {
            if(_ammoText != null)
            {
                _ammoText.SetText($"{CurrentAmmo}/{_currentWeapon.MaxAmmo}");
            }
        }

        private void UpdateReloadUI(float value)
        {
            if(_reloadCircleImage != null)
            {
                _reloadCircleImage.fillAmount = value;
            }
        }

        private void FinishReload()
        {
            if(_reloadUI != null)
            {
                _reloadUI.SetActive(false);
            }

            CurrentAmmo = _currentWeapon.MaxAmmo;
            UpdateCurrentAmmoUI();
            _isReloading = false;
        }

        public void AddExp(int amount)
        {
            CurrentExp += amount;

            if(CurrentExp >= RequiredExp)
            {
                LevelUp();
            }

            UpdateExpUI();
        }

        private void UpdateExpUI()
        {
            if(_expBar != null)
            {
                _expBar.value = (float)CurrentExp / RequiredExp;
            }
        }

        private void LevelUp()
        {
            CurrentLevel++;

            CurrentExp -= RequiredExp;

            if(_levelUpEffect != null)
            {
                _levelUpEffect.Play();
            }

            ShowLevelUpTextAsync().Forget();
        }

        private async UniTaskVoid ShowLevelUpTextAsync()
        {
            if(_levelUpText == null)
            {
                return;
            }

            _levelUpText.enabled = true;
            _levelUpText.SetText($"レベルアップ！\n<size=50%>Lv.{CurrentLevel}</size>");

            await UniTask.Delay(
                TimeSpan.FromSeconds(LEVEL_UP_EFFECT_DURATION), cancellationToken: this.GetCancellationTokenOnDestroy());

            _levelUpText.enabled = false;
        }
    }
}
