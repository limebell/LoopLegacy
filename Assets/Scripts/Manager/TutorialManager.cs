using System;
using System.Collections;
using LoopLegacy.Manager;
using LoopLegacy.Player;
using LoopLegacy.Region;
using LoopLegacy.State;
using LoopLegacy.UI.Component;
using R3;
using TMPro;
using UnityEngine;

namespace LoopLegacy
{
    // Hard-coded tutorial process
    public class TutorialManager : MonoBehaviour
    {
        public TutorialStep CurrentStep { get; private set; }
        private IDisposable _battlePointSubscription;
        private IDisposable _statPointSubscription;

        [SerializeField] private GameObject _waypoint;
        [SerializeField] private GameObject _tutorialPanel;
        [SerializeField] private TMP_Text _tutorialText;
        
        private IDisposable _dialogueIndexSubscription;
        private bool _waitingForStatsPanelOpen = false;
        private bool _hasShownStatInvestmentScript = false;
        private SpotlightOverlay _spotlightOverlay;
        private bool _isStatsButtonHighlighted = false;
        private bool _isStatsHighlighted = false;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _waypoint.SetActive(false);
            _tutorialPanel.SetActive(false);
            GameManager.Instance.HUDController.HideEncounterGauge();
            GameManager.Instance.Save();
            
            // SpotlightOverlay를 한 번만 찾아서 저장
            GameObject spotlightObj = GameObject.Find("SpotlightOverlay");
            if (spotlightObj != null)
            {
                _spotlightOverlay = spotlightObj.GetComponent<SpotlightOverlay>();
                if (_spotlightOverlay == null)
                {
                    Debug.LogError("SpotlightOverlay component not found on SpotlightOverlay GameObject");
                }
            }
            else
            {
                Debug.LogError("SpotlightOverlay GameObject not found");
            }
            
            StartTutorial();
        }

        void Update()
        {
            if (CurrentStep == TutorialStep.Begin || CurrentStep == TutorialStep.Movement)
            {
                // Keep encounter gauge at in tutorial 0
                GameManager.Instance.EncounterManager.ResetGauge();
            }
            
            // 스탯창이 열렸는지 확인
            if (_waitingForStatsPanelOpen && CurrentStep == TutorialStep.PlayerStats)
            {
                var statsController = GameManager.Instance.HUDController.StatsController;
                if (statsController != null && statsController.gameObject.activeSelf)
                {
                    _waitingForStatsPanelOpen = false;
                    OnStatsPanelOpened();
                }
            }
        }

        void OnDestroy()
        {
            _battlePointSubscription?.Dispose();
            _statPointSubscription?.Dispose();
            _dialogueIndexSubscription?.Dispose();
        }

        private void StartTutorial()
        {
            CurrentStep = TutorialStep.Begin;
            _waypoint.SetActive(false);
            
            // 대사 인덱스 구독하여 특정 대사에서 UI 강조
            _dialogueIndexSubscription = ScriptManager.Instance.CurrentDialogueIndex
                .Subscribe(index => OnDialogueIndexChanged(index));
            
            ScriptManager.Instance.StartScript("tutorial_script", PracticeMovement);
        }
        
        /// <summary>
        /// 대사 인덱스가 변경될 때 호출됩니다.
        /// 특정 대사에서 UI 요소를 강조합니다.
        /// </summary>
        private void OnDialogueIndexChanged(int index)
        {
            // 스탯 버튼이 강조 중이면 대사 인덱스 변경과 무관하게 유지
            if (_isStatsButtonHighlighted)
            {
                return;
            }
            
            // Stats가 강조 중이면 대사 인덱스 변경과 무관하게 유지
            if (_isStatsHighlighted)
            {
                return;
            }
            
            // 위험도 게이지 설명 (tutorial_script-5, 6, 7, 8)
            if (index >= 6 && index <= 8)
            {
                HighlightEncounterGauge();
            }
            // 스탯 버튼 설명 (tutorial_script-13은 CSV에서 인덱스 13)
            else if (index == 13)
            {
                HighlightStatsButton();
            }
            // Stats 설명 (tutorial_script-14는 CSV에서 인덱스 14)
            else if (index == 14)
            {
                HighlightStats();
            }
            // 자동 분배 설명 (tutorial_script-15는 CSV에서 인덱스 15)
            else if (index == 15)
            {
                HighlightAutoDistributeButton();
            }
            // 그 외의 경우 오버레이 숨김
            else
            {
                HideHighlight();
            }
        }
        
