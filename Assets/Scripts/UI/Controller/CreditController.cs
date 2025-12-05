using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LoopLegacy.UI.Controller
{
    public class CreditController : MonoBehaviour
    {
        [SerializeField]
        private Button _closeButton;
        private Action _onClose;
        private InputAction _quitApplicationAction;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
            _quitApplicationAction = InputSystem.actions.FindActionMap("UI").FindAction("Cancel");
        }

        void Update()
        {
            if (_quitApplicationAction.triggered &&
                !ConfirmationController.Instance.IsVisible)
            {
                Hide();
            }
        }
        public void Show(Action onClose)
        {
            gameObject.SetActive(true);
            _onClose = onClose;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _onClose?.Invoke();
            _onClose = null;
        }

        private void OnCloseButtonClicked()
        {
            Hide();
        }
    }
}
