using System.Collections.Generic;
using LoopLegacy.Loader;
using System;
using System.Linq;

namespace LoopLegacy.Manager
{
    public static class TableManager
    {
        private static Dictionary<string, RegionEntry> _regions;
        private static Dictionary<string, MonsterData> _monsters;
        private static Dictionary<string, MonsterData> _bosses;
        private static List<EquipmentData> _weapons;
        private static List<EquipmentData> _armors;
        private static List<RelicData> _relics;
        private static List<UpgradeData> _upgrades;

        private static object _lock = new object();
        private static bool isInitialized = false;

        public static bool IsInitialized => isInitialized;

        public static void LoadAllTables()
        {
            lock(_lock)
            {
                if (isInitialized) return;

                (_regions, _monsters) = RegionTableLoader.Load();
                //_monsters = MonsterTableLoader.Load();
                _bosses = MonsterTableLoader.LoadBosses();
                _weapons = EquipmentTableLoader.LoadWeapons();
                _armors = EquipmentTableLoader.LoadArmors();
                _relics = RelicTableLoader.Load();
                _upgrades = UpgradeTableLoader.Load();

                isInitialized = true;
            }
        }

        public static RegionEntry GetRegion(string code)
        {
            if (!isInitialized)
            {
                LoadAllTables();
            }

            if (!_regions.TryGetValue(code, out var region))
            {
                Debug.LogError($"[TableManager] Region code {code} not found");
                throw new Exception($"[TableManager] Region code {code} not found");
            }

            return region;
        }

        public static MonsterData GetMonster(string code)
        {
            if (!isInitialized)
            {
                LoadAllTables();
            }
            
            if (!_monsters.TryGetValue(code, out var monster))
            {
                Debug.LogError($"[TableManager] Monster code {code} not found");
                throw new Exception($"[TableManager] Monster code {code} not found");
            }

            return monster;
        }

        public static MonsterData[] GetAllMonsters()
        {
            if (!isInitialized)
            {
                LoadAllTables();
            }
            
            return _monsters.Values.ToArray();
        }

        public static MonsterData GetBoss(string code)
        {
            if (!isInitialized)
            {
                LoadAllTables();
            }
            
            if (!_bosses.TryGetValue(code, out var boss))
            {
                Debug.LogError($"[TableManager] Boss code {code} not found");
                throw new Exception($"[TableManager] Boss code {code} not found");
            }

            return boss;
        }

        public static MonsterData[] GetAllBosses()
        {
            if (!isInitialized)
            {
                LoadAllTables();
            }

            return _bosses.Values.ToArray();
        }

        public static EquipmentData GetEquipment(EquipmentType type, int id)
        {
            if (!isInitialized)
            {
                LoadAllTables();
            }
            
            try
            {
                switch(type)
                {
                    case EquipmentType.Weapon:
                        return _weapons[id];
                    case EquipmentType.Armor:
                        return _armors[id];
                    default:
                        throw new Exception($"Unknown equipment type: {type}");
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"[TableManager] 장비 정보 {id} 를 찾을 수 없습니다. : {e.Message}");
                throw e;
            }
        }

        public static List<EquipmentData> GetEquipments(EquipmentType type)
        {
            if (!isInitialized)
            {
                LoadAllTables();
            }

            switch(type)
            {
                case EquipmentType.Weapon:
                    return _weapons.ToList();
                case EquipmentType.Armor:
                    return _armors.ToList();
                default:
                    throw new Exception($"Unknown equipment type: {type}");
            }
        }

        public static RelicData GetRelic(string effectName)
        {
            if (!isInitialized)
            {
                LoadAllTables();
            }

            return _relics.Find(relic => relic.effectName == effectName);
        }

        public static List<RelicData> GetAllRelics()
        {
            if (!isInitialized)
            {
                LoadAllTables();
            }

            return _relics.ToList();
        }

        public static UpgradeData GetUpgrade(UpgradeType type)
        {
            if (!isInitialized)
            {
                LoadAllTables();
            }
            
            return _upgrades.Find(upgrade => upgrade.type == type);
        }
    }
} 