        /// <summary>
        /// 강조를 숨깁니다.
        /// </summary>
        private void HideHighlight()
        {
            if (_spotlightOverlay == null) return;
            _spotlightOverlay.Hide();
        }
        
        /// <summary>
        /// 위험도 게이지를 강조합니다.
        /// </summary>
        private void HighlightEncounterGauge()
        {
            if (_spotlightOverlay == null) return;
            
            var hudController = GameManager.Instance?.HUDController;
            RectTransform gaugeRect = hudController?.EncounterGaugeRect;
            
            if (gaugeRect == null)
            {
                // Fallback: GameObject.Find 사용
                GameObject encounterGaugeObj = GameObject.Find("EncounterGauge");
                if (encounterGaugeObj != null)
                {
                    gaugeRect = encounterGaugeObj.GetComponent<RectTransform>();
                }
            }
            
            if (gaugeRect != null)
            {
                _spotlightOverlay.Show();
                _spotlightOverlay.SetHighlightArea(gaugeRect, 10f);
            }
        }
        
        /// <summary>
        /// 스탯 버튼을 강조합니다.
        /// </summary>
        private void HighlightStatsButton()
        {
            if (_spotlightOverlay == null) return;
            
            var hudController = GameManager.Instance?.HUDController;
            RectTransform buttonRect = hudController?.StatsButtonRect;
            
            if (buttonRect == null)
            {
                // Fallback: GameObject.Find 사용
                GameObject statsButtonObj = GameObject.Find("StatsButton");
                if (statsButtonObj == null)
                {
                    // 다른 방법으로 찾기 시도
                    var buttons = FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None);
                    foreach (var button in buttons)
                    {
                        if (button.name.Contains("Stat") || button.name.Contains("stat"))
                        {
                            statsButtonObj = button.gameObject;
                            break;
                        }
                    }
                }
                
                if (statsButtonObj != null)
                {
                    buttonRect = statsButtonObj.GetComponent<RectTransform>();
                }
            }
            
