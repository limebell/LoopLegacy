using LoopLegacy.State;
using LoopLegacy.UI.Component;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace LoopLegacy.UI.Controller
{
    public class OptionController : MonoBehaviour
    {
        [SerializeField]
        private Button _resetButton;
        [SerializeField]
        private Button _closeButton;
        private Action _onClose;

        // Languages
        private Dictionary<string, string> _languageMap = new Dictionary<string, string>();
        [SerializeField]
        private TMP_Dropdown _languageDropdown;

        // Combat Speed
        private const int COMBAT_SPEED_COUNT = 3;
        [SerializeField]
        private Button[] _combatSpeedButtons = new Button[COMBAT_SPEED_COUNT];

        // Volume
        [SerializeField]
        private VolumeComponent _masterVolumeSlider;
        [SerializeField]
        private VolumeComponent _bgmVolumeSlider;
        [SerializeField]
        private VolumeComponent _sfxVolumeSlider;

        // Frame Rate
        private const int FRAME_RATE_COUNT = 3;
        [SerializeField]
        private Button[] _frameRateButtons;

        // Quality
        private const int QUALITY_COUNT = 3;
        [SerializeField]
        private Button[] _qualityButtons;

        // Control
        private const int CONTROL_COUNT = 3;
        [SerializeField]
        private Button[] _controlButtons;

        void Awake()
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            _languageMap.Clear();
            _languageDropdown.options.Clear();
            foreach (var locale in locales)
            {
                string localizedString = Utils.GetUIString("language_" + locale.Identifier.Code);
                _languageMap.Add(locale.Identifier.Code, localizedString);
                _languageDropdown.options.Add(new TMP_Dropdown.OptionData(localizedString));
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _controlButtons[0].onClick.AddListener(() => OptionState.Instance.Control.Value = ControlType.Touch);
            _controlButtons[1].onClick.AddListener(() => OptionState.Instance.Control.Value = ControlType.Static);
            _controlButtons[2].onClick.AddListener(() => OptionState.Instance.Control.Value = ControlType.Dynamic);
            _masterVolumeSlider.onValueChanged.AddListener((value) => OptionState.Instance.MasterVolume.Value = value);
            _bgmVolumeSlider.onValueChanged.AddListener((value) => OptionState.Instance.BgmVolume.Value = value);
            _sfxVolumeSlider.onValueChanged.AddListener((value) => OptionState.Instance.SfxVolume.Value = value);

            _languageDropdown.onValueChanged.AddListener((value) =>
            {
                _languageDropdown.value = value;
                OptionState.Instance.Language.Value = _languageMap.Keys.ElementAt(value);
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(_languageMap.Keys.ElementAt(value));
            });

            for (int i = 0; i < COMBAT_SPEED_COUNT; i++)
            {
                int index = i;
                _combatSpeedButtons[index].onClick.AddListener(() => OptionState.Instance.CombatSpeed.Value = index);
            }

            for (int i = 0; i < FRAME_RATE_COUNT; i++)
            {
                int index = i;
                _frameRateButtons[index].onClick.AddListener(() =>
                {
                    OptionState.Instance.FrameRate.Value = index;
                    ApplyQualitySettings();
                });
            }

            for (int i = 0; i < QUALITY_COUNT; i++)
            {
                int index = i;
                _qualityButtons[index].onClick.AddListener(() =>
                {
                    OptionState.Instance.Quality.Value = index;
                    ApplyQualitySettings();
                });
            }

            _masterVolumeSlider.onValueChanged.AddListener((value) => OptionState.Instance.MasterVolume.Value = value);
            _bgmVolumeSlider.onValueChanged.AddListener((value) => OptionState.Instance.BgmVolume.Value = value);
            _sfxVolumeSlider.onValueChanged.AddListener((value) => OptionState.Instance.SfxVolume.Value = value);

            _resetButton.onClick.AddListener(() =>
            {
                string message = Utils.GetUIString("reset-options-confirmation");
                ConfirmationController.Instance.ShowConfirmation(
                    message,
                    () =>
                    {
                        OptionState.Instance.Reset();
                        ApplyQualitySettings();
                        UpdateOptionValues();
                    });
            });

            _closeButton.onClick.AddListener(() => Hide());
            RegisterOptionValues();
        }
        
        public void Show(Action onClose)
        {
            _onClose = onClose;
            gameObject.SetActive(true);
            UpdateOptionValues();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            OptionState.Instance.SaveData();
            _onClose?.Invoke();
        }

        private void ApplyQualitySettings()
        {
            QualitySettings.SetQualityLevel(OptionState.Instance.Quality.Value + 1);
            Application.targetFrameRate = OptionState.Instance.FrameRate.Value switch
            {
                0 => 30,
                1 => 48,
                2 => 60,
                _ => 30
            };
        }

        private void UpdateOptionValues()
        {
            int languageDropdownIndex = _languageMap.Keys.ToList().IndexOf(OptionState.Instance.Language.Value);
            Debug.Log($"[UpdateOptionValues] {OptionState.Instance.Language.Value} {languageDropdownIndex}");
            _languageDropdown.value = languageDropdownIndex == -1 ? 0 : languageDropdownIndex;
            for (int i = 0; i < COMBAT_SPEED_COUNT; i++)
                _combatSpeedButtons[i].interactable = true;
            _combatSpeedButtons[OptionState.Instance.CombatSpeed.Value].interactable = false;
            for (int i = 0; i < CONTROL_COUNT; i++)
                _controlButtons[i].interactable = true;
            int controTypeIndex = OptionState.Instance?.Control.Value switch
            {
                ControlType.Touch => 0,
                ControlType.Static => 1,
                ControlType.Dynamic => 2,
                _ => 0
            };
            _controlButtons[controTypeIndex].interactable = false;
            for (int i = 0; i < FRAME_RATE_COUNT; i++)
                _frameRateButtons[i].interactable = true;
            _frameRateButtons[OptionState.Instance.FrameRate.Value].interactable = false;
            for (int i = 0; i < QUALITY_COUNT; i++)
                _qualityButtons[i].interactable = true;
            _qualityButtons[OptionState.Instance.Quality.Value].interactable = false;
            _masterVolumeSlider.value = OptionState.Instance.MasterVolume.Value;
            _bgmVolumeSlider.value = OptionState.Instance.BgmVolume.Value;
            _sfxVolumeSlider.value = OptionState.Instance.SfxVolume.Value;
        }

        private void RegisterOptionValues()
        {
            var d = Disposable.CreateBuilder();
            OptionState.Instance.CombatSpeed.Subscribe(value => {
                for (int i = 0; i < COMBAT_SPEED_COUNT; i++)
                    _combatSpeedButtons[i].interactable = true;
                _combatSpeedButtons[value].interactable = false;
            }).AddTo(ref d);
            OptionState.Instance.Control.Subscribe(value => {
                for (int i = 0; i < CONTROL_COUNT; i++)
                    _controlButtons[i].interactable = true;
                int controTypeIndex = value switch
                {
                    ControlType.Touch => 0,
                    ControlType.Static => 1,
                    ControlType.Dynamic => 2,
                    _ => 0
                };
                _controlButtons[controTypeIndex].interactable = false;
            }).AddTo(ref d);
            OptionState.Instance.FrameRate.Subscribe(value => {
                for (int i = 0; i < FRAME_RATE_COUNT; i++)
                    _frameRateButtons[i].interactable = true;
                _frameRateButtons[value].interactable = false;
            }).AddTo(ref d);
            OptionState.Instance.Quality.Subscribe(value => {
                for (int i = 0; i < QUALITY_COUNT; i++)
                    _qualityButtons[i].interactable = true;
                _qualityButtons[value].interactable = false;
            }).AddTo(ref d);
            d.RegisterTo(this.destroyCancellationToken);
        }
    }
}
