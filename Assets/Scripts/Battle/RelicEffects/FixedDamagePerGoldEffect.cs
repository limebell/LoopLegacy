using LoopLegacy.State;
using UnityEngine;

namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 보유 골드 대미지 증가 효과 (보유한 골드에 비례한 추가 고정 피해)
    /// </summary>
    public class FixedDamagePerGoldEffect : RelicEffect
    {
        private readonly float fixedDamagePerGold;

        public FixedDamagePerGoldEffect(string effectType, float fixedDamagePerGold) : base(effectType)
        {
            this.fixedDamagePerGold = fixedDamagePerGold;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            context.FixedDamage += Mathf.RoundToInt(PersistentGameState.Instance.Gold.Value * fixedDamagePerGold / 100);
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { fixedDamagePerGold });

        public float GetFixedDamagePerGold() => fixedDamagePerGold;
    }
}
