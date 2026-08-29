using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Core.MasterData;
using TPSRoguelite.InGame.Player;
using System;

namespace TPSRoguelite.InGame.Manager
{
    [Serializable]
    public class SkillButtonUI
    {
        public Button Button;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI DectText;
    }

    public class LevelUpManager : MonoBehaviour
    {
        public static LevelUpManager Instance { get; private set; }

        [Header("UI設定")]
        [SerializeField] GameObject _skillSelectPanel;
        [SerializeField] SkillButtonUI[] _skillButtons = new SkillButtonUI[3];

        PlayerInputActions _inputActions;
        PlayerController _playerController;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Time.timeScale = 1f;

            if(_skillSelectPanel != null)
            {
                _skillSelectPanel.SetActive(false);
            }
        }

        public void OnLevelUp(PlayerInputActions currentInput, PlayerController player)
        {
            _inputActions = currentInput;
            _playerController = player;

            var allSkills = MasterDataAccessor.Instance.GetAll<SkillDataRecord>();
            var chosenSkills = allSkills.OrderBy(v => Guid.NewGuid()).Take(3).ToList();

            for(int i = 0; i < 3; i++)
            {
                SkillDataRecord skill = chosenSkills[i];
                SkillButtonUI ui = _skillButtons[i];

                ui.NameText.text = skill.SkillName;
                ui.DectText.text = skill.Description;

                ui.Button.onClick.RemoveAllListeners();
                ui.Button.onClick.AddListener(() => OnSkillSelected(skill));
            }

            if(_skillSelectPanel != null)
            {
                _skillSelectPanel.SetActive(true);
            }

            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if(_inputActions != null)
            {
                _inputActions.Player.Disable();
            }
        }

        private void OnSkillSelected(SkillDataRecord selectedSkill)
        {
            if(_playerController != null)
            {
                _playerController.ApplySkill(selectedSkill);
            }

            if(_skillSelectPanel != null)
            {
                _skillSelectPanel.SetActive(false);
            }

            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if(_inputActions != null)
            {
                _inputActions.Player.Enable();
            }
        }
    }
}