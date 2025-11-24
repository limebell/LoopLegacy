using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LoopLegacy.Loader
{
    public static class MonsterTableLoader
    {
        public static List<MonsterData> Load()
        {
            var list = new List<MonsterData>();
            var handle = Addressables.LoadAssetAsync<TextAsset>("Data/monster_table");
            try
            {
                TextAsset csv = handle.WaitForCompletion();
                var lines = csv.text.Split('\n');

                for (int i = 1; i < lines.Length; i++) // skip header
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var tokens = line.Split(',');
                    var code = tokens[0];
                    var properties = tokens.Skip(1).ToArray();
                    var data = ParseMonsterData(properties, code, MonsterType.Normal);
                    list.Add(data);
                }
            }
            finally
            {
                handle.Release();
            }

            return list;
        }
        
        public static Dictionary<string, MonsterData> LoadBosses()
        {
            var dict = new Dictionary<string, MonsterData>();
            var handle = Addressables.LoadAssetAsync<TextAsset>("Data/boss_table");
            try
            {
                TextAsset csv = handle.WaitForCompletion();
                var lines = csv.text.Split('\n');

                for (int i = 1; i < lines.Length; i++) // skip header
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var tokens = line.Split(',');
                    var code = tokens[0];
                    var properties = tokens.Skip(1).ToArray();

                    var data = ParseMonsterData(properties, code, MonsterType.Boss);

                    dict.Add(code, data);
                }
            }
            finally
            {
                handle.Release();
            }

            return dict;
        }

        public static MonsterData ParseMonsterData(string[] properties, string code, MonsterType type)
        {
            Sprite sprite = null;
            try
            {
                string path = $"Images/{(type == MonsterType.Boss ? "Enemies/Bosses" : "Enemies")}/{code}";
                sprite = Utils.LoadFirstSpriteAsync(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load enemy sprite: Images/{(type == MonsterType.Boss ? "Bosses" : "Enemies")}/{code}");
                throw e;
            }

            return new MonsterData
            {
                code = code,
                type = type,
                sprite = sprite,
                level = int.Parse(properties[0]),
                hp = int.Parse(properties[1]),
                atk = int.Parse(properties[2]),
                gold = int.Parse(properties[3]),
                exp = BigInteger.Parse(properties[4]),
                bp = int.Parse(properties[5]),
                drops = ParseDropEntries(properties[6]),
                actions = ParseActions(properties[7])
            };
        }

        private static IEnumerable<DropEntry> ParseDropEntries(string drops)
        {
            if (drops == string.Empty) return new List<DropEntry>();
            var dropEntries = drops.Split('|');
            return dropEntries.Select(entry =>
            {
                var tokens = entry.Split(':');
                var itemType = tokens[0] switch
                {
                    "w" => DropType.Weapon,
                    "a" => DropType.Armor,
                    "r" => DropType.Relic,
                    _ => throw new Exception($"Invalid item type: {tokens[0]}")
                };
                if (itemType == DropType.Relic)
                {
                    return new DropEntry
                    {
                        itemType = itemType,
                        relicEffectName = tokens[1],
                        relicLevel = int.Parse(tokens[2]),
                        dropRate = float.Parse(tokens[3])
                    };
                }
                else
                {
                    return new DropEntry
                    {
                        itemType = itemType,
                        itemId = int.Parse(tokens[1]),
                        dropRate = float.Parse(tokens[2])
                    };
                }
            });
        }

        private static IEnumerable<MonsterActionEntry> ParseActions(string actions)
        {
            if (actions == string.Empty) return new List<MonsterActionEntry>();
            var actionEntries = actions.Split('|');
            return actionEntries.Select(entry => new MonsterActionEntry
            {
                actionType = entry.Split(':')[0] switch
                {
                    "r" => MonsterActionType.RewardRelic,
                    "t" => MonsterActionType.Teleport,
                    _ => MonsterActionType.None
                },
                values = entry.Split(':').Skip(1).ToArray()
            });
        }
    }
}