using LoopLegacy.Battle;
using LoopLegacy.Loader;
using LoopLegacy.Region;
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

        public ReactiveProperty<IReadOnlyDictionary<string, RegionEffect>> AppliedRegionEffects { get; private set; }

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
            AppliedRegionEffects = new ReactiveProperty<IReadOnlyDictionary<string, RegionEffect>>(new Dictionary<string, RegionEffect>());
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
                saveData.ownedRelics?.Select(effectName => PersistentGameState.Instance.CodexState.GetRelic(effectName)).ToList() ?? new List<Relic>());
            AppliedRegionEffects =
                new ReactiveProperty<IReadOnlyDictionary<string, RegionEffect>>(
                    saveData.appliedRegionEffects?.ToDictionary(entry => entry.regionCode, entry => new RegionEffect(entry.type, entry.duration))
                    ?? new Dictionary<string, RegionEffect>());
            CurrentMapCode = saveData.currentMapCode;
            PlayerPosition = new Vector2(saveData.playerPositionX, saveData.playerPositionY);
            CurrentEncounterGauge = saveData.currentEncounterGauge;
            DroppedItems = saveData.droppedItems?.ToList() ?? new List<DropEntryData>();
            IsAdvertised = saveData.isAdvertised;
            IsRestartingWithGold = saveData.isRestartingWithGold;
        }

        public void AddDroppedItem(DropEntryData droppedItem)
        {
            if (droppedItem.itemType == DropType.Weapon || droppedItem.itemType == DropType.Armor)
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
            else if (droppedItem.itemType == DropType.Relic)
            {
                if (DroppedItems.Any(item => item.itemType == droppedItem.itemType && item.relicEffectName == droppedItem.relicEffectName))
                {
                    DropEntryData existingItem = DroppedItems.Find(item => item.itemType == droppedItem.itemType && item.relicEffectName == droppedItem.relicEffectName);
                    existingItem.relicLevel = Math.Max(existingItem.relicLevel, droppedItem.relicLevel);
                }
                else
                {
                    DroppedItems.Add(droppedItem);
                }
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
                ownedRelics = OwnedRelics.Value.Select(relic => relic.EffectName).ToArray(),
                appliedRegionEffects = AppliedRegionEffects.Value.Select(entry => new RegionEffectData { regionCode = entry.Key, type = entry.Value.Type, duration = entry.Value.Duration }).ToArray(),
                currentMapCode = CurrentMapCode,
                playerPositionX = PlayerPosition.x,
                playerPositionY = PlayerPosition.y,
                currentEncounterGauge = CurrentEncounterGauge,
                droppedItems = DroppedItems.ToArray(),
                isAdvertised = IsAdvertised,
                isRestartingWithGold = IsRestartingWithGold,
            });
        }

        public void AddRegionEffect(string regionCode, RegionEffectType regionEffectType, int duration)
        {
            if (!AppliedRegionEffects.Value.ContainsKey(regionCode))
            {
                Dictionary<string, RegionEffect> newAppliedRegionEffects = new Dictionary<string, RegionEffect>(AppliedRegionEffects.Value);
                newAppliedRegionEffects[regionCode] = new RegionEffect(regionEffectType, duration);
                AppliedRegionEffects.Value = newAppliedRegionEffects;
            }
        }

        public RegionEffect GetRegionEffect(string regionCode)
        {
            if (AppliedRegionEffects.Value.TryGetValue(regionCode, out RegionEffect regionEffect))
            {
                return regionEffect;
            }
            return new RegionEffect(RegionEffectType.None, 0);
        }

        public void UpdateRegionEffect()
        {
            foreach (var entry in AppliedRegionEffects.Value)
            {
                entry.Value.ReduceDuration();
            }

            AppliedRegionEffects.Value = AppliedRegionEffects.Value.Where(entry => entry.Value.Duration > 0).ToDictionary(entry => entry.Key, entry => entry.Value);
        }
    }

    [Serializable]
    public class GameStateSaveData
    {
        public string playerStatsData;
        public string combatInfoData;
        public string[] defeatedBosses;
        public int relicRewardRerollCount;
        public string[] ownedRelics;
        public RegionEffectData[] appliedRegionEffects;
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
        public string relicEffectName;
        public int relicLevel;
        public int count;
    }

    [Serializable]
    public class RegionEffectData
    {
        public string regionCode;
        public RegionEffectType type;
        public int duration;
    }
} 