namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 적에게 입히는 피해량이 증가하는 대신에 자신이 받는 피해량이 증가하는 효과
    /// </summary>
    public class DamageBoostWeakEffect : RelicEffect
    {
        private readonly int increasePercentage;

        public DamageBoostWeakEffect(string effectType, int increasePercentage) : base(effectType)
        {
            this.increasePercentage = increasePercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            context.DamageMultiplier += increasePercentage / 100f;

            context.EnemyAttackReductionRate *= 1.2f;
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { increasePercentage });

        public int GetIncreasePercentage() => increasePercentage;
    }
}
