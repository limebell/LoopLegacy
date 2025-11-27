using LoopLegacy.State;
using LoopLegacy.Manager;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using System.Numerics;
using LoopLegacy.Player;
using TMPro;
using LoopLegacy.Loader;
using LoopLegacy.Battle;

namespace LoopLegacy.UI.Controller
{
    public class ConsoleController : MonoBehaviour
    {
        public static ConsoleController Instance;

        [SerializeField] private GameObject _gameObject;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private TMP_Text _logs;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _closeButton;

        private List<string> _previousCommands = new List<string>();
        private int _currentCommandIndex = 0;

        private InputAction _toggleConsoleAction;
        private InputAction _submitAction;
        private InputAction _previousAction;
        private InputAction _nextAction;

        void Awake()
        {
            Instance = this;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            InitializeInputActions();

            _closeButton.onClick.AddListener(OnCloseButtonClicked);

            Hide();
        }

        void Update()
        {
            if (_toggleConsoleAction != null && _toggleConsoleAction.triggered)
            {
                OnToggleConsole();
            }
            if (_submitAction != null && _submitAction.triggered)
            {
                OnSubmit();
            }
            if (_previousAction != null && _previousAction.triggered)
            {
                OnPrevious();
            }
            if (_nextAction != null && _nextAction.triggered)
            {
                OnNext();
            }
        }

        public void Show()
        {
            if (Debug.IsDebug)
            {
                _gameObject.SetActive(true);
                _inputField.text = "";
            }
        }

        public void Hide()
        {
            _gameObject.SetActive(false);
        }

        private void InitializeInputActions()
        {
            var uiActionMap = InputSystem.actions.FindActionMap("UI");
            if (uiActionMap == null)
            {
                Debug.LogError("UI Action Map not found!");
                return;
            }

            _toggleConsoleAction = uiActionMap.FindAction("ToggleConsole");
            _submitAction = uiActionMap.FindAction("Submit");
            _previousAction = uiActionMap.FindAction("Previous");
            _nextAction = uiActionMap.FindAction("Next");
        }

