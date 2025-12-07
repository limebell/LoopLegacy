using LoopLegacy.Loader;
using LoopLegacy.Manager;
using LoopLegacy.Player;
using LoopLegacy.State;
using LoopLegacy.UI.Component;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LoopLegacy.UI.Controller
{
    public class GameplayHUDController : MonoBehaviour
    {
        [Header("Controllers")]
        [SerializeField] private MenuController _menuController;
        [SerializeField] private StatsController _statsController;
        public StatsController StatsController => _statsController;

        [Header("Controller")]
        [SerializeField] private MovementController _movementController;

        [Header("Buttons")]
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _statsButton;
        public RectTransform StatsButtonRect => _statsButton != null ? _statsButton.GetComponent<RectTransform>() : null;
        [SerializeField] private GameObject _statsRedDot;
        [SerializeField] private Button _encounterButton;

        [Space(10f)]
        [SerializeField] private TMP_Text _goldLabel;
        [SerializeField] private Gauge _encounterGauge;
        public RectTransform EncounterGaugeRect => _encounterGauge != null ? _encounterGauge.GetComponent<RectTransform>() : null;
        [SerializeField] private TMP_Text _levelLabel;
        [SerializeField] private TMP_Text _regionLabel;
        [SerializeField] private TMP_Text _effectLabel;
        [SerializeField] private TMP_Text _battlePointLabel;
        [SerializeField] private GameObject _warningElement;
        private InputAction _quitApplicationAction;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _encounterButton.interactable = PersistentGameState.Instance.HouseState.GetUpgradeLevel(UpgradeType.InstantEncounter) > 0;
            _encounterButton.onClick.AddListener(OnEncounterButtonClicked);
            _menuButton.onClick.AddListener(OnMenuButtonClicked);
            _statsButton.onClick.AddListener(OnStatsButtonClicked);

            _warningElement.SetActive(false);

            // GameState 상태 구독 설정
            SubscribeToGameState();
            SubscribeToControllerInput();
            _quitApplicationAction = InputSystem.actions.FindActionMap("UI").FindAction("Cancel");
        }

        void Update()
        {
            if (_quitApplicationAction.triggered &&
                !ConfirmationController.Instance.IsVisible &&
                !GameManager.Instance.IsMovingMap.Value &&
                !ScriptManager.Instance.IsScriptPlaying &&
                BattleManager.Instance.CurrentBattleStep.Value == BattleStep.None &&
                !IsMenuVisible())
            {
                OnMenuButtonClicked();
            }
        }

        private bool IsMenuVisible()
        {
            return (GameObject.Find("MenuPanel")?.gameObject.activeSelf ?? false) ||
                   (GameObject.Find("StatsPanel")?.gameObject.activeSelf ?? false) ||
                   (GameObject.Find("AutoDistributePanel")?.gameObject.activeSelf ?? false) ||
                   (GameObject.Find("EquipmentsPanel")?.gameObject.activeSelf ?? false) ||
                   (GameObject.Find("CodexPanel")?.gameObject.activeSelf ?? false) ||
                   (GameObject.Find("OptionPanel")?.gameObject.activeSelf ?? false);
        }

#region Tutorial
        public void ShowEncounterGauge()
        {
            _encounterGauge.gameObject.SetActive(true);
        }

        public void HideEncounterGauge()
        {
            _encounterGauge.gameObject.SetActive(false);
        }
#endregion

        private void SubscribeToGameState()
        {
            GameManager.Instance.RegionDetector.LastRegion.Subscribe(region =>
            {
                UpdateRegionLabel(region);
            });

            var d = Disposable.CreateBuilder();
            // 엔카운터 게이지 구독
            GameManager.Instance.EncounterManager.EncounterGauge
                .Subscribe(gauge => 
                {
                    _encounterGauge.SetValue(gauge / 100f, true);
                })
                .AddTo(ref d);

            GameManager.Instance.GameState.AppliedRegionEffects
                .Subscribe(appliedRegionEffects =>
                {
                    UpdateRegionLabel();
                })
                .AddTo(ref d);

            // 플레이어 레벨 구독
            GameManager.Instance.GameState.PlayerStats.Level
                .Subscribe(level => 
                {
                    _levelLabel.text = $"{level:N0}";
                    UpdateRegionLabel();
                })
                .AddTo(ref d);

            GameManager.Instance.GameState.PlayerStats.StatPoints
                .Subscribe(statPoints =>
                {
                    _statsRedDot.SetActive(statPoints != 0);
                })
                .AddTo(ref d);

            // TODO: 새 장비를 획득했을 때 스탯 버튼에 빨간 점 표시

            // 골드 구독
            PersistentGameState.Instance.Gold
                .Subscribe(gold => 
                {
                    _goldLabel.text = $"{gold:N0}";
                })
                .AddTo(ref d);

            // 전투 포인트 구독
            GameManager.Instance.GameState.PlayerStats.BattlePoint
                 .Subscribe(battlePoint => 
                 {
                    _battlePointLabel.text = $"{battlePoint}";
                 })
                 .AddTo(ref d);
            d.RegisterTo(this.destroyCancellationToken);
        }

        private void SubscribeToControllerInput()
        {
            var d = Disposable.CreateBuilder();
            _movementController.Direction.Subscribe(direction => {
                        PlayerMovement.Instance.SetVector2Input(direction);
                })
                .AddTo(ref d);
            d.RegisterTo(this.destroyCancellationToken);
        }

        private void UpdateRegionLabel()
        {
            UpdateRegionLabel(GameManager.Instance.RegionDetector.LastRegion.Value);
        }

        private void UpdateRegionLabel(Region.Region region)
        {
            if (region != null)
            {
                var entry = region.GetRegionEntry();
                if (entry != null)
                {
                    if (int.TryParse(entry.label, out int id))
                    {
                        _regionLabel.text = $"{id:N0}";
                    }
                    else
                    {
                        _regionLabel.text = $"{entry.label}";
                    }

                    _regionLabel.color = region.GetColor(
                        GameManager.Instance.GameState.PlayerStats.Level.Value);
                    
                    if (GameManager.Instance.GameState.GetRegionEffect(entry.code).Type != RegionEffectType.None)
                    {
                        var regionEffect = GameManager.Instance.GameState.GetRegionEffect(entry.code);
                        _effectLabel.text = $"{Utils.GetRegionEffectText(regionEffect.Type)} ({regionEffect.Duration})";
                    }
                    else
                    {
                        _effectLabel.text = "-";
                    }

                    if (region.GetRelativeLevel(GameManager.Instance.GameState.PlayerStats.Level.Value) > 0)
                    {
                        _warningElement.SetActive(true);
                    }
                    else
                    {
                        _warningElement.SetActive(false);
                    }
                }
                else
                {
                    _regionLabel.text = "Unknown Region";
                    _regionLabel.color = Color.white;
                    _effectLabel.text = "-";
                }
            }
            else
            {
                _regionLabel.text = "-";
                _regionLabel.color = Color.white;
                _effectLabel.text = "-";
                _warningElement.SetActive(false);
            }
        }

        private void OnEncounterButtonClicked()
        {
            GameManager.Instance.EncounterManager.TriggerEncounter();
        }

        private void OnMenuButtonClicked()
        {
            _menuController.Show();
        }

        private void OnStatsButtonClicked()
        {
            _statsController.Show(null);
        }
    }
}
