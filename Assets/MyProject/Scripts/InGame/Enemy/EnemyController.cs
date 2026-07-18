using UnityEngine;
using UnityEngine.AI;

namespace TPSRoguelite.InGame.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        const string PLAYER_TAG_NAME = "Player";

        [SerializeField] EnemyState _enemyState;

        [SerializeField] NavMeshAgent _navMeshAgent;

        Transform _targetPlayer;

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
    }
}
