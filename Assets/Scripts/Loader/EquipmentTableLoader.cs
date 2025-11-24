using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LoopLegacy.Loader
{
    public static class EquipmentTableLoader
    {
        public static List<EquipmentData> LoadWeapons()
        {
            var list = new List<EquipmentData>();
            var handle = Addressables.LoadAssetAsync<TextAsset>("Data/weapon_table");
            try
            {
                TextAsset csv = handle.WaitForCompletion();
                if (csv == null)
                {
                    throw new Exception("weapon_table을 로드할 수 없습니다.");
                }

                var lines = csv.text.Split('\n');

                for (int i = 1; i < lines.Length; i++) // skip header
                {
                    var line = lines[i].Trim();
                    int id = i - 1;
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var tokens = line.Split(',');

                    Sprite sprite = null;
                    try
                    {
                        sprite = Addressables.LoadAssetAsync<Sprite>(
                            $"Images/Equipments/Weapons/{id}").WaitForCompletion();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to load sprite for weapons[{id}]: {e.Message}");
                    }

                    var data = new EquipmentData
                    {
                        id = id,
                        type = EquipmentType.Weapon,
                        sprite = sprite,
                        baseValue = int.Parse(tokens[0]),
                        multiplier = int.Parse(tokens[1]),
                        basePerLv = float.Parse(tokens[2]),
                        multPerLv = float.Parse(tokens[3]),
                        price = int.Parse(tokens[4])
                    };

                    list.Add(data);
                }
            }
            finally
            {
                handle.Release();
            }

            return list;
        }
            
        public static List<EquipmentData> LoadArmors()
        {
            var list = new List<EquipmentData>();
            var handle = Addressables.LoadAssetAsync<TextAsset>("Data/armor_table");
            try
            {
                TextAsset csv = handle.WaitForCompletion();
                if (csv == null)
                {
                    throw new Exception("equipment_table을 로드할 수 없습니다.");
                }

                var lines = csv.text.Split('\n');

                for (int i = 1; i < lines.Length; i++) // skip header
                {
                    var line = lines[i].Trim();
                    int id = i - 1;
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var tokens = line.Split(',');

                    Sprite sprite = null;
                    try
                    {
                        sprite = Addressables.LoadAssetAsync<Sprite>(
                            $"Images/Equipments/Armors/{id}").WaitForCompletion();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to load sprite for armors[{id}]: {e.Message}");
                    }

                    var data = new EquipmentData
                    {
                        id = id,
                        type = EquipmentType.Armor,
                        sprite = sprite,
                        baseValue = int.Parse(tokens[0]),
                        multiplier = int.Parse(tokens[1]),
                        basePerLv = float.Parse(tokens[2]),
                        multPerLv = float.Parse(tokens[3]),
                        price = int.Parse(tokens[4])
                    };

                    list.Add(data);
                }
            }
            finally
            {
                handle.Release();
            }

            return list;
        }
    }
    

    public class EquipmentData
    {
        public int id;
        public EquipmentType type;
        public Sprite sprite;
        public int baseValue;
        public int multiplier;
        public float basePerLv;
        public float multPerLv;
        public int price;
    }
} 