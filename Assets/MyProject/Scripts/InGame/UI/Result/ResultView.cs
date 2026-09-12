using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace TPSRoguelite.UI
{
    public class ResultView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _resultText;

        [SerializeField] Button _retryButton;

        [SerializeField] Button _returnToTitleButton;

        public event UnityAction OnRetryAction;
        public event UnityAction OnReturnToTitleAction;

        private void Awake()
        {
            if (_retryButton != null)
            {
                _retryButton.onClick.AddListener(() => OnRetryAction?.Invoke());
            }

            if(_returnToTitleButton != null)
            {
                _returnToTitleButton.onClick.AddListener(() => OnReturnToTitleAction?.Invoke());
            }
        }

        public void SetResultText(string text)
        {
            if(_resultText != null)
            {
                _resultText.text = text;
            }
        }
    }
}