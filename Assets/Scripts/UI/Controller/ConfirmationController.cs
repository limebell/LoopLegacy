using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using LoopLegacy.Battle;
using LoopLegacy.Loader;
using LoopLegacy.State;
using UnityEngine.InputSystem;

namespace LoopLegacy.UI.Controller
{
    public class ConfirmationController : MonoBehaviour
    {
        public static ConfirmationController Instance;

        public bool IsVisible => gameObject.activeSelf;

        [SerializeField]
        private TextMeshProUGUI _messageText;
        [SerializeField]
        private Button _confirmButton;
        [SerializeField]
        private Button _closeButton;
        private Action _onConfirm;
        private Action _onClose;
        private InputAction _quitApplicationAction;

        void Awake()
        {
            Instance = this;
            Hide();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _confirmButton.onClick.AddListener(OnConfirmButtonClick);
            _closeButton.onClick.AddListener(OnCloseButtonClick);
            _quitApplicationAction = InputSystem.actions.FindActionMap("UI").FindAction("Cancel");
        }

        void Update()
        {
            if (_quitApplicationAction.triggered && IsVisible)
            {
                OnCloseButtonClick();
            }
        }

        public void ShowConfirmation(string message, Action onConfirm, Action onClose = null, string confirmButtonText = "", string closeButtonText = "")
        {
            gameObject.SetActive(true);
            _confirmButton.gameObject.SetActive(true);
            string defaultConfirmButtonText = Utils.GetUIString("confirm");
            string defaultCloseButtonText = Utils.GetUIString("close");
            _confirmButton.GetComponentInChildren<TextMeshProUGUI>().text =
                confirmButtonText == "" ? defaultConfirmButtonText : confirmButtonText;
            _closeButton.GetComponentInChildren<TextMeshProUGUI>().text =
                closeButtonText == "" ? defaultCloseButtonText : closeButtonText;
            _messageText.text = message;
            _onConfirm = onConfirm;
            _onClose = onClose;
        }

        public void ShowWarning(string message, Action onClose = null, string closeButtonText = "")
        {
            gameObject.SetActive(true);
            _confirmButton.gameObject.SetActive(false);
            string defaultCloseButtonText = Utils.GetUIString("close");
            _closeButton.GetComponentInChildren<TextMeshProUGUI>().text =
                closeButtonText == "" ? defaultCloseButtonText : closeButtonText;
            _messageText.text = message;
            _onClose = onClose;
        }

        public void ShowEquipment(EquipmentData equipment)
        {
            gameObject.SetActive(true);
            _confirmButton.gameObject.SetActive(false);
            string defaultCloseButtonText = Utils.GetUIString("close");
            int ownedCount = PersistentGameState.Instance.InventoryState.GetOwnedEquipments(equipment.type)[equipment.id];
            _closeButton.GetComponentInChildren<TextMeshProUGUI>().text = defaultCloseButtonText;
            _messageText.text = $"<size=120%>[{Utils.GetEquipmentName(equipment.type, equipment.id)}]</size>\n";
            _messageText.text += $"{Utils.GetUIString("owned-count")}: {ownedCount}\n";
            _messageText.text += $"{equipment.baseValue} (+ {equipment.multiplier}%)\n\n";
            _messageText.text += $"{Utils.GetEquipmentDescription(equipment.type, equipment.id)}";
            _onClose = () => Hide();
        }

        public void ShowRelic(Relic relic)
        {
            gameObject.SetActive(true);
            _confirmButton.gameObject.SetActive(false);
            string defaultCloseButtonText = Utils.GetUIString("close");
            _closeButton.GetComponentInChildren<TextMeshProUGUI>().text = defaultCloseButtonText;
            _messageText.text = $"<size=120%>[{relic.Effect.GetName()} Lv. {relic.Level + 1}]</size>\n";
            _messageText.text += $"<color={Utils.GetRelicGradeColorHex(relic.Grade)}>{Utils.GetUIString("grade_" + relic.Grade.ToString().ToLowerInvariant())}</color>\n\n";
            _messageText.text += relic.Effect.GetDescription();
            _onClose = () => Hide();
        }

        public void ShowRelicDiscard(Relic relic, Action onDiscard)
        {
            gameObject.SetActive(true);
            _confirmButton.gameObject.SetActive(true);
            string defaultConfirmButtonText = Utils.GetUIString("discard");
            _confirmButton.GetComponentInChildren<TextMeshProUGUI>().text = defaultConfirmButtonText;
            string defaultCloseButtonText = Utils.GetUIString("close");
            _closeButton.GetComponentInChildren<TextMeshProUGUI>().text = defaultCloseButtonText;
            _messageText.text = $"<size=120%>[{relic.Effect.GetName()} Lv. {relic.Level + 1}]</size>\n";
            _messageText.text += $"<color={Utils.GetRelicGradeColorHex(relic.Grade)}>{Utils.GetUIString("grade_" + relic.Grade.ToString().ToLowerInvariant())}</color>\n\n";
            _messageText.text += relic.Effect.GetDescription();
            _onConfirm = onDiscard;
            _onClose = () => Hide();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnConfirmButtonClick()
        {
            Hide();
            try
            {
                _onConfirm?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void OnCloseButtonClick()
        {
            Hide();
            try
            {
                _onClose?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
