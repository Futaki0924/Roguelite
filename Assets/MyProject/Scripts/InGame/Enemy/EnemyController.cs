using UnityEngine;
using UnityEngine.AI;
using System.Threading;
using Cysharp.Threading.Tasks;
using System;

namespace TPSRoguelite.InGame.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        const string PLAYER_TAG_NAME = "Player";

        const float KNOCKBACK_FORCE = 2f;
        const float KNOCKBACK_DURATION = 0.15f;

        [SerializeField] EnemyState _enemyState;

        [SerializeField] NavMeshAgent _navMeshAgent;

        Transform _targetPlayer;

        CancellationTokenSource _hitCts;

        private void Awake()
        {
            GameObject player = GameObject.FindGameObjectWithTag(PLAYER_TAG_NAME);
            if (player != null)
            {
                _targetPlayer = player.transform;
            }
            else
            {
                Debug.LogError($"{PLAYER_TAG_NAME}というタグのついたオブジェクトが見つかりませんでした");
            }

            if (_navMeshAgent != null && _enemyState != null && _enemyState.EnemyDataAsset != null)
            {
                _navMeshAgent.speed = _enemyState.EnemyDataAsset.MoveSpeed;
            }
        }

        private void Update()
        {
            if (_targetPlayer != null && _navMeshAgent != null)
            {
                _navMeshAgent.SetDestination(_targetPlayer.position);
            }
        }

        private void OnEnable()
        {
            if(_enemyState != null)
            {
                _enemyState.OnDamageAction -= HandleDamage;
                _enemyState.OnDamageAction += HandleDamage;
            }
        }

        private void OnDisable()
        {
            if(_enemyState != null)
            {
                _enemyState.OnDamageAction -= HandleDamage;
            }

            if(_navMeshAgent != null && _navMeshAgent.isActiveAndEnabled)
            {
                _navMeshAgent.isStopped = false;
            }
        }

        private async UniTaskVoid KnockbackAsync(CancellationToken token)
        {
            if(_navMeshAgent == null)
            {
                return;
            }

            bool wasStopped = _navMeshAgent.isStopped;
            _navMeshAgent.isStopped = true;

            if(_targetPlayer != null)
            {
                Vector3 dir = (transform.position - _targetPlayer.position).normalized;
                dir.y = 0f;
                transform.position += dir * KNOCKBACK_FORCE;
            }

            bool isCanceled = await UniTask.Delay(
                TimeSpan.FromSeconds(KNOCKBACK_DURATION), cancellationToken: token).SuppressCancellationThrow();

            if (!isCanceled && _navMeshAgent.isActiveAndEnabled)
            {
                _navMeshAgent.isStopped = wasStopped;
            }
        }

        private void HandleDamage()
        {
            _hitCts?.Cancel();
            _hitCts?.Dispose();
            _hitCts = null;

            _hitCts = new CancellationTokenSource();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_hitCts.Token, this.GetCancellationTokenOnDestroy());

            KnockbackAsync(linkedCts.Token).Forget();
        }
    }
}
