using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LoopLegacy.Loader
{
    public static class UpgradeTableLoader
    {
        public static List<UpgradeData> Load()
        {
            var list = new List<UpgradeData>();
            var handle = Addressables.LoadAssetAsync<TextAsset>("Data/upgrade_table");
            try
            {
                TextAsset csv = handle.WaitForCompletion();
            if (csv == null)
                {
                    throw new Exception("upgrade_table을 로드할 수 없습니다.");
                }

                var lines = csv.text.Split('\n');

                for (int i = 1; i < lines.Length; i++) // skip header
                {
                    var line = lines[i].Trim();
                    int id = i - 1;
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var tokens = line.Split(',');
                    var entry = new UpgradeData
                    {
                        id = id,
                        type = GetUpgradeType(tokens[0]),
                        maxLevel = int.Parse(tokens[1]),
                        values = tokens[2].Split('|').Select(int.Parse).ToList(),
                        prices = tokens[3].Split('|').Select(int.Parse).ToList(),
                    };
                    list.Add(entry);
                }
            }
            finally
            {
                handle.Release();
            }

            return list;
        }

        private static UpgradeType GetUpgradeType(string type)
        {
            return type switch
            {
                "territory_level" => UpgradeType.TerritoryLevel,
                "base_hp" => UpgradeType.BaseHP,
                "base_atk" => UpgradeType.BaseATK,
                "base_def" => UpgradeType.BaseDEF,
                "base_luc" => UpgradeType.BaseLUC,
                "boost_exp" => UpgradeType.BoostExp,
                "boost_gold" => UpgradeType.BoostGold,
                "instant_encounter" => UpgradeType.InstantEncounter,
                "movement_speed" => UpgradeType.MovementSpeed,
                "encounter_rate" => UpgradeType.EncounterRate,
                "max_relic_count" => UpgradeType.MaxRelicCount,
                "relic_reward_choice_count" => UpgradeType.RelicRewardChoiceCount,
                "relic_reward_reroll_count" => UpgradeType.RelicRewardRerollCount,
                "relic_reward_rarity" => UpgradeType.RelicRewardRarity,
                "initial_relic_select" => UpgradeType.InitialRelicSelect,
                "library_management" => UpgradeType.LibraryManagement,
                _ => throw new Exception($"Invalid upgrade type: {type}"),
            };
        }
    }
    

    public class UpgradeData
    {
        public int id;
        public UpgradeType type;
        public int maxLevel;
        public List<int> values;
        public List<int> prices;
    }

    public enum UpgradeType : byte
    {
        TerritoryLevel,
        BaseHP,
        BaseATK,
        BaseDEF,
        BaseLUC,
        BoostExp,
        BoostGold,
        InstantEncounter,
        MovementSpeed,
        EncounterRate,
        MaxRelicCount,
        RelicRewardChoiceCount,
        RelicRewardRerollCount,
        RelicRewardRarity,
        InitialRelicSelect,
        LibraryManagement,
    }
} 