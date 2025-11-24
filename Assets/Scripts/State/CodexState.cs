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
        private readonly List<int> _relicLevels;

        public CodexState()
        {
            _visitedRegions = new List<string>();
            _mobKillCount = TableManager.GetAllMonsters().ToDictionary(data => data.code, _ => 0);
            foreach (var boss in TableManager.GetAllBosses())
            {
                _mobKillCount.Add(boss.code, 0);
            }

            _relicLevels = TableManager.GetAllRelics().Select(_ => -1).ToList();
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

        public void UnlockRelicWithLevel(int id, int level)
        {
            if (level < 0)
            {
                throw new ArgumentException("Relic level must be greater than or equal to 0");
            }

            _relicLevels[id] = level;
        }

        public Relic GetRelic(int id)
        {
            if (id < 0 || id >= _relicLevels.Count)
            {
                return null;
            }
            
            int level = _relicLevels[id];
            if (level < 0)
            {
                return null;
            }
            
            return new Relic(TableManager.GetRelic(id), level);
        }

        public List<Relic> GetAvailableRelics()
        {
            List<Relic> relics = new List<Relic>();
            for (int i = 0; i < _relicLevels.Count; i++)
            {
                if (_relicLevels[i] >= 0)
                {
                    relics.Add(new Relic(TableManager.GetRelic(i), _relicLevels[i]));
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
                relicLevels = _relicLevels.ToArray(),
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

            for (int i = 0; i < TableManager.GetAllRelics().Count(); i++)
            {
                if (i < saveData.relicLevels.Length)
                    codexState._relicLevels[i] = saveData.relicLevels[i];
                else
                {
                    string errorMessage =
                        $"CodexState: relic mismatch: {saveData.relicLevels.Length} != " +
                        $"{TableManager.GetAllRelics().Count()}";
                    Debug.LogError(errorMessage);
                    codexState._relicLevels[i] = -1;
                }
            }

            return codexState;
        }
    }

    [Serializable]
    public class CodexStateSaveData
    {
        public string[] visitedRegions;
        public DictionaryElement[] mobKillCount;
        public int[] relicLevels;
    }

    [Serializable]
    public class DictionaryElement
    {
        public string key;
        public int value;
    }
}