using System;

namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 매 타격 적 공격력 약화 효과 (매 타격마다 적의 공격력이 감소)
    /// </summary>
    public class WeakEveryHitEffect : RelicEffect
    {
        private readonly int attackReductionPercentage;

        public WeakEveryHitEffect(string effectType, int attackReductionPercentage) : base(effectType)
        {
            this.attackReductionPercentage = attackReductionPercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            context.EnemyAttackReductionRate *= (float)Math.Pow(1.0f - attackReductionPercentage / 100f, context.AttackCount);
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { attackReductionPercentage });

        public int GetAttackReductionPercentage() => attackReductionPercentage;
    }
}
