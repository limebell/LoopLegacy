namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 최대 체력일 때 대미지 증가 효과
    /// </summary>
    public class DamageBoostWhenFullHealthEffect : RelicEffect
    {
        private readonly int increasePercentage;

        public DamageBoostWhenFullHealthEffect(string effectType, int increasePercentage) : base(effectType)
        {
            this.increasePercentage = increasePercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            if (context.CurrentPlayerHP == context.MaxPlayerHP)
            {
                context.DamageMultiplier += increasePercentage / 100f;
            }
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { increasePercentage });

        public int GetIncreasePercentage() => increasePercentage;
    }
}
