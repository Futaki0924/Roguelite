using TPSRoguelite.InGame.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TPSRoguelite.UI
{
    public class TitlePresenter : MonoBehaviour
    {
        const string IN_GAME_SCENE_NAME = "InGameScene";

        [SerializeField] TitleView _titleView;
        TitleModel _titleModel;

        private void Start()
        {
            if (_titleView == null)
            {
                return;
            }

            _titleModel = new();
            _titleModel.Initialize();

            _titleView.OnGameStart += StartGame;
        }

        private void OnDestroy()
        {
            if(_titleView != null)
            {
                _titleView.OnGameStart -= StartGame;
            }
        }

        private void StartGame()
        {
            if (GameManager.Instance != null)
            {
                Destroy(GameManager.Instance.gameObject);
            }

            SceneManager.LoadScene(IN_GAME_SCENE_NAME);
        }
    }
}