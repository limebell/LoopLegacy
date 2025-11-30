using LoopLegacy.Battle;
using LoopLegacy.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LoopLegacy.State
{
    public class CodexState
    {
        private readonly List<string> _visitedRegions;
        private readonly Dictionary<string, int> _mobKillCount;
        private readonly Dictionary<string, int> _relicLevels;

        public CodexState()
        {
            _visitedRegions = new List<string>();
            _mobKillCount = TableManager.GetAllMonsters().ToDictionary(data => data.code, _ => 0);
            foreach (var boss in TableManager.GetAllBosses())
            {
                _mobKillCount.Add(boss.code, 0);
            }

            _relicLevels = TableManager.GetAllRelics().ToDictionary(data => data.effectName, _ => -1);
            _relicLevels["stat_boost_hp"] = 0;
            _relicLevels["stat_boost_atk"] = 0;
            _relicLevels["stat_boost_def"] = 0;
            _relicLevels["stat_boost_luc"] = 0;
        }

        public bool IsRegionVisited(string code)
        {
            return _visitedRegions.Contains(code);
        }

        public void VisitRegion(string code)
        {
            if (!_visitedRegions.Contains(code))
            {
                _visitedRegions.Add(code);
            }
        }

        public void AddMobKillCount(string code)
        {
            if (!_mobKillCount.ContainsKey(code))
            {
                string errorMessage =
                    $"CodexState: mobKillCount does not contain key: {code}";
                Debug.LogError(errorMessage);
                return;
            }

            _mobKillCount[code]++;
        }

        public int GetMobKillCount(string code)
        {
            if (!_mobKillCount.TryGetValue(code, out int count))
            {
                string errorMessage =
                    $"CodexState: mobKillCount does not contain key: {code}";
                Debug.LogError(errorMessage);
                return 0;
            }
            
            return count;
        }

        public void UnlockRelicWithLevel(string effectName, int level)
        {
            if (level < 0)
            {
                throw new ArgumentException("Relic level must be greater than or equal to 0");
            }

            if (!_relicLevels.ContainsKey(effectName))
            {
                string errorMessage =
                    $"CodexState: relic does not contain key: {effectName}";
                Debug.LogError(errorMessage);
                return;
            }

            if (_relicLevels[effectName] >= level)
            {
                Debug.Log($"CodexState: relic {effectName} level {level} already unlocked");
                return;
            }

            _relicLevels[effectName] = level;
        }

        public Relic GetRelic(string effectName)
        {
            if (!_relicLevels.TryGetValue(effectName, out int level))
            {
                return null;
            }

            if (level < 0)
            {
                return null;
            }
            
            return new Relic(TableManager.GetRelic(effectName), level);
        }

        public List<Relic> GetAvailableRelics()
        {
            List<Relic> relics = new List<Relic>();
            foreach (var data in _relicLevels)
            {
                if (data.Value >= 0)
                {
                    relics.Add(new Relic(TableManager.GetRelic(data.Key), data.Value));
                }
            }

            return relics;
        }

        public string ToJson()
        {
            var saveData = new CodexStateSaveData
            {
                visitedRegions = _visitedRegions.OrderBy(code => code).ToArray(),
                mobKillCount = _mobKillCount.Select(data => new DictionaryElement { key = data.Key, value = data.Value }).ToArray(),
                relicLevels = _relicLevels.Select(data => new DictionaryElement { key = data.Key, value = data.Value }).ToArray(),
            };
            return JsonUtility.ToJson(saveData);
        }

        public static CodexState FromJson(string json)
        {
            var saveData = JsonUtility.FromJson<CodexStateSaveData>(json);
            var codexState = new CodexState();
            foreach (var data in saveData.mobKillCount)
            {
                if (codexState._mobKillCount.ContainsKey(data.key))
                {
                    codexState._mobKillCount[data.key] = data.value;
                }
                else
                {
                    string errorMessage =
                        $"CodexState: mobKillCount does not contain key: {data.key}";
                    Debug.LogError(errorMessage);
                }
            }

            foreach (var data in saveData.visitedRegions)
            {
                codexState._visitedRegions.Add(data);
            }

            foreach (var data in saveData.relicLevels)
            {
                if (!codexState._relicLevels.ContainsKey(data.key))
                {
                    string errorMessage =
                        $"CodexState: relic does not contain key: {data.key}";
                    Debug.LogError(errorMessage);
                    continue;
                }

                codexState._relicLevels[data.key] = data.value;
            }

            return codexState;
        }
    }

    [Serializable]
    public class CodexStateSaveData
    {
        public string[] visitedRegions;
        public DictionaryElement[] mobKillCount;
        public DictionaryElement[] relicLevels;
    }

    [Serializable]
    public class DictionaryElement
    {
        public string key;
        public int value;
    }
}