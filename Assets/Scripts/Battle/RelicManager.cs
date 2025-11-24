using LoopLegacy.Manager;
using LoopLegacy.Battle.RelicEffects;
using System.Collections.Generic;

namespace LoopLegacy.Battle
{
    public class RelicManager
    {
        // Relic 효과 적용
        public void ApplyEffects(RelicEffectContext context)
        {
            var ownedRelics = GameManager.Instance.GameState.OwnedRelics.Value;
            
            // 개별 Relic 효과 적용
            foreach (var relic in ownedRelics)
            {
                relic.Effect.ApplyEffect(context);
            }
        }

        // Relic 효과로 인한 스탯 보정값 계산 (전투 중이 아닐 때도 사용)
        public Dictionary<StatType, int> GetStatBoost()
        {
            RelicEffectContext context = new RelicEffectContext
            {
                HPBoost = 0,
                DEFBoost = 0,
                ATKBoost = 0,
                LUCBoost = 0,
            };
            var ownedRelics = GameManager.Instance.GameState.OwnedRelics.Value;
            foreach (var relic in ownedRelics)
            {
                relic.Effect.ApplyEffect(context);
            }

            return new Dictionary<StatType, int>
            {
                { StatType.HP, context.HPBoost },
                { StatType.DEF, context.DEFBoost },
                { StatType.ATK, context.ATKBoost },
                { StatType.LUC, context.LUCBoost },
            };
        }
    }
}
