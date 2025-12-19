using System;
using LoopLegacy.State;
using LoopLegacy.UI.Component;
using LoopLegacy.UI.Controller;
using R3;
using TMPro;
using UnityEngine;

namespace LoopLegacy.Manager
{
    public enum RelicTutorialStep
    {
        None,
        HighlightRewardArea,        // 1. 보상 영역 하이라이트
        SelectReward,               // 2. 보상 영역 선택 유도
        HighlightDescription,       // 3. 보상 설명 영역 하이라이트
        ExplainReroll,              // 4. 리롤 설명 및 버튼 하이라이트
        WaitForReroll,              // 리롤 대기
        ExplainRerollSuccess,       // 리롤 성공 설명
        SelectAfterReroll,          // 5. 리롤 후 유물 선택
        HighlightAcquireButton,     // 획득 버튼 하이라이트
        ExplainStatsWindow,         // 6. 스탯창 안내 설명
        WaitForStatsOpen,           // 스탯창 열기 대기
        HighlightRelicInStats,      // 7. 스탯창에서 유물 영역 하이라이트
        Complete
    }

    public class RelicTutorialManager : MonoBehaviour
    {
        public static RelicTutorialManager Instance { get; private set; }
        public bool IsTutorialActive { get; private set; } = false;
        public RelicTutorialStep CurrentStep { get; private set; } = RelicTutorialStep.None;

        [SerializeField] private GameObject _tutorialPanel;
        [SerializeField] private TMP_Text _tutorialText;

        private SpotlightOverlay _spotlightOverlay;
        private IDisposable _dialogueIndexSubscription;
        
        // 대기 상태 플래그
        private bool _waitingForRewardSelection;
        private bool _waitingForReroll;
        private bool _waitingForAcquire;
        private bool _waitingForStatsOpen;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            if (_tutorialPanel != null)
                _tutorialPanel.SetActive(false);
        }

        void Update()
        {
            if (!IsTutorialActive) return;
            
            // 스탯창 열림 대기
            if (_waitingForStatsOpen && CurrentStep == RelicTutorialStep.WaitForStatsOpen)
            {
                var statsController = GameManager.Instance?.HUDController?.StatsController;
                if (statsController != null && statsController.gameObject.activeSelf)
                {
                    _waitingForStatsOpen = false;
                    OnStatsPanelOpened();
                }
            }
        }

