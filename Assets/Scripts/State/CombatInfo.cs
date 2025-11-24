using UnityEngine;
using System;
using LoopLegacy.Loader;

namespace LoopLegacy.State
{
    public class CombatInfo
    {
        public int CombatCount { get; private set; }
        public int Wins { get; private set; }
        public int Losses { get; private set; }
        public int DefeatedBossesCount { get; private set; }

        public CombatInfo()
        {
            CombatCount = 0;
            Wins = 0;
            Losses = 0;
            DefeatedBossesCount = 0;
        }

        public CombatInfo(string json)
        {
            var saveData = JsonUtility.FromJson<CombatInfoSaveData>(json);
            CombatCount = saveData.combatCount;
            Wins = saveData.wins;
            Losses = saveData.losses;
            DefeatedBossesCount = saveData.defeatedBossesCount;
        }

        public void IncrementCombatCount(MonsterData monsterData)
        {
            CombatCount++;
        }

        public void IncrementWins(MonsterType monsterType)
        {
            Wins++;
            if (monsterType == MonsterType.Boss)
            {
                DefeatedBossesCount++;
            }
        }

        public void IncrementLosses()
        {
            Losses++;
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(new CombatInfoSaveData
            {
                combatCount = CombatCount,
                wins = Wins,
                losses = Losses,
                defeatedBossesCount = DefeatedBossesCount,
            });
        }

        [Serializable]
        public class CombatInfoSaveData
        {
            public int combatCount;
            public int wins;
            public int losses;
            public int defeatedBossesCount;
        }
    }
}