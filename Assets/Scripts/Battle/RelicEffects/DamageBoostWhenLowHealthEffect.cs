namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 버서커 효과 (HP가 특정 비율 이하일 때 입히는 피해량 증가)
    /// </summary>
    public class DamageBoostWhenLowHealthEffect : RelicEffect
    {
        private readonly int lowHealthPercentage;
        private readonly int damageIncreasePercentage;

        public DamageBoostWhenLowHealthEffect(string effectType, int lowHealthPercentage, int damageIncreasePercentage) : base(effectType)
        {
            this.lowHealthPercentage = lowHealthPercentage;
            this.damageIncreasePercentage = damageIncreasePercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            if (context.CurrentPlayerHP <= context.MaxPlayerHP * lowHealthPercentage / 100f)
            {
                context.DamageMultiplier += damageIncreasePercentage / 100f;
            }
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { lowHealthPercentage, damageIncreasePercentage });

        public int GetLowHealthPercentage() => lowHealthPercentage;
        
        public int GetDamageIncreasePercentage() => damageIncreasePercentage;
    }
}
