using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TPSRoguelite.UI
{
    public class TitleView : MonoBehaviour
    {
        public event UnityAction OnGameStart;

        [SerializeField] Button _gameStartButton;

        private void Awake()
        {
            if(_gameStartButton != null)
            {
                _gameStartButton.onClick.AddListener(() => OnGameStart?.Invoke());
            }
        }
    }
}