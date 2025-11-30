using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LoopLegacy.Loader
{
    public static class RelicTableLoader
    {
        public static List<RelicData> Load()
        {
            var list = new List<RelicData>();
            var handle = Addressables.LoadAssetAsync<TextAsset>("Data/relic_table");
            try
            {
                TextAsset csv = handle.WaitForCompletion();
                if (csv == null)
                {
                    throw new Exception("relic_table을 로드할 수 없습니다.");
                }

                var lines = csv.text.Split('\n');

                for (int i = 1; i < lines.Length; i++) // skip header
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var tokens = line.Split(',');
                    if (tokens.Length < 3) continue;

                    int id = list.Count; // 동적으로 ID 할당
                    
                    Sprite sprite = null;
                    try
                    {
                        sprite = Addressables.LoadAssetAsync<Sprite>(
                            $"Images/Relics/{tokens[1]}").WaitForCompletion();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to load sprite for Images/Relics/{tokens[1]}: {e.Message}");
                    }

                    var data = new RelicData
                    {
                        sprite = sprite,
                        grade = (RelicGrade)int.Parse(tokens[0]),
                        effectName = tokens[1],
                        values = ParseValues(tokens[2]),
                        prices = ParsePrices(tokens[3])
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

        private static string[] ParseValues(string values)
        {
            if (values == string.Empty) return new string[0];
            return values.Split('|');
        }

        private static int[] ParsePrices(string prices)
        {
            if (prices == string.Empty) return Array.Empty<int>();
            return prices.Split('|').Select(int.Parse).ToArray();
        }
    }
} 