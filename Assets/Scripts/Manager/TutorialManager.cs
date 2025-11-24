using System;
using System.Collections;
using LoopLegacy.Manager;
using LoopLegacy.Player;
using LoopLegacy.State;
using R3;
using TMPro;
using UnityEngine;

namespace LoopLegacy
{
    // Hard-coded tutorial process
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }
        public TutorialStep CurrentStep { get; private set; }
        private IDisposable _battlePointSubscription;
        private IDisposable _statPointSubscription;

        [SerializeField] private GameObject _waypoint;
        [SerializeField] private GameObject _tutorialPanel;
        [SerializeField] private TMP_Text _tutorialText;

        void Awake()
        {
            Instance = this;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _waypoint.SetActive(false);
            _tutorialPanel.SetActive(false);
            GameManager.Instance.HUDController.HideEncounterGauge();
            GameManager.Instance.Save();
            StartTutorial();
        }

        void Update()
        {
            if (CurrentStep == TutorialStep.Begin || CurrentStep == TutorialStep.Movement)
            {
                // Keep encounter gauge at in tutorial 0
                GameManager.Instance.EncounterManager.ResetGauge();
            }
        }

        void OnDestroy()
        {
            Instance = null;
            _battlePointSubscription?.Dispose();
            _statPointSubscription?.Dispose();
        }

        private void StartTutorial()
        {
            CurrentStep = TutorialStep.Begin;
            _waypoint.SetActive(false);
            ScriptManager.Instance.StartScript("tutorial_script", PracticeMovement);
        }
        
        private void PracticeMovement()
        {
            CurrentStep = TutorialStep.Movement;
            _waypoint.SetActive(true);
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
            _tutorialPanel.SetActive(true);
            _tutorialText.text = Utils.GetCutsceneText("tutorial_objective-2");
        }

        private IEnumerator EndStatPointTutorialCoroutine()
        {
            yield return new WaitForSeconds(1);
            EndStatPointTutorial();
        }

        private void OnStatPointChanged(int statPoints)
        {
            if (statPoints == 0)
            {
                GameManager.Instance.HUDController.StatsController.Hide(false);
                EndStatPointTutorial();
            }
        }

        private void EndStatPointTutorial()
        {
            _statPointSubscription?.Dispose();
            _tutorialPanel.SetActive(false);
            ScriptManager.Instance.StartScript("tutorial_script", EndTutorial, 15);

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
