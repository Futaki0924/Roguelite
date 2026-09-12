using UnityEngine;
using UnityEngine.SceneManagement;
using TPSRoguelite.InGame.Manager;

namespace TPSRoguelite.UI
{
    public class ResultPresenter : MonoBehaviour
    {
        const string TITLE_SCENE_NAME = "TitleScene";
        const string IN_GAME_SCENE_NAME = "InGameScene";

        [SerializeField] private ResultView _resultView;
        ResultModel _resultModel;

        private void Start()
        {
            if(_resultView == null)
            {
                return;
            }

            _resultModel = new ResultModel();
            _resultModel.Initialize();

            _resultView.OnRetryAction += RetryGame;
            _resultView.OnReturnToTitleAction += ReturnToTitle;

            string message = string.Empty;

            if (_resultModel.IsClear)
            {
                message = $"GAME CLEAR!\n\n到達レベル: {_resultModel.Level}";
            }
            else
            {
                int minutes = Mathf.FloorToInt(_resultModel.SurvivedTime / 60f);
                int seconds = Mathf.FloorToInt(_resultModel.SurvivedTime - minutes * 60f);

                message = $"GAME OVER…\n\n生存時間: {minutes: 00}:{seconds: 00}\n到達レベル: {_resultModel.Level}";
            }

            _resultView.SetResultText(message);
        }

        private void OnDestroy()
        {
            if(_resultView != null)
            {
                _resultView.OnRetryAction -= RetryGame;
                _resultView.OnReturnToTitleAction -= ReturnToTitle;
            }
        }

        private void RetryGame()
        {
            if(GameManager.Instance != null)
            {
                Destroy(GameManager.Instance.gameObject);
            }

            SceneManager.LoadScene(IN_GAME_SCENE_NAME);
        }

        private void ReturnToTitle()
        {
            SceneManager.LoadScene(TITLE_SCENE_NAME);
        }
    }
}