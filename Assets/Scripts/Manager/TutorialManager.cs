using System;
using LoopLegacy.Manager;
using LoopLegacy.Player;
using LoopLegacy.Region;
using LoopLegacy.State;
using LoopLegacy.UI.Component;
using LoopLegacy.UI.Controller;
using R3;
using TMPro;
using UnityEngine;

namespace LoopLegacy
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }
        public bool IsTutorialActive { get; private set; } = false;
        public TutorialStep CurrentStep { get; private set; }
        
        [SerializeField] private GameObject _waypoint;
        [SerializeField] private GameObject _tutorialPanel;
        [SerializeField] private TMP_Text _tutorialText;
        
        private IDisposable _dialogueIndexSubscription;
        private IDisposable _battlePointSubscription;
        private IDisposable _statPointSubscription;
        
        private SpotlightOverlay _spotlightOverlay;
        private bool _waitingForStatsPanelOpen;
        private bool _hasShownStatInvestmentScript;
        
        // 강조 상태 플래그
        private bool _isStatsButtonHighlighted;
        private bool _isStatsHighlighted;
        private bool _isAddStatHighlighted;
        private bool _isAutoDistributeButtonHighlighted;
        private bool _isToggleSwitchHighlighted;
        private bool _isApplyButtonHighlighted;
        
        // 대기 상태 플래그
        private bool _waitingForAutoDistributePanel;
        private bool _waitingForToggleSwitchOn;
        private bool _waitingForAutoDistribute;
        
        // 캐싱된 GameObject
        private GameObject _addStatGameObject;
        private GameObject _autoDistributePanelGameObject;
        private GameObject _applyButtonGameObject;
        private ToggleSwitch _toggleSwitch;

        void Start()
        {
            Instance = this;
            _waypoint.SetActive(false);
            _tutorialPanel.SetActive(false);
            GameManager.Instance.HUDController.HideEncounterGauge();
            GameManager.Instance.Save();
            
            _spotlightOverlay = GameObject.Find("SpotlightOverlay")?.GetComponent<SpotlightOverlay>();
            if (_spotlightOverlay == null) Debug.LogError("SpotlightOverlay not found");
            
            StartTutorial();
        }

        void Update()
        {
            if (CurrentStep == TutorialStep.Begin || CurrentStep == TutorialStep.Movement)
                GameManager.Instance.EncounterManager.ResetGauge();
            
            if (CurrentStep != TutorialStep.PlayerStats) return;
            
            // 스탯창 열림 대기
            if (_waitingForStatsPanelOpen)
            {
                var statsController = GameManager.Instance.HUDController.StatsController;
                if (statsController != null && statsController.gameObject.activeSelf)
                {
                    _waitingForStatsPanelOpen = false;
                    OnStatsPanelOpened();
                }
            }
            
            // AddStat 패널 상태 모니터링
            if (_isStatsHighlighted) CheckAddStatPanelState();
            
            // 자동 분배 패널 열림 모니터링
            if (_waitingForAutoDistributePanel) CheckAutoDistributePanelState();
            
            // 토글 스위치 On 모니터링
            if (_waitingForToggleSwitchOn) CheckToggleSwitchState();
            
            // 자동 분배 설정 모니터링
            if (_waitingForAutoDistribute && PersistentGameState.Instance.AutoDistributeStats)
            {
                _waitingForAutoDistribute = false;
                OnAutoDistributeEnabled();
            }
        }

        void OnDestroy()
        {
            _battlePointSubscription?.Dispose();
            _statPointSubscription?.Dispose();
            _dialogueIndexSubscription?.Dispose();
        }

        #region Spotlight Helpers
        
        private bool SetSpotlight(string objectName, float margin = 5f)
        {
            if (_spotlightOverlay == null) return false;
            
            var rect = GameObject.Find(objectName)?.GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.LogError($"{objectName} GameObject not found");
                return false;
            }
            
            _spotlightOverlay.Show();
            _spotlightOverlay.SetHighlightArea(rect, margin);
            return true;
        }
        
        private void HideHighlight()
        {
            _spotlightOverlay?.Hide();
        }
        
        #endregion

        #region Tutorial Flow
        
        private void StartTutorial()
        {
            CurrentStep = TutorialStep.Begin;
            IsTutorialActive = true;
            _waypoint.SetActive(false);
            
            _dialogueIndexSubscription = ScriptManager.Instance.CurrentDialogueIndex
                .Subscribe(OnDialogueIndexChanged);
            
            ScriptManager.Instance.StartScript("tutorial_script", PracticeMovement);
        }
        
        private void OnDialogueIndexChanged(int index)
        {
            // 강조 중이면 대사 인덱스 변경 무시
            if (_isStatsButtonHighlighted || _isStatsHighlighted || 
                _isAutoDistributeButtonHighlighted || _isToggleSwitchHighlighted || _isApplyButtonHighlighted)
                return;
            
            switch (index)
            {
                case >= 6 and <= 8:
                    SetSpotlight("EncounterGauge", 5f);
                    break;
                case 13:
                    _isStatsButtonHighlighted = SetSpotlight("StatButton", 5f);
                    break;
                case 14:
                    HighlightStats();
                    break;
                case 15:
                    HighlightAutoDistributionButton();
                    break;
                default:
                    HideHighlight();
                    break;
            }
        }
        
        private void PracticeMovement()
        {
            CurrentStep = TutorialStep.Movement;
            _waypoint.SetActive(true);
            _waypoint.GetComponent<Waypoint>().OnWaypointReached.AddListener(OnWaypointReached);
            ShowTutorialObjective("tutorial_objective-0");
        }

        public void OnWaypointReached()
        {
            PlayerMovement.Instance.SetCannotMove(true);
            _waypoint.SetActive(false);
            _tutorialPanel.SetActive(false);
            GameManager.Instance.HUDController.ShowEncounterGauge();
            ScriptManager.Instance.StartScript("tutorial_script", PracticeEncounter, 4);
        }

        private void PracticeEncounter()
        {
            PlayerMovement.Instance.SetCannotMove(false);
            CurrentStep = TutorialStep.Encounter;
            _battlePointSubscription = GameManager.Instance.GameState.PlayerStats.BattlePoint
                .Subscribe(OnBattlePointChanged);
            ShowTutorialObjective("tutorial_objective-1");
        }

        private void OnBattlePointChanged(int battlePoints)
        {
            if (battlePoints <= 29)
            {
                _battlePointSubscription?.Dispose();
                _tutorialPanel.SetActive(false);
                ScriptManager.Instance.StartScript("tutorial_script", PracticePlayerStats, 9);
            }
        }

        private void PracticePlayerStats()
        {
            CurrentStep = TutorialStep.PlayerStats;
            PlayerMovement.Instance.SetCannotMove(true);
            _statPointSubscription = GameManager.Instance.GameState.PlayerStats.StatPoints
                .Subscribe(OnStatPointChanged);
            
            _waitingForStatsPanelOpen = true;
            _hasShownStatInvestmentScript = false;
            _tutorialPanel.SetActive(false);
            ScriptManager.Instance.StartScript("tutorial_script", () => ShowTutorialObjective("tutorial_objective-2"), 12);
        }

        private void OnStatsPanelOpened()
        {
            if (_hasShownStatInvestmentScript) return;
            
            _hasShownStatInvestmentScript = true;
            _tutorialPanel.SetActive(false);
            _isStatsButtonHighlighted = false;
            HideHighlight();
            
            ScriptManager.Instance.StartScript("tutorial_script", () => ShowTutorialObjective("tutorial_objective-3"), 14);
        }

        private void OnStatPointChanged(int statPoints)
        {
            if (statPoints == 0)
            {
                _isStatsHighlighted = false;
                _isAddStatHighlighted = false;
                HideHighlight();
                GuideAutoDistribute();
            }
        }
        
        private void GuideAutoDistribute()
        {
            var statsController = GameManager.Instance.HUDController.StatsController;
            if (statsController != null && statsController.gameObject.activeSelf)
            {
                _tutorialPanel.SetActive(false);
                ScriptManager.Instance.StartScript("tutorial_script", StartAutoDistributeTutorial, 15);
            }
            else
            {
                EndStatPointTutorial();
            }
        }
        
        private void StartAutoDistributeTutorial()
        {
            ShowTutorialObjective("tutorial_objective-4");
            
            if (PersistentGameState.Instance.AutoDistributeStats)
            {
                EndStatPointTutorial();
            }
            else
            {
                HighlightAutoDistributionButton();
                _waitingForAutoDistributePanel = true;
            }
        }
        
        private void OnAutoDistributeEnabled()
        {
            _isAutoDistributeButtonHighlighted = false;
            _isToggleSwitchHighlighted = false;
            _isApplyButtonHighlighted = false;
            HideHighlight();
            
            GameManager.Instance.HUDController.StatsController.SetExpandButtonEnabled(true);
            EndStatPointTutorial();
        }

        private void EndStatPointTutorial()
        {
            _statPointSubscription?.Dispose();
            _tutorialPanel.SetActive(false);
            GameManager.Instance.HUDController.StatsController.Hide(false);
            ScriptManager.Instance.StartScript("tutorial_script", EndTutorial, 16);
        }

        private void EndTutorial()
        {
            IsTutorialActive = false;
            GameManager.Instance.HUDController.ShowEncounterGauge();
            PlayerMovement.Instance.SetCannotMove(false);
            PersistentGameState.Instance.CompleteTutorial(TutorialType.GameStart);
            GameManager.Instance.Save();
            GameManager.Instance.MoveToMap("Start", new Vector2(0, 0));
        }
        
        #endregion

        #region Highlight Methods
        
        private void HighlightStats()
        {
            if (SetSpotlight("Stats", 0f))
            {
                _isStatsHighlighted = true;
                _isAddStatHighlighted = false;
            }
        }
        
        private void HighlightAutoDistributionButton()
        {
            if (SetSpotlight("AutoDistributionButton", 3f))
            {
                _isAutoDistributeButtonHighlighted = true;
                GameManager.Instance.HUDController.StatsController.SetExpandButtonEnabled(false);
            }
        }
        
        private void HighlightToggleSwitch()
        {
            if (SetSpotlight("ToggleSwitch", 10f))
                _isToggleSwitchHighlighted = true;
        }
        
        private void HighlightApplyButton()
        {
            if (_spotlightOverlay == null) return;
            
            if (_applyButtonGameObject == null && _autoDistributePanelGameObject != null)
            {
                var transform = _autoDistributePanelGameObject.transform.Find("ApplyButton");
                _applyButtonGameObject = transform?.gameObject ?? GameObject.Find("ApplyButton");
            }
            
            if (_applyButtonGameObject == null)
            {
                Debug.LogError("ApplyButton not found");
                return;
            }
            
            var rect = _applyButtonGameObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                _isApplyButtonHighlighted = true;
                _spotlightOverlay.Show();
                _spotlightOverlay.SetHighlightArea(rect, 10f);
            }
        }
        
        #endregion

        #region State Checkers
        
        private void CheckAddStatPanelState()
        {
            if (_addStatGameObject == null)
            {
                _addStatGameObject = GameObject.Find("AddStat");
                if (_addStatGameObject == null)
                {
                    var statsController = GameManager.Instance?.HUDController?.StatsController;
                    _addStatGameObject = statsController?.transform.Find("AddStat")?.gameObject;
                }
            }
            
            if (_addStatGameObject == null) return;
            
            bool isActive = _addStatGameObject.activeSelf;
            
            if (isActive && !_isAddStatHighlighted)
            {
                var rect = _addStatGameObject.GetComponent<RectTransform>();
                if (rect != null)
                {
                    _isAddStatHighlighted = true;
                    _spotlightOverlay?.Show();
                    _spotlightOverlay?.SetHighlightArea(rect, 0f);
                }
            }
            else if (!isActive && _isAddStatHighlighted)
            {
                _isAddStatHighlighted = false;
                SetSpotlight("Stats", 0f);
            }
        }
        
        private void CheckAutoDistributePanelState()
        {
            if (_autoDistributePanelGameObject == null)
            {
                _autoDistributePanelGameObject = GameObject.Find("AutoDistributePanel");
                if (_autoDistributePanelGameObject == null)
                {
                    var controller = FindAnyObjectByType<AutoDistributeController>();
                    _autoDistributePanelGameObject = controller?.gameObject;
                }
            }
            
            if (_autoDistributePanelGameObject != null && _autoDistributePanelGameObject.activeSelf)
            {
                _waitingForAutoDistributePanel = false;
                _isAutoDistributeButtonHighlighted = false;
                HighlightToggleSwitch();
                _waitingForToggleSwitchOn = true;
            }
        }
        
        private void CheckToggleSwitchState()
        {
            if (_toggleSwitch == null)
                _toggleSwitch = GameObject.Find("ToggleSwitch")?.GetComponent<ToggleSwitch>();
            
            if (_toggleSwitch != null && _toggleSwitch.CurrentValue)
            {
                _waitingForToggleSwitchOn = false;
                _isToggleSwitchHighlighted = false;
                HighlightApplyButton();
                _waitingForAutoDistribute = true;
            }
        }
        
        #endregion

        #region Helpers
        
        private void ShowTutorialObjective(string key)
        {
            _tutorialPanel.SetActive(true);
            _tutorialText.text = Utils.GetCutsceneText(key);
        }
        
        #endregion
    }
}
