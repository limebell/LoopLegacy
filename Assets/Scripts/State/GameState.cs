using LoopLegacy.Battle;
using LoopLegacy.Loader;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LoopLegacy.State
{
    public class GameState
    {
        public PlayerStats PlayerStats { get; private set; }

        public CombatInfo CombatInfo { get; private set; }

        public List<string> DefeatedBosses { get; private set; }

        public ReactiveProperty<int> RelicRewardRerollCount { get; private set; }

        public ReactiveProperty<IReadOnlyList<Relic>> OwnedRelics { get; private set; }

        public List<DropEntryData> DroppedItems { get; private set; }

        public string CurrentMapCode;
        public Vector2 PlayerPosition;
        public float CurrentEncounterGauge;
        public bool IsAdvertised;
        public bool IsRestartingWithGold;

        public GameState()
        {
            PlayerStats = new PlayerStats();
            CombatInfo = new CombatInfo();
            DefeatedBosses = new List<string>();
            int relicRewardRerollCount = PersistentGameState.Instance.HouseState.GetUpgradeValue(UpgradeType.RelicRewardRerollCount);
            RelicRewardRerollCount = new ReactiveProperty<int>(relicRewardRerollCount);
            OwnedRelics = new ReactiveProperty<IReadOnlyList<Relic>>(new List<Relic>());
            DroppedItems = new List<DropEntryData>();
            IsAdvertised = false;
            IsRestartingWithGold = false;
            InitializeDefaultState();
        }

        // PersistentGameState가 반드시 먼저 로드 되어 있어야 함
        public GameState(string json)
        {
            var saveData = JsonUtility.FromJson<GameStateSaveData>(json);
            PlayerStats = new PlayerStats(saveData.playerStatsData);
            CombatInfo = new CombatInfo(saveData.combatInfoData);
            DefeatedBosses = saveData.defeatedBosses?.ToList() ?? new List<string>();
            RelicRewardRerollCount = new ReactiveProperty<int>(saveData.relicRewardRerollCount);
            OwnedRelics = new ReactiveProperty<IReadOnlyList<Relic>>(
                saveData.ownedRelics?.Select(id => PersistentGameState.Instance.CodexState.GetRelic(id)).ToList() ?? new List<Relic>());
            CurrentMapCode = saveData.currentMapCode;
            PlayerPosition = new Vector2(saveData.playerPositionX, saveData.playerPositionY);
            CurrentEncounterGauge = saveData.currentEncounterGauge;
            DroppedItems = saveData.droppedItems?.ToList() ?? new List<DropEntryData>();
            IsAdvertised = saveData.isAdvertised;
            IsRestartingWithGold = saveData.isRestartingWithGold;
        }

        public void AddDroppedItem(DropEntryData droppedItem)
        {
            if (DroppedItems.Any(item => item.itemType == droppedItem.itemType && item.itemId == droppedItem.itemId))
            {
                DroppedItems.Find(item => item.itemType == droppedItem.itemType && item.itemId == droppedItem.itemId).count += droppedItem.count;
            }
            else
            {
                DroppedItems.Add(droppedItem);
            }
        }

        public void InitializeDefaultState()
        {
            PlayerStats.InitializeDefaultValues();
            DefeatedBosses = new List<string>();
            RelicRewardRerollCount.Value = PersistentGameState.Instance.HouseState.GetUpgradeValue(UpgradeType.RelicRewardRerollCount);
            OwnedRelics.Value = new List<Relic>();
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(new GameStateSaveData
            {
                playerStatsData = PlayerStats.ToJson(),
                combatInfoData = CombatInfo.ToJson(),
                defeatedBosses = DefeatedBosses.ToArray(),
                relicRewardRerollCount = RelicRewardRerollCount.Value,
                ownedRelics = OwnedRelics.Value.Select(relic => relic.Id).ToArray(),
                currentMapCode = CurrentMapCode,
                playerPositionX = PlayerPosition.x,
                playerPositionY = PlayerPosition.y,
                currentEncounterGauge = CurrentEncounterGauge,
                droppedItems = DroppedItems.ToArray(),
                isAdvertised = IsAdvertised,
                isRestartingWithGold = IsRestartingWithGold,
            });
        }
    }

    [Serializable]
    public class GameStateSaveData
    {
        public string playerStatsData;
        public string combatInfoData;
        public string[] defeatedBosses;
        public int relicRewardRerollCount;
        public int[] ownedRelics;
        public string currentMapCode;
        public float playerPositionX;
        public float playerPositionY;
        public float currentEncounterGauge;
        public DropEntryData[] droppedItems;
        public bool isAdvertised;
        public bool isRestartingWithGold;
    }

    [Serializable]
    public class DropEntryData
    {
        public DropType itemType;
        public int itemId;
        public int relicLevel;
        public int count;
    }
} 