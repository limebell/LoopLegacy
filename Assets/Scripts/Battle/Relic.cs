using LoopLegacy.Battle.RelicEffects;
using LoopLegacy.Loader;
using LoopLegacy.State;
using System;
using System.Linq;
using UnityEngine;

namespace LoopLegacy.Battle
{
    public class Relic
    {
        public int Id { get; private set; }
        public RelicGrade Grade { get; private set; }
        public int Level { get; private set; }
        public Sprite Sprite { get; private set; }
        public RelicEffect Effect { get; private set; }

        // 특수한 유물들에 사용되는 스택 (ex HitFixedDamageEffect)
        private int _stack;
        public int Stack
        {
            get
            {
                if (Effect is StatBoostCollectEffect statBoostCollectEffect)
                {
                    return PersistentGameState.Instance.InventoryState.GetOwnedEquipments(statBoostCollectEffect.GetEquipmentType())
                        .Count(c => c == InventoryState.MAX_EQUIPMENT_DUPLICATE_COUNT);
                }
                return _stack;
            }
            set => _stack = value;
        }

        public Relic(RelicData relicData, int level)
        {
            if (level < 0)
            {
                throw new ArgumentException("Relic level must be greater than or equal to 0");
            }

            Id = relicData.id;
            Grade = relicData.grade;
            if (level >= relicData.values.Length)
            {
                Debug.LogError($"Relic level of {relicData.effectName} is greater than the number of values: {level} >= {relicData.values.Length}");
                level = relicData.values.Length - 1;
            }
            Level = level;
            Sprite = relicData.sprite;
            Effect = RelicEffectFactory.CreateEffect(relicData.effectName, relicData.values[Level]);
            _stack = 0;
        }
    }
} 