        void OnDestroy()
        {
            _dialogueIndexSubscription?.Dispose();
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// RelicRewardController가 열릴 때 호출되어 튜토리얼 시작 여부를 결정합니다.
        /// </summary>
        public void TryStartTutorial()
        {
            // 이미 튜토리얼이 진행 중이면 시작하지 않음
            if (IsTutorialActive)
                return;
            
            // 이미 튜토리얼을 완료했으면 시작하지 않음
            if (PersistentGameState.Instance.CompletedTutorials[TutorialType.RelicReward])
                return;

            string message = Utils.GetUIString("start-tutorial-confirmation_relic");
            ConfirmationController.Instance.ShowConfirmation(
                message: message,
                onConfirm: () => StartTutorial(),
                onClose: () => {
                    PersistentGameState.Instance.CompleteTutorial(TutorialType.RelicReward);
                    PersistentGameState.Instance.SaveState();
                },
                confirmButtonText: Utils.GetUIString("yes"),
                closeButtonText: Utils.GetUIString("no")
            );
        }

        #region Spotlight Helpers

        private bool SetSpotlight(string objectName, float margin = 5f)
        {
            if (_spotlightOverlay == null)
            {
                _spotlightOverlay = GameObject.Find("SpotlightOverlay")?.GetComponent<SpotlightOverlay>();
                if (_spotlightOverlay == null)
                {
                    Debug.LogError("SpotlightOverlay not found");
                    return false;
                }
            }

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
            IsTutorialActive = true;
            CurrentStep = RelicTutorialStep.HighlightRewardArea;

            _dialogueIndexSubscription = ScriptManager.Instance.CurrentDialogueIndex
                .Subscribe(OnDialogueIndexChanged);

            // 첫 번째 단계: 보상 영역 하이라이트와 설명
            ScriptManager.Instance.StartScript("relic_tutorial_script", OnStep1Complete);
        }

        private void OnDialogueIndexChanged(int index)
        {
            if (!IsTutorialActive) return;

            switch (CurrentStep)
            {
                case RelicTutorialStep.HighlightRewardArea:
                    // 보상 영역 하이라이트 (index 1부터)
                    if (index >= 1)
                        HighlightRewardArea();
                    break;
                case RelicTutorialStep.HighlightDescription:
                    // 설명 영역 하이라이트
                    HighlightDescriptionArea();
                    break;
                case RelicTutorialStep.ExplainReroll:
                    // 리롤 버튼 하이라이트
                    HighlightRerollButton();
                    break;
                case RelicTutorialStep.SelectAfterReroll:
                    // 리롤 후 보상 영역 다시 하이라이트
                    HighlightRewardArea();
                    break;
                case RelicTutorialStep.HighlightAcquireButton:
                    // 획득 버튼 하이라이트
                    HighlightAcquireButton();
                    break;
                case RelicTutorialStep.HighlightRelicInStats:
                    // 스탯창에서 유물 영역 하이라이트
                    HighlightRelicInStats();
                    break;
            }
        }

        /// <summary>
        /// Step 1 완료: 보상 영역 선택 유도
        /// </summary>
        private void OnStep1Complete()
        {
            CurrentStep = RelicTutorialStep.SelectReward;
            _waitingForRewardSelection = true;
            ShowTutorialObjective("relic_tutorial_objective-0"); // "보상 유물을 선택해주세요"
        }

        /// <summary>
        /// 보상 유물이 선택되었을 때 RelicRewardController에서 호출
        /// </summary>
        public void OnRewardSelected()
        {
            if (!IsTutorialActive) return;

            if (_waitingForRewardSelection && CurrentStep == RelicTutorialStep.SelectReward)
            {
                _waitingForRewardSelection = false;
                HideHighlight();
                _tutorialPanel?.SetActive(false);

                // Step 3: 설명 영역 하이라이트 및 설명
                CurrentStep = RelicTutorialStep.HighlightDescription;
                ScriptManager.Instance.StartScript("relic_tutorial_script", OnStep3Complete, 2);
            }
            else if (_waitingForRewardSelection && CurrentStep == RelicTutorialStep.SelectAfterReroll)
            {
                _waitingForRewardSelection = false;
                HideHighlight();
                _tutorialPanel?.SetActive(false);

                // Step 5-2: 획득 버튼 하이라이트 (대사 없이 바로 진행)
                CurrentStep = RelicTutorialStep.HighlightAcquireButton;
                HighlightAcquireButton();
                _waitingForAcquire = true;
                ShowTutorialObjective("relic_tutorial_objective-3"); // "획득 버튼을 눌러 유물을 획득하세요"
            }
        }

        /// <summary>
        /// Step 3 완료: 리롤 설명 시작
        /// </summary>
        private void OnStep3Complete()
        {
            CurrentStep = RelicTutorialStep.ExplainReroll;
            ScriptManager.Instance.StartScript("relic_tutorial_script", OnStep4Complete, 4);
        }

        /// <summary>
        /// Step 4 완료: 리롤 대기
        /// </summary>
        private void OnStep4Complete()
        {
            CurrentStep = RelicTutorialStep.WaitForReroll;
            GameManager.Instance.GameState.RelicRewardRerollCount.Value += 1;
            _waitingForReroll = true;
            ShowTutorialObjective("relic_tutorial_objective-1"); // "리롤 버튼을 눌러 새로운 유물을 확인해보세요"
        }

        /// <summary>
        /// 리롤이 수행되었을 때 RelicRewardController에서 호출
        /// </summary>
        public void OnRerolled()
        {
            if (!IsTutorialActive || !_waitingForReroll) return;

            _waitingForReroll = false;
            CurrentStep = RelicTutorialStep.ExplainRerollSuccess;
            HideHighlight();
            _tutorialPanel?.SetActive(false);

            // 리롤 성공 설명 대사 재생
            ScriptManager.Instance.StartScript("relic_tutorial_script", OnRerollSuccessComplete, 6);
        }

        /// <summary>
        /// 리롤 성공 설명 완료: 유물 선택 유도
        /// </summary>
        private void OnRerollSuccessComplete()
        {
            CurrentStep = RelicTutorialStep.SelectAfterReroll;
            _waitingForRewardSelection = true;
            HighlightRewardArea();
            ShowTutorialObjective("relic_tutorial_objective-2"); // "새로운 유물 중 하나를 선택하세요"
        }

        /// <summary>
        /// Step 5 완료: 획득 대기
        /// </summary>
        private void OnStep5Complete()
        {
            _waitingForAcquire = true;
            ShowTutorialObjective("relic_tutorial_objective-3"); // "획득 버튼을 눌러 유물을 획득하세요"
        }

        /// <summary>
        /// 유물이 획득되었을 때 RelicRewardController에서 호출
        /// </summary>
        public void OnRelicAcquired()
        {
            if (!IsTutorialActive || !_waitingForAcquire) return;

            _waitingForAcquire = false;
            CurrentStep = RelicTutorialStep.ExplainStatsWindow;
            HideHighlight();
            _tutorialPanel?.SetActive(false);

            // Step 6: 스탯창 안내 대사 재생
            ScriptManager.Instance.StartScript("relic_tutorial_script", OnStep6Complete, 7);
        }

        /// <summary>
        /// Step 6 완료: 스탯창 열기 유도
        /// </summary>
        private void OnStep6Complete()
        {
            CurrentStep = RelicTutorialStep.WaitForStatsOpen;
            _waitingForStatsOpen = true;
            HighlightStatButton();
            ShowTutorialObjective("relic_tutorial_objective-4"); // "스탯 버튼을 눌러 획득한 유물을 확인하세요"
        }

        /// <summary>
        /// 스탯창이 열렸을 때 호출
        /// </summary>
        private void OnStatsPanelOpened()
        {
            HideHighlight();
            _tutorialPanel?.SetActive(false);

            // Step 7: 유물 영역 하이라이트 및 설명
            CurrentStep = RelicTutorialStep.HighlightRelicInStats;
            ScriptManager.Instance.StartScript("relic_tutorial_script", OnStep7Complete, 8);
        }

        /// <summary>
        /// Step 7 완료: 튜토리얼 완료
        /// </summary>
        private void OnStep7Complete()
        {
            HideHighlight();
            CompleteTutorial();
        }

        private void CompleteTutorial()
        {
            CurrentStep = RelicTutorialStep.Complete;
            IsTutorialActive = false;
            _dialogueIndexSubscription?.Dispose();

            PersistentGameState.Instance.CompleteTutorial(TutorialType.RelicReward);
            PersistentGameState.Instance.SaveState();
        }

        #endregion

        #region Highlight Methods

        private void HighlightRewardArea()
        {
            SetSpotlight("RelicRewards", 10f);
        }

        private void HighlightDescriptionArea()
        {
            SetSpotlight("RelicDescription/Background", 10f);
        }

        private void HighlightRerollButton()
        {
            SetSpotlight("RerollButton", 10f);
        }

        private void HighlightAcquireButton()
        {
            SetSpotlight("AcquireDiscardButton", 10f);
        }

        private void HighlightStatButton()
        {
            SetSpotlight("StatButton", 10f);
        }

        private void HighlightRelicInStats()
        {
            SetSpotlight("StatsPanel/Panel/Components/Component/Relics", 10f);
        }

        #endregion

        #region Helpers

        private void ShowTutorialObjective(string key)
        {
            if (_tutorialPanel == null || _tutorialText == null) return;

            _tutorialPanel.SetActive(true);
            _tutorialText.text = Utils.GetCutsceneText(key);
        }

        /// <summary>
        /// 현재 특정 단계에서 상호작용을 차단해야 하는지 확인합니다.
        /// </summary>
        public bool ShouldBlockInteraction(string interactionType)
        {
            if (!IsTutorialActive) return false;

            switch (interactionType)
            {
                case "reroll":
                    // 리롤 대기 단계가 아니면 리롤 차단
                    return CurrentStep != RelicTutorialStep.WaitForReroll && 
                           CurrentStep != RelicTutorialStep.ExplainReroll;
                case "acquire":
                    // 획득 버튼 하이라이트 단계가 아니면 획득 차단
                    return CurrentStep != RelicTutorialStep.HighlightAcquireButton;
                case "skip":
                    // 튜토리얼 중에는 스킵 버튼 항상 차단
                    return true;
                default:
                    return false;
            }
        }

        #endregion
    }
}
