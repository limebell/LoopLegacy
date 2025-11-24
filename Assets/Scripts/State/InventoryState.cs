using LoopLegacy.Manager;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace LoopLegacy.State
{
    public class InventoryState
    {
        public const int MAX_EQUIPMENT_DUPLICATE_COUNT = 10;

        private List<int> _ownedWeapons;
        private List<int> _ownedArmors;

        public InventoryState()
        {
            _ownedWeapons = TableManager.GetEquipments(EquipmentType.Weapon).Select(_ => 0).ToList();
            _ownedArmors = TableManager.GetEquipments(EquipmentType.Armor).Select(_ => 0).ToList();
        }

        public void InitializeDefaultState()
        {
            // 기본 장비 추가
            AddEquipment(EquipmentType.Weapon, 0); // Basic Weapon
            AddEquipment(EquipmentType.Armor, 0); // Basic Armor
        }

        public int[] GetOwnedEquipments(EquipmentType type)
        {
            switch (type)
            {
                case EquipmentType.Weapon:
                    return _ownedWeapons.ToArray();
                case EquipmentType.Armor:
                    return _ownedArmors.ToArray();
                default:
                    throw new Exception($"Unknown equipment type: {type}");
            }
        }

        public void AddEquipment(EquipmentType type, int id)
        {
            switch (type)
            {
                case EquipmentType.Weapon:
                    if (_ownedWeapons[id] < MAX_EQUIPMENT_DUPLICATE_COUNT)
                    {
                        _ownedWeapons[id]++;
                    }
                    break;
                case EquipmentType.Armor:
                    if (_ownedArmors[id] < MAX_EQUIPMENT_DUPLICATE_COUNT)
                    {
                        _ownedArmors[id]++;
                    }
                    break;
            }
        }

        public string ToJson()
        {
            var saveData = new InventoryStateSaveData
            {
                ownedWeapons = _ownedWeapons,
                ownedArmors = _ownedArmors,
            };
            return JsonUtility.ToJson(saveData);
        }

        public static InventoryState FromJson(string json)
        {
            var saveData = JsonUtility.FromJson<InventoryStateSaveData>(json);
            var inventoryState = new InventoryState();
            for (int i = 0; i < inventoryState._ownedWeapons.Count; i++)
            {
                if (saveData.ownedWeapons.Count > i)
                {
                    inventoryState._ownedWeapons[i] = saveData.ownedWeapons[i];
                }
            }
            for (int i = 0; i < inventoryState._ownedArmors.Count; i++)
            {
                if (saveData.ownedArmors.Count > i)
                {
                    inventoryState._ownedArmors[i] = saveData.ownedArmors[i];
                }
            }
            return inventoryState;
        }
    }

    [Serializable]
    public class InventoryStateSaveData
    {
        public List<int> ownedWeapons;
        public List<int> ownedArmors;
    }
}