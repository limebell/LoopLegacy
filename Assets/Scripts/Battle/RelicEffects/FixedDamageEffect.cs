namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 고정 대미지 추가 효과 (매 공격 당 고정 대미지를 입힘)
    /// </summary>
    public class FixedDamageEffect : RelicEffect
    {
        private readonly int fixedDamage;

        public FixedDamageEffect(string effectType, int fixedDamage) : base(effectType)
        {
            this.fixedDamage = fixedDamage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            context.FixedDamage += fixedDamage;
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { fixedDamage });

        public int GetFixedDamage() => fixedDamage;
    }
}
