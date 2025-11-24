using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LoopLegacy.Loader
{
    public static class RegionTableLoader
    {
        public static (Dictionary<string, RegionEntry>, Dictionary<string, MonsterData>) Load()
        {
            var regionEntryDict = new Dictionary<string, RegionEntry>();
            var monsterDataDict = new Dictionary<string, MonsterData>();
            var handle = Addressables.LoadAssetAsync<TextAsset>("Data/region_table");
            try
            {
                TextAsset csv = handle.WaitForCompletion();
                var lines = csv.text.Split('\n').Skip(2).ToArray(); // skip header

                for (int i = 0; i < lines.Count(); i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (line.StartsWith("#")) continue;

                    var tokens = line.Trim().Split(',');
                    if (tokens.Length != 3)
                    {
                        Debug.LogError($"[RegionTableLoader] Invalid line: {line}");
                        continue;
                    }

                    string regionCode = tokens[0];
                    var monsters = ParseMonsterList(regionCode, tokens[2]);
                    var regionEntry = new RegionEntry
                    {
                        code = regionCode,
                        label = tokens[1],
                        monsters = monsters
                    };

                    if (regionEntryDict.ContainsKey(regionEntry.code))
                    {
                        Debug.LogError($"[RegionTableLoader] Region code mismatch: {regionEntry.code} already exists");
                    }

                    regionEntryDict[regionEntry.code] = regionEntry;
                    for (int j = 0; j < monsters.Count(); j++)
                    {
                        var monsterLine = lines[i + j + 1];
                        var properties = monsterLine.Split(',');
                        var monsterData = MonsterTableLoader.ParseMonsterData(
                            properties, $"{regionEntry.code}_{j}", MonsterType.Normal);
                        monsterDataDict[monsterData.code] = monsterData;
                    }

                    i += monsters.Count();
                }
            }
            finally
            {
                handle.Release();
            }

            return (regionEntryDict, monsterDataDict);
        }

        private static List<MonsterSpawnInfo> ParseMonsterList(string regionCode, string monsterField)
        {
            var result = new List<MonsterSpawnInfo>();
            var spawnRates = monsterField.Split('|');
            for (int index = 0; index < spawnRates.Length; index++)
            {
                var spawnRate = spawnRates[index];
                float rate = float.Parse(spawnRate);
                result.Add(new MonsterSpawnInfo($"{regionCode}_{index}", rate));
            }

            return result;
        }
    }

    public class RegionEntry
    {
        public string code;
        public string label;
        public List<MonsterSpawnInfo> monsters = new();
    }

    public class MonsterSpawnInfo
    {
        public string monsterCode;
        public float spawnRate;

        public MonsterSpawnInfo(string code, float rate)
        {
            monsterCode = code;
            spawnRate = rate;
        }
    }
}