        private void OnToggleConsole()
        {
            if (!_gameObject.activeSelf)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void OnCloseButtonClicked()
        {
            Hide();
        }

        private void OnSubmit()
        {
            if (gameObject.activeSelf && Debug.IsDebug)
            {
                ProcessCommand(_inputField.text);
                _inputField.text = "";
            }
        }

        private void OnPrevious()
        {
            if (gameObject.activeSelf &&
                _previousCommands.Count > 0)
            {
                if (_currentCommandIndex > 0)
                {
                    _currentCommandIndex--;
                    _inputField.text = _previousCommands[_currentCommandIndex];
                }
            }
        }

        private void OnNext()
        {
            if (gameObject.activeSelf &&
                _previousCommands.Count > 0)
            {
                if (_currentCommandIndex < _previousCommands.Count - 1)
                {
                    _currentCommandIndex++;
                    _inputField.text = _previousCommands[_currentCommandIndex];
                }
                else
                {
                    _currentCommandIndex = _previousCommands.Count;
                    _inputField.text = "";
                }
            }
        }

        public void ProcessCommand(string command)
        {
            Trace(command);
            _previousCommands.Add(command);
            _currentCommandIndex = _previousCommands.Count;
            var tokens = command.Split(' ');
            var commandName = tokens[0];
            var args = tokens.Skip(1).ToArray();
            switch (commandName)
            {
                case "calculateExp":
                    if (args.Length == 0)
                    {
                        var level = GameManager.Instance.GameState.PlayerStats.Level.Value;
                        var exp = PlayerStats.CalculateRequiredEXP(level);
                        Trace($"Exp for level {level}: {exp}");
                    }
                    else
                    {
                        var level = int.Parse(args[0]);
                        var exp = PlayerStats.CalculateRequiredEXP(level);
                        Trace($"Exp for level {level}: {exp}");
                    }
                    break;

                case "calculateTotalExp":
                    if (args.Length == 0)
                    {
                        var level = GameManager.Instance.GameState.PlayerStats.Level.Value;
                        var exp = CalculateTotalExp(level);
                        Trace($"Total Exp for level {level}: {exp}");
                    }
                    else
                    {
                        var level = int.Parse(args[0]);
                        var exp = CalculateTotalExp(level);
                        Trace($"Total Exp for level {level}: {exp}");
                    }
                    break;

                case "resetCharacter":
                    GameManager.Instance.GameState.PlayerStats.EXP.Value = 0;
                    GameManager.Instance.GameState.PlayerStats.Level.Value = 1;
                    GameManager.Instance.GameState.PlayerStats.InitializeStats(StatType.HP, PersistentGameState.Instance.GetBaseStat(StatType.HP));
                    GameManager.Instance.GameState.PlayerStats.InitializeStats(StatType.ATK, PersistentGameState.Instance.GetBaseStat(StatType.ATK));
                    GameManager.Instance.GameState.PlayerStats.InitializeStats(StatType.DEF, PersistentGameState.Instance.GetBaseStat(StatType.DEF));
                    GameManager.Instance.GameState.PlayerStats.InitializeStats(StatType.LUC, PersistentGameState.Instance.GetBaseStat(StatType.LUC));
                    break;

                case "addLevel":
                    if (args.Length != 1)
                    {
                        Trace("Usage: addLevel <level>");
                    }
                    else
                    {
                        var level = int.Parse(args[0]);
                        var result = new LevelUpResult
                        {
                            InitialLevel = GameManager.Instance.GameState.PlayerStats.Level.Value,
                            FinalLevel = GameManager.Instance.GameState.PlayerStats.Level.Value + level,
                            LevelUps = level,
                            InitialEXP = GameManager.Instance.GameState.PlayerStats.EXP.Value,
                            AddedEXP = 0,
                            RemainingEXP = 0
                        };
                        GameManager.Instance.GameState.PlayerStats.ApplyLevelUpResult(result);
                        Trace($"Added {level} level");
                    }
                    break;

                case "increaseLevel":
                    if (args.Length != 1)
                    {
                        Trace("Usage: increaseLevel <level>");
                    }
                    else
                    {
                        var level = int.Parse(args[0]);
                        GameManager.Instance.GameState.PlayerStats.Level.Value += level;
                        GameManager.Instance.GameState.PlayerStats.StatPoints.Value += level * 5;
                        Trace($"Increased level to {level}");
                    }
                    break;

                case "setLevel":
                    if (args.Length != 1)
                    {
                        Trace("Usage: setLevel <level>");
                    }
                    else
                    {
                        var level = int.Parse(args[0]);
                        GameManager.Instance.GameState.PlayerStats.Level.Value = level;
                        Trace($"Level set to {level}");
                    }
                    break;

                case "resetStats":
                    GameManager.Instance.InitializeStats();
                    Trace("Stats reset");
                    break;

                case "addGold":
                    if (args.Length != 1)
                    {
                        Trace("Usage: addGold <gold>");
                    }
                    else
                    {
                        var gold = int.Parse(args[0]);
                        PersistentGameState.Instance.AddGold(gold);
                        Trace($"Gold added {gold}");
                    }
                    break;

                case "setGold":
                    if (args.Length != 1)
                    {
                        Trace("Usage: setGold <gold>");
                    }
                    else
                    {
                        var gold = int.Parse(args[0]);
                        PersistentGameState.Instance.Gold.Value = gold;
                        Trace($"Gold set to {gold}");
                    }
                    break;

                case "addBattlePoint":
                    if (args.Length != 1)
                    {
                        Trace("Usage: addBattlePoint <amount>");
                    }
                    else
                    {
                        var amount = int.Parse(args[0]);
                        GameManager.Instance.GameState.PlayerStats.AddBattlePoint(amount);
                        Trace($"Battle point set to {GameManager.Instance.GameState.PlayerStats.BattlePoint.Value}");
                    }
                    break;

                case "addEquipment":
                    if (args.Length != 3)
                    {
                        Trace("Usage: addEquipment <itemId> <type> <quantity>");
                    }
                    else
                    {
                        var itemId = int.Parse(args[0]);
                        var type = args[1];
                        var quantity = int.Parse(args[2]);
                        for (int i = 0; i < quantity; i++)
                        {
                            PersistentGameState.Instance.InventoryState.AddEquipment(
                                (EquipmentType)Enum.Parse(typeof(EquipmentType), type),
                                itemId);
                        }
                        Trace($"Added {quantity} equipments {itemId}");
                    }
                    break;

                case "encounter":
                    if (args.Length == 0)
                    {
                        GameManager.Instance.EncounterManager.TriggerEncounter();
                        Trace("Encountered");
                    }
                    else if (args.Length == 1)
                    {
                        MonsterData monster;
                        monster = TableManager.GetBoss(args[0]);
                        GameManager.Instance.EncounterManager.ResetGauge();
                        BattleManager.Instance.StartBattle(monster, _ => { });
                        Trace($"Encountered {args[0]}");
                    }
                    else
                    {
                        Trace("Usage: encounter or encounter <monsterId>");
                    }

                    break;

                case "moveMap":
                    if (args.Length != 1)
                    {
                        Trace("Usage: moveMap <mapCode>");
                    }
                    else
                    {
                        var mapCode = args[0];
                        GameManager.Instance.MoveToMap(mapCode, new UnityEngine.Vector2(0, 0));
                    }
                    break;

                case "teleport":
                    if (args.Length != 2)
                    {
                        Trace("Usage: teleport <x> <y>");
                    }
                    else
                    {
                        var x = int.Parse(args[0]);
                        var y = int.Parse(args[1]);
                        PlayerMovement.Instance.SetPosition(new UnityEngine.Vector2(x, y));
                        CameraFollow.Instance.gameObject.transform.position = new UnityEngine.Vector3(x, y, -10);
                    }
                    break;

                case "enableEncounter":
                    GameManager.Instance.EncounterManager.SetEnabled(true);
                    break;

                case "disableEncounter":
                    GameManager.Instance.EncounterManager.SetEnabled(false);
                    break;

                case "unlockRelic":
                    if (args.Length != 2)
                    {
                        Trace("Usage: unlockRelic <relicId> <level>");
                    }
                    else
                    {
                        var relicId = int.Parse(args[0]);
                        var level = int.Parse(args[1]);
                        PersistentGameState.Instance.CodexState.UnlockRelicWithLevel(relicId, level);
                        Trace($"Unlocked relic {relicId} level {level}");
                    }
                    break;

                case "unlockAllRelics":
                    foreach (var relic in TableManager.GetAllRelics())
                    {
                        PersistentGameState.Instance.CodexState.UnlockRelicWithLevel(relic.id, 0);
                        Trace($"Unlocked relic {relic.id}");
                    }
                    break;

                case "addRerollCount":
                    if (args.Length != 1)
                    {
                        Trace("Usage: addRerollCount <count>");
                    }
                    else
                    {
                        var count = int.Parse(args[0]);
                        GameManager.Instance.GameState.RelicRewardRerollCount.Value += count;
                        Trace($"Increased reroll count to {GameManager.Instance.GameState.RelicRewardRerollCount.Value}");
                    }
                    break;

                case "addRelic":
                    if (args.Length != 1)
                    {
                        Trace("Usage: addRelic <relicId>");
                    }
                    else
                    {
                        var relicId = int.Parse(args[0]);
                        GameManager.Instance.AcquireRelic(relicId);
                        Trace($"Acquired relic {relicId}");
                    }
                    break;

                case "removeRelic":
                    if (args.Length != 1)
                    {
                        Trace("Usage: removeRelic <relicId>");
                    }
                    else
                    {
                        var relicId = int.Parse(args[0]);
                        var ownedRelics = GameManager.Instance.GameState.OwnedRelics.Value.ToList();
                        ownedRelics.Remove(ownedRelics.First(relic => relic.Id == relicId));
                        GameManager.Instance.GameState.OwnedRelics.Value = ownedRelics;
                        Trace($"Removed relic {relicId}");
                    }
                    break;

                case "rewardRelic":
                    if (args.Length != 1)
                    {
                        Trace("Usage: rewardRelic <weight>");
                    }
                    else
                    {
                        GameManager.Instance.RelicReward(int.Parse(args[0]), Array.Empty<Relic>());
                        Trace($"Rewarded relic with weight {args[0]}");
                    }
                    break;

                case "upgrade":
                    if (args.Length != 1)
                    {
                        Trace("Usage: upgrade <upgradeType>");
                    }
                    else
                    {
                        try
                        {
                            var upgradeType = (UpgradeType)Enum.Parse(typeof(UpgradeType), args[0]);
                            PersistentGameState.Instance.HouseState.Upgrade(upgradeType);
                            Trace($"Upgraded {upgradeType}");
                        }
                        catch (Exception)
                        {
                            Trace($"Invalid upgrade type: {args[0]}");
                        }
                    }
                    break;

                case "upgradeAll":
                    foreach (var upgradeType in Enum.GetValues(typeof(UpgradeType)))
                    {
                        var upgrade = TableManager.GetUpgrade((UpgradeType)upgradeType);
                        for (int i = PersistentGameState.Instance.HouseState.GetUpgradeLevel((UpgradeType)upgradeType); i < upgrade.maxLevel - 1; i++)
                        {
                            PersistentGameState.Instance.HouseState.Upgrade((UpgradeType)upgradeType);
                        }

                        Trace($"Upgraded {upgradeType} to level {PersistentGameState.Instance.HouseState.GetUpgradeLevel((UpgradeType)upgradeType)}");
                    }
                    break;

                case "openShortcut":
                    if (args.Length != 1)
                    {
                        Trace("Usage: openShortcut <shortcutType>");
                    }
                    else
                    {
                        var shortcutType = (Shortcut)Enum.Parse(typeof(Shortcut), args[0]);
                        PersistentGameState.Instance.HouseState.OpenShortcut(shortcutType);
                        Trace($"Opened shortcut {shortcutType}");
                    }
                    break;

                case "addKillCount":
                    if (args.Length != 2)
                    {
                        Trace("Usage: addKillCount <monsterCode> <count>");
                    }
                    else
                    {
                        var monsterCode = args[0];
                        var count = int.Parse(args[1]);
                        for (int i = 0; i < count; i++)
                        {
                            PersistentGameState.Instance.CodexState.AddMobKillCount(monsterCode);
                        }
                        Trace($"Added {count} kill count for {monsterCode}");
                    }
                    break;
                
                case "sa": // Superman Mode
                    {
                        foreach (var upgradeType in Enum.GetValues(typeof(UpgradeType)))
                        {
                            var upgrade = TableManager.GetUpgrade((UpgradeType)upgradeType);
                            for (int i = PersistentGameState.Instance.HouseState.GetUpgradeLevel((UpgradeType)upgradeType); i < upgrade.maxLevel - 1; i++)
                            {
                                PersistentGameState.Instance.HouseState.Upgrade((UpgradeType)upgradeType);
                            }
                        }

                        foreach (var weapon in TableManager.GetEquipments(EquipmentType.Weapon))
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                PersistentGameState.Instance.InventoryState.AddEquipment(EquipmentType.Weapon, weapon.id);
                            }
                        }
                        
                        foreach (var armor in TableManager.GetEquipments(EquipmentType.Armor))
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                PersistentGameState.Instance.InventoryState.AddEquipment(EquipmentType.Armor, armor.id);
                            }
                        }

