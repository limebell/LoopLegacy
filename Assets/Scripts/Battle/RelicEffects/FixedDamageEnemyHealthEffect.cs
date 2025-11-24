namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 적 체력 비례 대미지 효과 (매 타격마다 적의 최대 체력의 일정 비율만큼의 고정 피해)
    /// </summary>
    public class FixedDamageEnemyHealthEffect : RelicEffect
    {
        private readonly int healthDamagePercentage;

        public FixedDamageEnemyHealthEffect(string effectType, int healthDamagePercentage) : base(effectType)
        {
            this.healthDamagePercentage = healthDamagePercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            if (context.MonsterData == null)
            {
                return;
            }
            
            context.FixedDamage += (int)(context.MonsterData.hp * healthDamagePercentage / 100f);
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { healthDamagePercentage });

        public int GetHealthDamagePercentage() => healthDamagePercentage;
    }
}
