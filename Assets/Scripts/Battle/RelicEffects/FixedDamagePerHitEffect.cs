using LoopLegacy.Manager;

namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 타격당 고정 피해 증가 효과 (타격한 횟수마다 고정 피해 증가)
    /// </summary>
    public class FixedDamagePerHitEffect : RelicEffect
    {
        private readonly int fixedDamage;

        public FixedDamagePerHitEffect(string effectType, int fixedDamage) : base(effectType)
        {
            this.fixedDamage = fixedDamage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            if (GameManager.Instance.TryGetRelic<FixedDamagePerHitEffect>(out Relic relic))
            {
                context.FixedDamage += relic.Stack * fixedDamage;
            }
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { fixedDamage });

        public int GetFixedDamage() => fixedDamage;
    }
}

