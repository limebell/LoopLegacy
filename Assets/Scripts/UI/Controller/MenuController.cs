using LoopLegacy.Manager;
using LoopLegacy.State;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LoopLegacy.UI.Controller
{
    public class MenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField]
        private Button _statsButton;
        [SerializeField]
        private Button _codexButton;
        [SerializeField]
        private Button _optionButton;   
        [SerializeField]        
        private Button _giveUpButton;
        [SerializeField]
        private Button _titleButton;
        [SerializeField]
        private Button _closeButton;

        // 하위 메뉴 컨트롤러
        [Header("Controllers")]
        [SerializeField] public StatsController statsController;
        [SerializeField] public CodexController codexController;
        [SerializeField] public OptionController optionController;

        private InputAction _quitApplicationAction;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            // UI 요소 찾기 및 디버깅
            _statsButton.onClick.AddListener(OnStatsButtonClicked);
            _codexButton.onClick.AddListener(OnCodexButtonClicked);
            _optionButton.onClick.AddListener(OnOptionButtonClicked);
            _giveUpButton.onClick.AddListener(OnGiveUpButtonClicked);
            _titleButton.onClick.AddListener(OnTitleButtonClicked);
            _closeButton.onClick.AddListener(OnCloseButtonClicked);

            if (PersistentGameState.Instance.IsInGame)
            {
                _giveUpButton.gameObject.SetActive(true);
            }
            else
            {
                _giveUpButton.gameObject.SetActive(false);
            }

            var uiActionMap = InputSystem.actions.FindActionMap("UI");
            _quitApplicationAction = uiActionMap.FindAction("Cancel");
        }

        void Update()
        {
            if (_quitApplicationAction.triggered)
            {
                if (gameObject.activeSelf)
                {
                    Hide();
                }
                else
                {
                    Show();
                }
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _codexButton.gameObject.SetActive(PersistentGameState.Instance.HouseState.CanViewCodex);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnStatsButtonClicked()
        {
            Hide();
            statsController.Show(Show);
        }

        private void OnCodexButtonClicked()
        {
            Hide();
            codexController.Show(Show);
        }

        private void OnOptionButtonClicked()
        {
            Hide();
            optionController.Show(Show);
        }

        private void OnGiveUpButtonClicked()
        {
            if (PersistentGameState.Instance.IsInGame)
            {
                ConfirmationController.Instance.ShowConfirmation(
                    Utils.GetUIString("giveup-confirmation"),
                    OnGiveUpConfirmButtonClicked);
            }
            else
            {
                ConfirmationController.Instance.ShowWarning(
                    Utils.GetUIString("Error"));
            }
        }

        private void OnTitleButtonClicked()
        {
            string message = Utils.GetUIString("back-to-title-confirmation");
            ConfirmationController.Instance.ShowConfirmation(
                message,
                OnTitleConfirmButtonClicked);
        }

        private void OnGiveUpConfirmButtonClicked()
        {
            if (PersistentGameState.Instance.IsInGame)
            {
                Hide();
                GameManager.Instance.GameState.IsAdvertised = true;
                GameManager.Instance.GameState.PlayerStats.BattlePoint.Value = 0;
            }
        }

        private void OnTitleConfirmButtonClicked()
        {
            if (PersistentGameState.Instance.IsInGame)
            {
                GameManager.Instance.Save();
                GameEssentials.Instance?.DestroyEssentials();
            }
            else if (PersistentGameState.Instance.IsInGame)
            {
                PersistentGameState.Instance.SaveState();
            }

            FadeController.LoadScene("Title");
        }

        private void OnCloseButtonClicked()
        {
            Hide();
        }
    }
}