            if (buttonRect != null)
            {
                _isStatsButtonHighlighted = true;
                _spotlightOverlay.Show();
                _spotlightOverlay.SetHighlightArea(buttonRect, 10f);
            }
        }
        
        /// <summary>
        /// Stats GameObject를 강조합니다.
        /// </summary>
        private void HighlightStats()
        {
            if (_spotlightOverlay == null) return;
            
            // "Stats"라는 이름의 GameObject 찾기
            GameObject statsObj = GameObject.Find("Stats");
            if (statsObj == null)
            {
                Debug.LogError("Stats GameObject not found");
                return;
            }
            
            RectTransform statsRect = statsObj.GetComponent<RectTransform>();
            if (statsRect == null)
            {
                Debug.LogError("Stats GameObject does not have RectTransform");
                return;
            }
            
            _isStatsHighlighted = true;
            _spotlightOverlay.Show();
            _spotlightOverlay.SetHighlightArea(statsRect, 10f);
        }
        
        /// <summary>
        /// 자동 분배 버튼을 강조합니다.
        /// </summary>
        private void HighlightAutoDistributeButton()
        {
            if (_spotlightOverlay == null) return;
            
            var statsController = GameManager.Instance?.HUDController?.StatsController;
            RectTransform buttonRect = statsController?.AutoDistributeButtonRect;
            
            if (buttonRect == null)
            {
                // Fallback: GameObject.Find 사용
                GameObject autoDistributeButtonObj = GameObject.Find("AutoDistributeButton");
                if (autoDistributeButtonObj == null)
                {
                    // 다른 방법으로 찾기 시도
                    var buttons = FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None);
                    foreach (var button in buttons)
                    {
                        if (button.name.Contains("AutoDistribute") || button.name.Contains("auto") || button.name.Contains("distribute"))
                        {
                            autoDistributeButtonObj = button.gameObject;
                            break;
                        }
                    }
                }
                
                if (autoDistributeButtonObj != null)
                {
                    buttonRect = autoDistributeButtonObj.GetComponent<RectTransform>();
                }
            }
            
            if (buttonRect != null)
            {
                _spotlightOverlay.Show();
                _spotlightOverlay.SetHighlightArea(buttonRect, 10f);
            }
        }
        
        private void PracticeMovement()
        {
            CurrentStep = TutorialStep.Movement;
            _waypoint.SetActive(true);
            _waypoint.GetComponent<Waypoint>().OnWaypointReached.AddListener(OnWaypointReached);
            _tutorialPanel.SetActive(true);
            _tutorialText.text = Utils.GetCutsceneText("tutorial_objective-0");
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
            _battlePointSubscription = GameManager.Instance.GameState.PlayerStats.BattlePoint.Subscribe(x => OnBattlePointChanged(x));
            _tutorialPanel.SetActive(true);
            _tutorialText.text = Utils.GetCutsceneText("tutorial_objective-1");
        }

        private void OnBattlePointChanged(int battlePoints)
        {
            if (battlePoints == 29)
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
            _statPointSubscription = GameManager.Instance.GameState.PlayerStats.StatPoints.Subscribe(x => OnStatPointChanged(x));
            
            // 스탯 버튼을 강조하고 스탯창을 열도록 유도
            _waitingForStatsPanelOpen = true;
            _hasShownStatInvestmentScript = false;
            
            // 스탯창 열기 유도 스크립트 재생 (12번부터 시작, 13번에서 스탯 버튼 강조)
            _tutorialPanel.SetActive(false);
            ScriptManager.Instance.StartScript(
                "tutorial_script", ShowTutorialPanelForStats, 12);
        }

        private void ShowTutorialPanelForStats()
        {
            _tutorialPanel.SetActive(true);
            _tutorialText.text = Utils.GetCutsceneText("tutorial_objective-2");
        }
        
        /// <summary>
        /// 스탯창이 열렸을 때 호출됩니다.
        /// </summary>
        private void OnStatsPanelOpened()
        {
            if (_hasShownStatInvestmentScript) return;
            
            _hasShownStatInvestmentScript = true;
            _tutorialPanel.SetActive(false);
            
            // 스탯 버튼 강조 상태 해제 및 숨김
            _isStatsButtonHighlighted = false;
            HideHighlight();
            
            // 스탯 투자 설명 스크립트 재생 (tutorial_script-14부터 시작, 인덱스 14)
            ScriptManager.Instance.StartScript("tutorial_script", ShowTutorialPanelForStatsInvestment, 14);
        }

        private void ShowTutorialPanelForStatsInvestment()
        {
            _tutorialPanel.SetActive(true);
            _tutorialText.text = Utils.GetCutsceneText("tutorial_objective-3");
        }

        private void OnStatPointChanged(int statPoints)
        {
            if (statPoints == 0)
            {
                // Stats 강조 상태 해제 및 숨김
                if (_isStatsHighlighted)
                {
                    _isStatsHighlighted = false;
                    HideHighlight();
                }
                
                // 스탯 포인트가 모두 소모되면 자동 분배 유도
                StartCoroutine(GuideAutoDistribute());
            }
        }
        
        /// <summary>
        /// 자동 분배를 유도합니다.
        /// </summary>
        private IEnumerator GuideAutoDistribute()
        {
            // 스탯창이 열려있으면 잠시 대기
            var statsController = GameManager.Instance.HUDController.StatsController;
            if (statsController != null && statsController.gameObject.activeSelf)
            {
                yield return new WaitForSeconds(0.5f);
                
                // 자동 분배 설명 스크립트 재생 (tutorial_script-15부터 시작, 인덱스 15)
                _tutorialPanel.SetActive(false);
                ScriptManager.Instance.StartScript("tutorial_script", OnAutoDistributeScriptComplete, 15);
            }
            else
            {
                // 스탯창이 닫혀있으면 바로 종료
                EndStatPointTutorial();
            }
        }
        
        /// <summary>
        /// 자동 분배 설명 스크립트가 완료된 후 호출됩니다.
        /// </summary>
        private void OnAutoDistributeScriptComplete()
        {
            GameManager.Instance.HUDController.StatsController.Hide(false);
            EndStatPointTutorial();
        }

        private void EndStatPointTutorial()
        {
            _statPointSubscription?.Dispose();
            _tutorialPanel.SetActive(false);
            ScriptManager.Instance.StartScript("tutorial_script", EndTutorial, 16);

        }

        private void PracticeEquipment()
        {
            Debug.Log("PracticeEquipment");
        }

        private void EndTutorial()
        {
            GameManager.Instance.HUDController.ShowEncounterGauge();
            PlayerMovement.Instance.SetCannotMove(false);
            PersistentGameState.Instance.CompleteTutorial(TutorialType.GameStart);
            GameManager.Instance.Save();
            GameManager.Instance.MoveToMap("Start", new Vector2(0, 0));
        }
    }
}
