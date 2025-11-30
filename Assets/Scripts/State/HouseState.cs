using LoopLegacy.Loader;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using LoopLegacy.Manager;

namespace LoopLegacy.State
{
    // 영지 업그레이드 관련 상태 저장소
    public class HouseState
    {
        private Dictionary<UpgradeType, int> Upgrades;

        private Dictionary<Shortcut, bool> Shortcuts;

        public ReactiveProperty<int> TerritoryLevel { get; private set; }

        public bool CanViewCodex => TerritoryLevel.Value >= 2;

        public ReactiveProperty<string> InitialRelic { get; private set; }

        public HouseState()
        {
            TerritoryLevel = new ReactiveProperty<int>(0);
            InitialRelic = new ReactiveProperty<string>(string.Empty);
            Upgrades = new Dictionary<UpgradeType, int>();
            foreach (var upgrade in Enum.GetValues(typeof(UpgradeType)))
            {
                Upgrades[(UpgradeType)upgrade] = 0;
            }
            Shortcuts = new Dictionary<Shortcut, bool>();
            foreach (var shortcut in Enum.GetValues(typeof(Shortcut)))
            {
                Shortcuts[(Shortcut)shortcut] = false;
            }
#if UNITY_EDITOR
            Upgrades[UpgradeType.InstantEncounter] = 1;
#endif
        }

        public void Upgrade(UpgradeType type)
        {
            if (!Upgrades.ContainsKey(type))
            {
                Debug.LogError($"[HouseState] Upgrade type {type} does not exist");
                return;
            }

            if (Upgrades[type] >= TableManager.GetUpgrade(type).maxLevel - 1)
            {
                Debug.LogError($"[HouseState] Upgrade type {type} has reached the max level");
                return;
            }

            Upgrades[type]++;

            if (type == UpgradeType.TerritoryLevel)
            {
                TerritoryLevel.Value = Upgrades[type];

                if (TerritoryLevel.Value == 6)
                {
                    // 도서관 해금하면서 도서관관리 1레벨
                    Upgrades[UpgradeType.LibraryManagement] = 1;
                }
            }
        }

        public int GetUpgradeValue(UpgradeType type)
        {
            return TableManager.GetUpgrade(type).values[GetUpgradeLevel(type)];
        }

        public int GetUpgradeLevel(UpgradeType type)
        {
            if(Upgrades.ContainsKey(type))
            {
                return Upgrades[type];
            }
            else
            {
                Debug.LogError($"[HouseState] 존재하지 않는 업그레이드 타입: {type}");
                return -1;
            }
        }

        public void OpenShortcut(Shortcut shortcut)
        {
            Shortcuts[shortcut] = true;
        }

        public bool GetShortcut(Shortcut shortcut)
        {
            return Shortcuts[shortcut];
        }

        public void SetInitialRelic(string effectName)
        {
            if (PersistentGameState.Instance.CodexState.GetAvailableRelics().Any(relic => relic.EffectName == effectName))
            {
                InitialRelic.Value = effectName;
            }
            else
            {
                InitialRelic.Value = string.Empty;
            }
        }

        public string ToJson()
        {
            var saveData = new HouseStateSaveData
            {
                upgrades = Upgrades.Values.ToArray(),
                shortcuts = Shortcuts.Values.ToArray(),
                initialRelicEffectName = InitialRelic.Value,
            };

            return JsonUtility.ToJson(saveData);
        }

        public static HouseState FromJson(string json)
        {
            var saveData = JsonUtility.FromJson<HouseStateSaveData>(json);
            var houseState = new HouseState();
            for (int i = 0; i < saveData.upgrades.Length; i++)
            {
                if (i < Enum.GetValues(typeof(UpgradeType)).Length)
                {
                    if (saveData.upgrades[i] > TableManager.GetUpgrade((UpgradeType)i).maxLevel - 1)
                    {
                        Debug.LogError($"[HouseState] Upgrade level of {((UpgradeType)i).ToString()} is greater than the max level: {saveData.upgrades[i]} > {TableManager.GetUpgrade((UpgradeType)i).maxLevel - 1}");
                        saveData.upgrades[i] = TableManager.GetUpgrade((UpgradeType)i).maxLevel - 1;
                    }
                    houseState.Upgrades[(UpgradeType)i] = saveData.upgrades[i];
                }
            }
            for (int i = 0; i < saveData.shortcuts.Length; i++)
            {
                if (i < Enum.GetValues(typeof(Shortcut)).Length)
                {
                    houseState.Shortcuts[(Shortcut)i] = saveData.shortcuts[i];
                }
            }
            houseState.InitialRelic.Value = saveData.initialRelicEffectName;
            houseState.TerritoryLevel.Value = houseState.Upgrades[UpgradeType.TerritoryLevel];
            return houseState;
        }
    }

    [Serializable]
    public class HouseStateSaveData
    {
        public int[] upgrades;
        public bool[] shortcuts;
        public string initialRelicEffectName;
    }

    public enum Shortcut : byte
    {
        Desert,
        Castle,
    }
}