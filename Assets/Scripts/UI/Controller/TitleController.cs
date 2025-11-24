using LoopLegacy.Manager;
using LoopLegacy.Player;
using LoopLegacy.State;
using LoopLegacy.UI.Component;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LoopLegacy.UI.Controller
{
    public class TitleController : MonoBehaviour
    {
        [SerializeField]
        private GameObject _mainPanel;
        [SerializeField]
        private Button _startButton;
        [SerializeField]
        private Button _optionButton;
        [SerializeField]
        private Button _creditButton;
        [SerializeField]
        private GameObject _startPanel;
        [SerializeField]
        private Button _startCloseButton;
        [SerializeField]
        private SaveSlotElement[] _slots;

        [SerializeField]
        private OptionController _optionController;
        [SerializeField]
        private CreditController _creditController;

        [SerializeField]
        private TMP_Text _versionText;

        [SerializeField]
        private AudioClip _titleBGM;

        private InputAction _quitApplicationAction;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                int index = i;
                _slots[i].OnStartButtonClicked.AddListener(() => StartSlot(index));
                _slots[i].OnDeleteButtonClicked.AddListener(() => DeleteSlot(index));
            }

            _startButton.onClick.AddListener(OnStartButtonClicked);
            _optionButton.onClick.AddListener(OnOptionButtonClicked);
            _creditButton.onClick.AddListener(OnCreditButtonClicked);
            _startCloseButton.onClick.AddListener(OnStartBackButtonClicked);

            AudioManager.Instance.PlayBGM(_titleBGM, true);
            
            var uiActionMap = InputSystem.actions.FindActionMap("UI");
            _quitApplicationAction = uiActionMap.FindAction("Cancel");

            ShowMainPanel();
            HideStartPanel();
            _versionText.text = Application.version;
        }

        void Update()
        {
            if (_quitApplicationAction.triggered)
            {
                string message = Utils.GetUIString("quit-confirmation");
                ConfirmationController.Instance.ShowConfirmation(
                    message,
                    () => Application.Quit());
            }
        }

        public void ShowMainPanel()
        {
            _mainPanel.SetActive(true);
        }

        public void HideMainPanel()
        {
            _mainPanel.SetActive(false);
        }

        public void ShowStartPanel()
        {
            _startPanel.SetActive(true);
        }

        public void HideStartPanel()
        {
            _startPanel.SetActive(false);
        }

        private void OnStartButtonClicked()
        {
            UpdateSlots();
            HideMainPanel();
            ShowStartPanel();
        }

        private void OnOptionButtonClicked()
        {
            HideMainPanel();
            _optionController.Show(() => ShowMainPanel());
        }

        private void OnCreditButtonClicked()
        {
            HideMainPanel();
            _creditController.Show(() => ShowMainPanel());
        }

        private void OnStartBackButtonClicked()
        {
            HideStartPanel();
            ShowMainPanel();
        }

        private void UpdateSlots()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                string name = Utils.GetUIString("slot-name");
                _slots[i].SlotText = $"{name} {i + 1}";
                string savePath = PersistentGameState.GetSavePath(i);
                if (File.Exists(savePath))
                {
                    _slots[i].SetDataValid(true);
                    try
                    {
                        string json = File.ReadAllText(savePath);
                        var saveData = JsonUtility.FromJson<PersistentGameStateSaveData>(json);
                        string heroGenString = Utils.Ordinal(saveData.currentLoopCount + 1);
                        string genString = Utils.GetUIString("slot-description", new object[] { heroGenString });
                        _slots[i].GenText = genString;
                        var timespan = TimeSpan.Parse(saveData.playTime);
                        _slots[i].TimeText = $"{timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}";
                        if (saveData.isInGame && File.Exists(GameManager.GetSavePath(i)))
                        {
                            try
                            {
                                var dummyGameState = new GameState(File.ReadAllText(GameManager.GetSavePath(i)));
                                _slots[i].LevelText = $"Lv. {dummyGameState.PlayerStats.Level.Value}";
                                _slots[i].BpText = $"BP. {dummyGameState.PlayerStats.BattlePoint.Value}";
                            }
                            catch
                            {
                                string error = Utils.GetUIString("slot-data-corrupted");
                                _slots[i].LevelText = error;
                                _slots[i].BpText = Utils.GetUIString("slot-data-game-state-corrupted");
                            }
                        }
                        else
                        {
                            _slots[i].LevelText = "Lv. -";
                            _slots[i].BpText = Utils.GetRegionName("territory");
                        }
                    }
                    catch
                    {
                        string error = Utils.GetUIString("slot-data-corrupted");
                        _slots[i].GenText = error;
                    }
                }
                else
                {
                    _slots[i].SetDataValid(false);
                }
            }
        }

        private void StartSlot(int slotIndex)
        {
            string savePath = PersistentGameState.GetSavePath(slotIndex);
            try
            {
                if (File.Exists(savePath))
                {
                    // 기존 데이터 로드
                    PersistentGameState.LoadState(slotIndex);
                    if (PersistentGameState.Instance.IsInGame)
                    {
                        var path = GameManager.GetSavePath(PersistentGameState.Instance.CurrentSlotIndex);
                        if (!File.Exists(path))
                        {
                            Debug.LogError($"[TitleController] Game state file not found. (path: {path})");
                            FadeController.LoadScene("Territory", FadeController.MAP_MOVE_DELAY);
                            return;
                        }
                        try
                        {
                            var dummyGameState = new GameState(File.ReadAllText(path));
                            var mapCode = dummyGameState.CurrentMapCode;
                            var position = dummyGameState.PlayerPosition;
                            FadeController.LoadSceneWithMiddleScene(
                                $"G-{mapCode}",
                                "PreGame",
                                delay: FadeController.MAP_MOVE_DELAY,
                                onLoadComplete: () => PlayerMovement.Instance.SetPosition(position));
                        }
                        catch
                        {
                            Debug.LogError($"[TitleController] Error loading game state. (path: {path})");
                            FadeController.LoadScene("Territory", FadeController.MAP_MOVE_DELAY);
                            return;
                        }
                    }
                    else
                    {
                        FadeController.LoadScene("Territory", FadeController.MAP_MOVE_DELAY);
                    }
                }
                else
                {
                    // 새 게임 시작
                    PersistentGameState.CreateDefaultState(slotIndex);
                    CutsceneManager.LoadCutsceneScene(
                        "initial_cutscene",
                        () => FadeController.LoadSceneWithMiddleScene(
                            "G-Start",
                            "PreGame",
                            FadeController.MAP_MOVE_DELAY,
                            onLoadComplete: () => {
                                PlayerMovement.Instance.SetPosition(new Vector3(-0.5f, 0.0f, 0));
                            }));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[TitleController] Error starting slot {slotIndex}: {e.Message}");
                ConfirmationController.Instance.ShowWarning(
                    Utils.GetUIString("slot-data-corrupted"));
            }
        }

        private void DeleteSlot(int slotIndex)
        {
            string savePath = PersistentGameState.GetSavePath(slotIndex);
            if (File.Exists(savePath))
            {
                try
                {
                    string message = Utils.GetUIString("delete-slot-confirmation", new object[] { slotIndex + 1 });
                    ConfirmationController.Instance.ShowConfirmation(
                        message,
                        () =>
                        {
                            File.Delete(savePath);
                            string gameStatePath = GameManager.GetSavePath(slotIndex);
                            if (File.Exists(gameStatePath))
                            {
                                File.Delete(gameStatePath);
                            }
                            UpdateSlots();
                        });
                }
                catch (Exception e)
                {
                    Debug.LogError($"슬롯 {slotIndex} 데이터 삭제 중 오류: {e.Message}");
                }
            }
        }
    }
}
