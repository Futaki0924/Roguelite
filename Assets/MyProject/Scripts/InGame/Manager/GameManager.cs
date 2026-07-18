using Cysharp.Threading.Tasks;
using TPSRoguelite.InGame.Player;
using TPSRoguelite.InGame.Spawner;
using UnityEngine;
using Core.MasterData;

namespace TPSRoguelite.InGame.Manager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] PlayerController _player = null;
        [SerializeField] EnemySpawner _enemySpawner = null;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SetUp().Forget();
        }

        private async UniTaskVoid SetUp()
        {
            await MasterDataAccessor.Instance.InitializeAsync();

            if(_player != null)
            {
                _player.SetUp();
            }

            if(_enemySpawner != null)
            {
                _enemySpawner.SetUp();
            }
        }
    }
}
