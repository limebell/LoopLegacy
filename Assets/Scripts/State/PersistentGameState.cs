using R3;
using UnityEngine;
using LoopLegacy.Manager;
using LoopLegacy.Loader;
using System.IO;
using System;
using System.Linq;
using LoopLegacy.Battle;
using System.Collections.Generic;

namespace LoopLegacy.State
{
    [Serializable]
    public class EquipmentSaveData
    {
        public string name;
        public int enchantmentLevel;
        public int baseValue;
        public float multiplier;
    }

    public class PersistentGameState
    {
        private static PersistentGameState instance;

        public static PersistentGameState Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PersistentGameState();
                    instance.InitializeDefaultState();
                    #if UNITY_EDITOR
                    instance.InitializeDefaultStateForEditor();
                    #endif
                }
                return instance;
            }
        }

        public int CurrentLoopCount { get; private set; }
        public ReactiveProperty<int>[] CurrentEquipments { get; private set; }

        // 현재 선택된 슬롯 번호
        public int CurrentSlotIndex { get; private set; } = -1;

        // 영구적으로 유지되는 골드
        public ReactiveProperty<int> Gold { get; private set; }

        // 영지 업그레이드 관련 상태
        public HouseState HouseState { get; private set; }

        // 인벤토리 상태
        public InventoryState InventoryState { get; private set; }

        // 스탯 자동 분배 설정
        public const int AUTO_DISTRIBUTE_PRESET_COUNT = 5;
        public bool AutoDistributeStats { get; private set; }
        private int _currentAutoDistributePreset;
        private int[][] _autoDistributeRate;

        // 도감 상태
        public CodexState CodexState { get; private set; }

        // 누적 레벨
        public int AccumulatedLevel { get; private set; }

        // 튜토리얼 완료 여부
        public Dictionary<TutorialType, bool> CompletedTutorials { get; private set; }

        // 현재 게임 상태
        public bool IsInGame { get; private set; }

        // 로드 시점
        public DateTime LastLoadTime { get; private set;}

        // 플레이타임
        public TimeSpan PlayTime { get; private set; }

        private PersistentGameState()
        {
            CurrentLoopCount = 0;
            CurrentEquipments = new ReactiveProperty<int>[Enum.GetValues(typeof(EquipmentType)).Length];
            for (int i = 0; i < CurrentEquipments.Length; i++)
            {
                CurrentEquipments[i] = new ReactiveProperty<int>(-1);
            }

            Gold = new ReactiveProperty<int>(0);

            HouseState = new HouseState();
            InventoryState = new InventoryState();
            CodexState = new CodexState();

            AccumulatedLevel = 0;

            AutoDistributeStats = false;
            _currentAutoDistributePreset = 0;
            _autoDistributeRate = new int[AUTO_DISTRIBUTE_PRESET_COUNT][];
            for (int i = 0; i < AUTO_DISTRIBUTE_PRESET_COUNT; i++)
            {
                _autoDistributeRate[i] = new int[Enum.GetValues(typeof(StatType)).Length];
                Array.Fill(_autoDistributeRate[i], 0);
            }

            CompletedTutorials = new Dictionary<TutorialType, bool>();
            foreach (var tutorialType in Enum.GetValues(typeof(TutorialType)))
            {
                CompletedTutorials[(TutorialType)tutorialType] = false;
            }
            IsInGame = false;
        }

        public static void CreateDefaultState(int slotIndex)
        {
            var state = new PersistentGameState();
            state.SetCurrentSlotIndex(slotIndex);
            state.InitializeDefaultState();
            instance = state;
        }

        private void InitializeDefaultState()
        {
            CurrentLoopCount = 0;
            Gold.Value = 0;

            // 기본 스탯 초기화
            InventoryState.InitializeDefaultState();
            AccumulatedLevel = 0;

            // 기본 장비 장착
            Equip(EquipmentType.Armor, 0);
            Equip(EquipmentType.Weapon, 0);

            IsInGame = false;
            LastLoadTime = DateTime.Now;
            PlayTime = TimeSpan.Zero;
            _autoDistributeRate[0][(int)StatType.HP] = 1;
            _autoDistributeRate[0][(int)StatType.ATK] = 2;
            _autoDistributeRate[0][(int)StatType.DEF] = 1;
            _autoDistributeRate[0][(int)StatType.LUC] = 0;
        }

        private void InitializeDefaultStateForEditor()
        {
            Gold.Value = 100000000;
            CompletedTutorials[TutorialType.GameStart] = true;
            CompletedTutorials[TutorialType.Territory] = true;
            foreach (var i in Enumerable.Range(0, TableManager.GetUpgrade(UpgradeType.TerritoryLevel).maxLevel - 1))
            {
                HouseState.Upgrade(UpgradeType.TerritoryLevel);
            }
            foreach (var monster in TableManager.GetAllMonsters())
            {
                CodexState.AddMobKillCount(monster.code);
            }
            foreach (var boss in TableManager.GetAllBosses())
            {
                CodexState.AddMobKillCount(boss.code);
            }
            AutoDistributeStats = true;

            foreach (var relic in TableManager.GetAllRelics())
            {
                CodexState.UnlockRelicWithLevel(relic.effectName, 0);
            }

            foreach (var equipment in TableManager.GetEquipments(EquipmentType.Weapon))
            {
                for (int i = 0; i < 1; i++)
                {
                    InventoryState.AddEquipment(EquipmentType.Weapon, equipment.id);
                }
            }

            foreach (var equipment in TableManager.GetEquipments(EquipmentType.Armor))
            {
                for (int i = 0; i < 1; i++)
                {
                    InventoryState.AddEquipment(EquipmentType.Armor, equipment.id);
                }
            }

            HouseState.Upgrade(UpgradeType.MaxRelicCount);
            HouseState.Upgrade(UpgradeType.MaxRelicCount);
            HouseState.Upgrade(UpgradeType.MaxRelicCount);

            HouseState.Upgrade(UpgradeType.RelicRewardChoiceCount);
            HouseState.Upgrade(UpgradeType.RelicRewardChoiceCount);

            HouseState.Upgrade(UpgradeType.BaseHP);
            HouseState.Upgrade(UpgradeType.BaseHP);
            HouseState.Upgrade(UpgradeType.BaseHP);
            HouseState.Upgrade(UpgradeType.BaseHP);
            HouseState.Upgrade(UpgradeType.BaseHP);
            HouseState.Upgrade(UpgradeType.BaseHP);
            HouseState.Upgrade(UpgradeType.BaseATK);
            HouseState.Upgrade(UpgradeType.BaseATK);
            HouseState.Upgrade(UpgradeType.BaseATK);
            HouseState.Upgrade(UpgradeType.BaseATK);
            HouseState.Upgrade(UpgradeType.BaseATK);
            HouseState.Upgrade(UpgradeType.BaseATK);
            HouseState.Upgrade(UpgradeType.BaseDEF);
            HouseState.Upgrade(UpgradeType.BaseDEF);
            HouseState.Upgrade(UpgradeType.BaseDEF);
            HouseState.Upgrade(UpgradeType.BaseDEF);
            HouseState.Upgrade(UpgradeType.BaseDEF);
            HouseState.Upgrade(UpgradeType.BaseDEF);
            HouseState.Upgrade(UpgradeType.BaseLUC);
            HouseState.Upgrade(UpgradeType.BaseLUC);
            HouseState.Upgrade(UpgradeType.BaseLUC);
            HouseState.Upgrade(UpgradeType.BaseLUC);
            HouseState.Upgrade(UpgradeType.BaseLUC);
            HouseState.Upgrade(UpgradeType.BoostExp);
            HouseState.Upgrade(UpgradeType.BoostExp);
            HouseState.Upgrade(UpgradeType.BoostGold);
            HouseState.Upgrade(UpgradeType.BoostGold);
        }

        private void SetCurrentSlotIndex(int slotIndex)
        {
            CurrentSlotIndex = slotIndex;
        }

        public void SaveState()
        {
            if (CurrentSlotIndex == -1)
            {
                Debug.Log("[PersistentGameState] Debug instance does not save.");
                return;
            }

            try
            {
                PlayTime += DateTime.Now - LastLoadTime;
                LastLoadTime = DateTime.Now;

                var saveData = new PersistentGameStateSaveData
                {
                    currentLoopCount = CurrentLoopCount,
                    gold = Gold.Value,
                    houseStateData = HouseState.ToJson(),
                    inventoryStateData = InventoryState.ToJson(),
                    codexStateData = CodexState.ToJson(),
                    accumulatedLevel = AccumulatedLevel,
                    currentWeapon = CurrentEquipments[(int)EquipmentType.Weapon].Value,
                    currentArmor = CurrentEquipments[(int)EquipmentType.Armor].Value,
                    autoDistributeStats = AutoDistributeStats,
                    currentAutoDistributePreset = _currentAutoDistributePreset,
                    autoDistributePresets = _autoDistributeRate.Select(x => new AutoDistributePresetSaveData { rate = x }).ToArray(),
                    isInGame = IsInGame,
                    playTime = PlayTime.ToString(),
                    completedTutorials = CompletedTutorials.Values.ToArray(),
                };

                string json = JsonUtility.ToJson(saveData);
                string path = GetSavePath(CurrentSlotIndex);
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[PersistentGameState] Error saving game state: {e.Message}");
            }
        }

        public static void LoadState(int slotIndex)
        {
            if (slotIndex == -1)
            {
                Debug.Log("[PersistentGameState] Debug mode does not load.");
                return;
            }

            string path = GetSavePath(slotIndex);
            if (!File.Exists(path))
            {
                Debug.Log("[PersistentGameState] No saved game state found. Initializing with default values.");
                return;
            }

            string json = File.ReadAllText(path);
            var saveData = JsonUtility.FromJson<PersistentGameStateSaveData>(json);

            var state = new PersistentGameState();

            // 상태 복원
            state.CurrentLoopCount = saveData.currentLoopCount;
            state.Gold.Value = saveData.gold;
            state.HouseState = HouseState.FromJson(saveData.houseStateData);
            state.InventoryState = InventoryState.FromJson(saveData.inventoryStateData);
            state.CodexState = CodexState.FromJson(saveData.codexStateData);
            state.AccumulatedLevel = saveData.accumulatedLevel;

            state.CurrentEquipments[(int)EquipmentType.Weapon].Value = saveData.currentWeapon;
            state.CurrentEquipments[(int)EquipmentType.Armor].Value = saveData.currentArmor;

            state.AutoDistributeStats = saveData.autoDistributeStats;
            state._currentAutoDistributePreset = saveData.currentAutoDistributePreset;
            state._autoDistributeRate = saveData.autoDistributePresets.Select(x => x.rate).ToArray();

            foreach (var tutorialType in Enum.GetValues(typeof(TutorialType)))
            {
                if ((int)tutorialType < saveData.completedTutorials.Length)
                {
                    state.CompletedTutorials[(TutorialType)tutorialType] = saveData.completedTutorials[(int)tutorialType];
                }
            }
            state.IsInGame = saveData.isInGame;
            state.LastLoadTime = DateTime.Now;
            state.PlayTime = TimeSpan.Parse(saveData.playTime);

            state.SetCurrentSlotIndex(slotIndex);
            instance = state;
        }

        public int GetBaseStat(StatType stat)
        {
            switch(stat)
            {
                case StatType.HP:
                    return HouseState.GetUpgradeValue(UpgradeType.BaseHP) + LibraryManager.GetStatBoost();
                case StatType.ATK:
                    return HouseState.GetUpgradeValue(UpgradeType.BaseATK) + LibraryManager.GetStatBoost();
                case StatType.DEF:
                    return HouseState.GetUpgradeValue(UpgradeType.BaseDEF) + LibraryManager.GetStatBoost();
                case StatType.LUC:
                    return HouseState.GetUpgradeValue(UpgradeType.BaseLUC) + LibraryManager.GetStatBoost();
                default:
                    throw new Exception($"[PersistentGameState] Invalid stat: {stat}");
            }
        }

        public int GetBoostExp()
        {
            return HouseState.GetUpgradeValue(UpgradeType.BoostExp);
        }

        public int GetBoostGold()
        {
            return HouseState.GetUpgradeValue(UpgradeType.BoostGold);
        }

        public int GetMaxRelicCount()
        {
            return HouseState.GetUpgradeValue(UpgradeType.MaxRelicCount);
        }

        public void IncrementLoopCount()
        {
            CurrentLoopCount++;
        }

        public void SetIsInGame(bool isInGame)
        {
            IsInGame = isInGame;
        }

        public void IncrementAccumulatedLevel(int amount)
        {
            AccumulatedLevel += amount;
        }

        public void AddGold(int amount)
        {
            if (amount < 0)
            {
                return;
            }

            if (Gold.Value > int.MaxValue - amount)
            {
                Gold.Value = int.MaxValue;
                return;
            }
            
            Gold.Value += amount;
        }

        public bool SpendGold(int amount)
        {
            if (amount < 0)
            {
                return false;
            }

            if (Gold.Value < amount)
            {
                Gold.Value = 0;
                return false;
            }
            
            Gold.Value -= amount;
            return true;
        }

        public Weapon GetCurrentWeapon()
        {
            var id = CurrentEquipments[(int)EquipmentType.Weapon].Value;
            if (id == -1)
            {
                return null;
            }
            return new Weapon(TableManager.GetEquipment(EquipmentType.Weapon, id), InventoryState.GetOwnedEquipments(EquipmentType.Weapon)[id]);
        }
        
        public Armor GetCurrentArmor()
        {
            var id = CurrentEquipments[(int)EquipmentType.Armor].Value;
            if (id == -1)
            {
                return null;
            }
            return new Armor(TableManager.GetEquipment(EquipmentType.Armor, id), InventoryState.GetOwnedEquipments(EquipmentType.Armor)[id]);
        }

        public void Equip(EquipmentType type, int id)
        {
            if (id < 0 || InventoryState.GetOwnedEquipments(type).Length < id)
            {
                Debug.LogError($"[PersistentGameState] Failed to equip equipment: {type} {id} equipment not found.");
                return;
            }

            var count = InventoryState.GetOwnedEquipments(type)[id];
            if (count == 0)
            {
                Debug.LogError($"[PersistentGameState] Failed to equip equipment: {type} {id} equipment has no count.");
                return;
            }

            CurrentEquipments[(int)type].Value = id;
            SaveState();
        }

        public void SetAutoDistributeStats(bool onOff)
        {
            AutoDistributeStats = onOff;
        }

        public void SetAutoDistributePreset(int preset)
        {
            _currentAutoDistributePreset = preset;
        }

        public void SetAutoDistributeRate(int preset, int[] rate)
        {
            for (int i = 0; i < _autoDistributeRate[preset].Length; i++)
            {
                _autoDistributeRate[preset][i] = rate[i] < 0 ? 0 : rate[i];
            }
        }

        public int GetCurrentAutoDistributePreset()
        {
            return _currentAutoDistributePreset;
        }

        public int[] GetAutoDistributeRate(int preset = -1)
        {
            if (preset == -1)
            {
                return _autoDistributeRate[_currentAutoDistributePreset];
            }

            return _autoDistributeRate[preset];
        }

        public void CompleteTutorial(TutorialType tutorialType)
        {
            if (CompletedTutorials.ContainsKey(tutorialType))
            {
                CompletedTutorials[tutorialType] = true;
            }
        }

        public static string GetSavePath(int slotIndex)
        {
            return Path.Combine(Application.persistentDataPath, $"persistent_game_state_{slotIndex}.json");
        }
    }

    [Serializable]
    internal class PersistentGameStateSaveData
    {
        public int currentLoopCount;
        public int gold;
        public string houseStateData;
        public string inventoryStateData;
        public string codexStateData;
        public int accumulatedLevel;
        public int currentWeapon;
        public int currentArmor;
        public bool autoDistributeStats;
        public int currentAutoDistributePreset;
        public AutoDistributePresetSaveData[] autoDistributePresets;
        public bool isInGame;
        public string playTime;
        public bool[] completedTutorials;
    }

    [Serializable]
    internal class AutoDistributePresetSaveData
    {
        public int[] rate;
    }
} 