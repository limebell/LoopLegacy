using LoopLegacy.Battle;
using LoopLegacy.Battle.RelicEffects;
using LoopLegacy.Manager;
using LoopLegacy.State;
using LoopLegacy.UI.Component;
using R3;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LoopLegacy.UI.Controller
{
    public class StatsController : MonoBehaviour
    {
        [Header("Stats Headaer")]
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _expText;
        [SerializeField] private TMP_Text _finalAttackText;
        [SerializeField] private TMP_Text _finalDefenseText;
        [SerializeField] private Button _expandButton;
        [SerializeField] private GameObject _statsDetailContainer;
        [SerializeField] private TMP_Text _statsDetailLabelText;
        [SerializeField] private TMP_Text _statsDetailValueText;
        [SerializeField] private Button _backgroundButton;
        [SerializeField] private Button _foldButton;

        [Header("Stats")]
        [SerializeField] private TMP_Text _autoDistributePresetText;
        [SerializeField] private Button _autoDistributeButton;
        [SerializeField] private TMP_Text _statPointsText;
        [SerializeField] private StatElement[] _statElements;

        [Header("Equipment")]
        [SerializeField] private EquipmentElement[] _equipmentElements;

        [Header("Relics")]
        [SerializeField] private ItemContainer[] _relicContainers;

        [Header("Etc")]
        [SerializeField] private Button _backButton;
        [SerializeField] private AutoDistributeController _autoDistributeController;
        [SerializeField] private AddStatController _addStatController;
        [SerializeField] private EquipmentController _equipmentController;
        [SerializeField] private Sprite _lockedRelicSprite;

        private Action _onClose;
        private InputAction _quitApplicationAction;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _expandButton.onClick.AddListener(OpenStatsDetail);
            _foldButton.onClick.AddListener(CloseStatsDetail);
            _backgroundButton.onClick.AddListener(CloseStatsDetail);

            for (int i = 0; i < _statElements.Length; i++)
            {
                _statElements[i].onOpenAddStatButtonClicked.AddListener(OpenAddStatsUI);
            }

            for (int i = 0; i < _equipmentElements.Length; i++)
            {
                _equipmentElements[i].onChangeButtonClicked.AddListener(OpenEquipmentUI);
            }

            _autoDistributeButton.onClick.AddListener(OnAutoDistributeButtonClicked);
            _backButton.onClick.AddListener(OnBackButtonClicked);

            SubscribeToState();
            _quitApplicationAction = InputSystem.actions.FindActionMap("UI").FindAction("Cancel");
        }

        void Update()
        {
            // 튜토리얼 중에는 닫기 비활성화
            bool isTutorialActive = TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive;
            
            if (_quitApplicationAction?.triggered ?? false && !ConfirmationController.Instance.IsVisible && !isTutorialActive)
            {
                Hide();
            }
        }

        public void Show(Action onClose)
        {
            CloseStatsDetail();
            _autoDistributeButton.GetComponentInChildren<TMP_Text>().text = PersistentGameState.Instance.AutoDistributeStats ? "On" : "Off";
            foreach (var statElement in _statElements)
            {
                statElement.UpdateValues();
            }
            UpdateAutoDistributePresetText();
            UpdateFinalStats();
            UpdateRelics();
            _addStatController.Hide();
            gameObject.SetActive(true);

            _onClose = onClose;
        }

        public void Hide(bool invokeOnClose = true)
        {
            gameObject.SetActive(false);
            if (invokeOnClose)
            {
                _onClose?.Invoke();
                _onClose = null;
            }
        }
        
        /// <summary>
        /// Expand 버튼의 활성화 상태를 설정합니다.
        /// </summary>
        public void SetExpandButtonEnabled(bool enabled)
        {
            _expandButton.interactable = enabled;
        }

        private void OpenStatsDetail()
        {
            int criticalRate = 0;
            int criticalDamageMultiplier = (int)(PlayerStats.CRIT_MULTIPLIER * 100);
            int fixedDamage = 0;
            int drainPercentage = 0;
            int healEveryTurn = 0;
            int damageMultiplier = 100;
            int expMultiplier = 100 + PersistentGameState.Instance.GetBoostExp();
            int goldMultiplier = 100 + PersistentGameState.Instance.GetBoostGold();
            int dropRateMultiplier = 100;
            if (GameManager.Instance != null)
            {
                // Relic 효과 컨텍스트 생성
                var relicContext = new RelicEffectContext
                {
                    MonsterData = null,
                    MaxPlayerHP = GameManager.GetStat(StatType.HP),
                    CurrentPlayerHP = GameManager.GetStat(StatType.HP),
                    CurrentMonsterHP = GameManager.GetStat(StatType.HP),
                    CriticalDamageMultiplier = PlayerStats.CRIT_MULTIPLIER,
                    BaseDefense = GameManager.GetStat(StatType.DEF),
                    TurnCount = 0,
                    AttackCount = 0,
                    DamageMultiplier = damageMultiplier / 100f,
                    DropRateMultiplier = dropRateMultiplier / 100f,
                    GoldMultiplier = goldMultiplier / 100f,
                    ExpMultiplier = expMultiplier / 100f,
                    FixedDamage = fixedDamage,
                    DrainPercentage = drainPercentage,
                };
                GameManager.Instance.RelicManager.ApplyEffects(relicContext);

                damageMultiplier = Mathf.RoundToInt(relicContext.DamageMultiplier * 100);
                criticalRate = Mathf.RoundToInt(relicContext.CriticalRate * 100);
                criticalDamageMultiplier = Mathf.RoundToInt(relicContext.CriticalDamageMultiplier * 100);
                goldMultiplier = Mathf.RoundToInt(relicContext.GoldMultiplier * 100);
                expMultiplier = Mathf.RoundToInt(relicContext.ExpMultiplier * 100);
                fixedDamage = relicContext.FixedDamage;
                drainPercentage = relicContext.DrainPercentage;
                if (GameManager.Instance.TryGetRelic<HealEveryTurnEffect>(out Relic relic))
                {
                    healEveryTurn = (relic.Effect as HealEveryTurnEffect).GetHealAmount();
                }
            }
            
            _statsDetailLabelText.text =
                $"{Utils.GetUIString("damage-multiplier")}\n" +
                $"{Utils.GetUIString("critical-rate")}\n" +
                $"{Utils.GetUIString("critical-damage-multiplier")}\n" +
                $"{Utils.GetUIString("fixed-damage")}\n" +
                $"{Utils.GetUIString("drain-percentage")}\n" +
                $"{Utils.GetUIString("heal-every-turn")}\n" +
                $"{Utils.GetUIString("exp-multiplier")}\n" +
                $"{Utils.GetUIString("gold-multiplier")}\n" +
                $"{Utils.GetUIString("drop-multiplier")}";
            _statsDetailValueText.text =
                $"{damageMultiplier}%\n" +
                $"{criticalRate}%\n" +
                $"{criticalDamageMultiplier}%\n" +
                $"{fixedDamage}\n" +
                $"{drainPercentage}%\n" +
                $"{healEveryTurn}\n" +
                $"{expMultiplier}%\n" +
                $"{goldMultiplier}%\n" +
                $"{dropRateMultiplier}%";
            _statsDetailContainer.SetActive(true);
            _expandButton.gameObject.SetActive(false);
        }

        private void CloseStatsDetail()
        {
            _statsDetailContainer.SetActive(false);
            _expandButton.gameObject.SetActive(true);
        }

        private void OnAutoDistributeButtonClicked()
        {
            Hide(false);
            _autoDistributeController.Show(() => {
                Show(_onClose);
                _autoDistributeButton.GetComponentInChildren<TMP_Text>().text = PersistentGameState.Instance.AutoDistributeStats ? "On" : "Off";
            });
        }

        private void OnBackButtonClicked()
        {
            Hide();
        }

        private void SubscribeToState()
        {
            var d = Disposable.CreateBuilder();
            PersistentGameState.Instance.CurrentEquipments[(int)EquipmentType.Weapon].Subscribe(_ => UpdateFinalStats()).AddTo(ref d);
            PersistentGameState.Instance.CurrentEquipments[(int)EquipmentType.Armor].Subscribe(_ => UpdateFinalStats()).AddTo(ref d);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameState.PlayerStats.StatPoints
                    .Subscribe(statPoints => _statPointsText.text = $"{statPoints:n0}")
                    .AddTo(ref d);
                GameManager.Instance.GameState.PlayerStats.Level
                    .Subscribe(level => _levelText.text = $"{level:n0}")
                    .AddTo(ref d);
                GameManager.Instance.GameState.PlayerStats.EXP
                    .Subscribe(exp => _expText.text = $"{exp:n0}/{PlayerStats.CalculateRequiredEXP(GameManager.Instance.GameState.PlayerStats.Level.Value):n0}")
                    .AddTo(ref d);
                foreach (var stat in GameManager.Instance.GameState.PlayerStats.Stats)
                {
                    stat.Subscribe(_ => UpdateFinalStats()).AddTo(ref d);
                }
                GameManager.Instance.GameState.OwnedRelics.Subscribe(_ => {
                    UpdateFinalStats();
                    UpdateRelics();
                }).AddTo(ref d);
            }
            d.RegisterTo(this.destroyCancellationToken);
        }

        private void UpdateAutoDistributePresetText()
        {
            _autoDistributePresetText.text = "PRESET: " + (PersistentGameState.Instance.GetCurrentAutoDistributePreset() + 1);
        }

        private void UpdateFinalStats()
        {
            Weapon weapon = PersistentGameState.Instance.GetCurrentWeapon();
            Armor armor = PersistentGameState.Instance.GetCurrentArmor();
            _finalAttackText.text = $"{(int)weapon.GetFinalDamage(GameManager.GetStat(StatType.ATK)):n0}";
            _finalDefenseText.text = $"{(int)armor.GetFinalDefense(GameManager.GetStat(StatType.DEF)):n0}";
        }

        private void UpdateRelics()
        {
            for (int i = 0; i < _relicContainers.Length; i++)
            {
                if (GameManager.Instance != null && i < GameManager.Instance.GameState.OwnedRelics.Value.Count)
                {
                    int index = i;
                    var relic = GameManager.Instance.GameState.OwnedRelics.Value[index];
                    _relicContainers[i].SetRelic(relic);
                    _relicContainers[i].onClick.AddListener(() => ShowRelicInfo(relic));
                }
                else if (i < PersistentGameState.Instance.GetMaxRelicCount())
                {
                    _relicContainers[i].SetRelic(null);
                    _relicContainers[i].onClick.RemoveAllListeners();
                }
                else
                {
                    _relicContainers[i].SetImage(_lockedRelicSprite);
                    _relicContainers[i].onClick.RemoveAllListeners();
                }
            }
        }

        private void ShowRelicInfo(Relic relic)
        {
            ConfirmationController.Instance.ShowRelicDiscard(
                relic,
                () => { ShowRelicDiscardConfirmation(relic); });
        }

        private void ShowRelicDiscardConfirmation(Relic relic)
        {
            ConfirmationController.Instance.ShowConfirmation(
                message: Utils.GetUIString("relic_discard-confirmation", new[] { relic.Effect.GetName() }),
                onConfirm: () => { GameManager.Instance.DiscardRelic(relic.EffectName); UpdateRelics(); },
                onClose: () => { ShowRelicInfo(relic); }
            );
        }

        private void OpenEquipmentUI(EquipmentType equipmentType)
        {
            Hide(false);
            _equipmentController.Show(equipmentType, () => Show(_onClose));
        }

        private void OpenAddStatsUI(StatType statType)
        {
            _addStatController.Show(statType, GameManager.Instance.GameState.PlayerStats.StatPoints.Value);
        }
    }
}