                        foreach (var relic in TableManager.GetAllRelics())
                        {
                            PersistentGameState.Instance.CodexState.UnlockRelicWithLevel(relic.id, relic.values.Length - 1);
                        }

                        PersistentGameState.Instance.SetAutoDistributeStats(true);
                        PersistentGameState.Instance.SetAutoDistributeRate(0, new int[] { 1, 2, 1, 0 });

                        Trace("U ARE NOW SUPERMAN!");
                    }
                    break;

                case "help":
                    Trace("Available commands:");
                    Trace("calculateExp <level>: Calculate the required experience for a given level");
                    Trace("calculateTotalExp <level>: Calculate the total experience for a given level");
                    Trace("resetCharacter: Reset the character's stats and level");
                    Trace("addLevel <level>: Add a level to the character");
                    Trace("increaseLevel <level>: Increase the character's level");
                    Trace("setLevel <level>: Set the character's level");
                    Trace("resetStats: Reset the stats");
                    Trace("addGold <gold>: Add gold to the player's inventory");
                    Trace("setGold <gold>: Set the player's gold");
                    Trace("addBattlePoint <amount>: Add battle points to the player's stats");
                    Trace("addEquipment <itemId> <type> <quantity>: Add equipment to the player's inventory");
                    Trace("encounter: Encounter a monster");
                    Trace("encounter <monsterId>: Encounter a specific monster");
                    Trace("moveMap <mapCode>: Move to a specific map");
                    Trace("teleport <x> <y>: Teleport to a specific position");
                    Trace("enableEncounter: Enable encounter");
                    Trace("disableEncounter: Disable encounter");
                    Trace("unlockRelic <relicId> <level>: Unlock a relic with a specific level");
                    Trace("unlockAllRelics: Unlock all relics");
                    Trace("addRerollCount <count>: Add reroll count to the player's stats");
                    Trace("addRelic <relicId>: Add a relic to the player's inventory");
                    Trace("removeRelic <relicId>: Remove a relic from the player's inventory");
                    Trace("rewardRelic <weight>: Reward a relic from a monster");
                    Trace("upgrade <upgradeType>: Upgrade a specific upgrade");
                    Trace("upgradeAll: Upgrade all upgrades");
                    Trace("openShortcut <shortcutType>: Open a specific shortcut");
                    Trace("addKillCount <monsterCode> <count>: Add kill count for a specific monster");
                    Trace("sa: Superman Mode");
                    Trace("help: Show this help message");

                    break;

                default:
                    Trace($"Unknown command: {commandName}");
                    Trace("Type 'help' to see the list of available commands");
                    break;
            }
        }

        public void Trace(string message, Color color = default(Color))
        {
            _logs.text += message + "\n";
            // 레이아웃을 즉시 강제 업데이트
            LayoutRebuilder.ForceRebuildLayoutImmediate(_logs.rectTransform);
            _scrollRect.verticalNormalizedPosition = 0;
        }

        private BigInteger CalculateTotalExp(int level)
        {
            BigInteger totalExp = 0;
            for (int i = 1; i <= level; i++)
            {
                totalExp += PlayerStats.CalculateRequiredEXP(i);
            }
            return totalExp;
        }
    }
}
