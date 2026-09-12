using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AI;
using System.Threading;
using System.Collections.Generic;
using TPSRoguelite.InGame.Enemy;
using Core.MasterData;

namespace TPSRoguelite.InGame.Spawner
{
    public class EnemySpawner : MonoBehaviour
    {
        const float SPAWN_INTERVAL = 1f;
        const float MAX_SPAWN_DISTANCE = 2f;
        const int POOL_SIZE = 20;

        [SerializeField] GameObject _enemyPrefab;

        [SerializeField] Transform[] _spawnPoints;

        private Queue<EnemyState> _enemyPool = new Queue<EnemyState>();

        public void SetUp()
        {
            if (_enemyPrefab == null)
            {
                return;
            }

            for (int i = 0; i < POOL_SIZE; i++)
            {
                GameObject enemyObj = Instantiate(_enemyPrefab);
                EnemyState enemy = enemyObj.GetComponent<EnemyState>();
                if (enemy != null)
                {
                    ulong randomId = (ulong)Random.Range(1, MasterDataAccessor.Instance.Count<EnemyDataRecord>());
                    enemy.Initialize(randomId);
                    enemy.gameObject.SetActive(false);
                    _enemyPool.Enqueue(enemy);
                }
            }

            SpawnLoopAsync().Forget();
        }

        private async UniTaskVoid SpawnLoopAsync()
        {
            CancellationToken token = this.GetCancellationTokenOnDestroy();

            while (true)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(SPAWN_INTERVAL), cancellationToken: token);
                SpawnEnemyFromPool();
            }
        }

        private void SpawnEnemyFromPool()
        {
            if(_enemyPrefab == null || _spawnPoints.Length == 0)
            {
                return;
            }

            int randomIndex = Random.Range(0, _spawnPoints.Length);
            Transform spawnPoint = _spawnPoints[randomIndex];

            Vector3 safePosition = spawnPoint.position;
            if(NavMesh.SamplePosition(spawnPoint.position, out NavMeshHit hit, MAX_SPAWN_DISTANCE, NavMesh.AllAreas))
            {
                safePosition = hit.position;
            }
            else
            {
                Debug.LogWarning("近くに安全なスポーン位置が見つかりませんでした");
                return;
            }

            EnemyState enemy;

            if(_enemyPool.Count > 0)
            {
                enemy = _enemyPool.Dequeue();
            }
            else
            {
                Debug.LogWarning("プールに空きがなかったため、Instantiateで生成します。プールのサイズを増やすか、生成に制限をかけてください");
                GameObject enemyObj = Instantiate(_enemyPrefab);
                enemy = enemyObj.GetComponent<EnemyState>();
                if(enemy == null)
                {
                    Debug.LogError("EnemyStateの取得に失敗しました");
                    return;
                }
            }

            enemy.OnReturnToPoolAction -= ReturnToPool;
            enemy.OnReturnToPoolAction += ReturnToPool;

            enemy.transform.position = safePosition;
            enemy.transform.rotation = spawnPoint.rotation;

            enemy.SetUp();
        }

        private void ReturnToPool(EnemyState enemy)
        {
            _enemyPool.Enqueue(enemy);
            enemy.OnReturnToPoolAction -= ReturnToPool;
        }
    }
}
