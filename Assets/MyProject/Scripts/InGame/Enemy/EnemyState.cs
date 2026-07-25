using Core.Interface;
using UnityEngine;
using UnityEngine.Events;
using Core.MasterData;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TPSRoguelite.InGame.Enemy
{
    public class EnemyState : MonoBehaviour, IDamageable
    {
        const float FLASH_DURATION = 0.1f;

        const float ORB_DROP_HEIGHT_OFFSET = 0.5f;

        [SerializeField] Renderer[] _modelRenderers;

        [SerializeField] GameObject _experienceOrbPrefab;

        Color[] _defaultColors;

        CancellationTokenSource _flashCts;

        public EnemyDataRecord EnemyDataAsset { get; private set; }

        public int CurrentHP { get; private set; }

        public event UnityAction<EnemyState> OnReturnToPoolAction;

        public event UnityAction OnDamageAction;

        public void Initialize(ulong id)
        {
            EnemyDataAsset = MasterDataAccessor.Instance.GetById<EnemyDataRecord>(id);

            if (_modelRenderers != null)
            {
                _defaultColors = new Color[_modelRenderers.Length];

                for (int i = 0; i < _modelRenderers.Length; i++)
                {
                    if (_modelRenderers[i] != null)
                    {
                        _defaultColors[i] = _modelRenderers[i].material.color;
                    }
                }
            }
        }

        public void SetUp()
        {
            if (EnemyDataAsset == null)
            {
                Debug.LogError("EnemyDataがセットされていません。");
                return;
            }

            CurrentHP = EnemyDataAsset.MaxHP;
            gameObject.SetActive(true);

            ResetColor();
        }

        public void TakeDamage(int damageAmount)
        {
            if (damageAmount <= 0)
            {
                return;
            }

            CurrentHP -= damageAmount;
            Debug.Log($"{EnemyDataAsset.EnemyName}に{damageAmount}のダメージ！残りHP: {CurrentHP}");

            if(CurrentHP > 0)
            {
                OnDamageAction?.Invoke();

                _flashCts?.Cancel();
                _flashCts?.Dispose();
                _flashCts = null;

                _flashCts = new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    _flashCts.Token, this.GetCancellationTokenOnDestroy());

                DamageFlashAsync(linkedCts.Token).Forget();
            }
            else
            {
                Die();
            }
        }

        private void Die()
        {
            if(_experienceOrbPrefab != null)
            {
                Vector3 spawnPosition = transform.position + Vector3.up * ORB_DROP_HEIGHT_OFFSET;
                Instantiate(_experienceOrbPrefab, spawnPosition, Quaternion.identity);
            }

            Debug.Log($"{EnemyDataAsset.EnemyName}を倒しました");
            gameObject.SetActive(false);
            OnReturnToPoolAction?.Invoke(this);
        }

        private void ResetColor()
        {
            if (_modelRenderers == null || _defaultColors == null)
            {
                return;
            }

            for (int i = 0; i < _modelRenderers.Length; i++)
            {
                if (_modelRenderers[i] != null)
                {
                    _modelRenderers[i].material.color = _defaultColors[i];
                }
            }
        }

        private async UniTaskVoid DamageFlashAsync(CancellationToken token)
        {
            if(_modelRenderers == null)
            {
                return;
            }

            foreach(var renderer in _modelRenderers)
            {
                if(renderer != null)
                {
                    renderer.material.color = Color.red;
                }
            }

            bool isCanceled = await UniTask.Delay(
                TimeSpan.FromSeconds(FLASH_DURATION), cancellationToken: token).SuppressCancellationThrow();

            if (!isCanceled)
            {
                ResetColor();
            }
        }
    }
}
