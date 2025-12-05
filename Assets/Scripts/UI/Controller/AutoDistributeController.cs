using System;
using LoopLegacy.State;
using LoopLegacy.UI.Component;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LoopLegacy.UI.Controller
{
    public class AutoDistributeController : MonoBehaviour
    {
        [SerializeField] private ToggleSwitch _onOffSwitch;
        [SerializeField] private TMP_InputField[] _inputFields;
        [SerializeField] private Button[] _presetButtons;
        [SerializeField] private Button _applyButton;
        [SerializeField] private Button _cancelButton;

        private int _tempPresetIndex;
        private bool _isDirty = false;

        private Action _onClose;
        private InputAction _quitApplicationAction;
        
        void Start()
        {
            _onOffSwitch.onToggleOn.AddListener(OnOnButtonClicked);
            _onOffSwitch.onToggleOff.AddListener(OnOffButtonClicked);
            _applyButton.onClick.AddListener(OnApplyButtonClicked);
            _cancelButton.onClick.AddListener(OnCancelButtonClicked);
            foreach (var inputField in _inputFields)
            {
                inputField.onValueChanged.AddListener(_ => _isDirty = true);
            }
            for (int i = 0; i < _presetButtons.Length; i++)
            {
                int index = i;
                _presetButtons[i].onClick.AddListener(() => OnPresetButtonClicked(index));
            }
        }

        public void Show(Action onClose)
        {
            _isDirty = false;
            gameObject.SetActive(true);
            _onOffSwitch.SetValue(PersistentGameState.Instance.AutoDistributeStats);
            for (int i = 0; i < PersistentGameState.Instance.GetAutoDistributeRate().Length; i++)
            {
                _inputFields[i].text = PersistentGameState.Instance.GetAutoDistributeRate()[i].ToString();
            }
            _tempPresetIndex = PersistentGameState.Instance.GetCurrentAutoDistributePreset();
            for (int i = 0; i < _presetButtons.Length; i++)
            {
                _presetButtons[i].interactable = i != _tempPresetIndex;
            }
            _onClose = onClose;
            _quitApplicationAction = InputSystem.actions.FindActionMap("UI").FindAction("Cancel");
        }

        void Update()
        {
            if (_quitApplicationAction.triggered && !ConfirmationController.Instance.IsVisible)
            {
                Hide();
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _onClose?.Invoke();
            _onClose = null;
        }

        private void OnOnButtonClicked()
        {
            _isDirty = true;
        }

        private void OnOffButtonClicked()
        {
            _isDirty = true;
        }

        private void OnApplyButtonClicked()
        {
            PersistentGameState.Instance.SetAutoDistributeStats(_onOffSwitch.CurrentValue);
            int[] autoDistributeRate = new int[Enum.GetValues(typeof(StatType)).Length];
            for (int i = 0; i < autoDistributeRate.Length; i++)
            {
                if (int.TryParse(_inputFields[i].text, out int parsed))
                {
                    autoDistributeRate[i] = Mathf.Max(0, parsed);
                }
                else
                {
                    autoDistributeRate[i] = 0;
                }
            }
            PersistentGameState.Instance.SetAutoDistributePreset(_tempPresetIndex);
            PersistentGameState.Instance.SetAutoDistributeRate(_tempPresetIndex, autoDistributeRate);
            Hide();
        }

        private void OnCancelButtonClicked()
        {
            if (_isDirty)
            {
                ConfirmationController.Instance.ShowConfirmation(
                    Utils.GetUIString("auto-distribution-cancel-confirmation"),
                    () => { Hide(); });
            }
            else
            {
                Hide();
            }
        }

        private void OnPresetButtonClicked(int index)
        {
            _tempPresetIndex = index;
            for (int i = 0; i < _inputFields.Length; i++)
            {
                _inputFields[i].text = PersistentGameState.Instance.GetAutoDistributeRate(_tempPresetIndex)[i].ToString();
            }
            for (int i = 0; i < _presetButtons.Length; i++)
            {
                _presetButtons[i].interactable = i != _tempPresetIndex;
            }
            _isDirty = true;
        }
    }
}