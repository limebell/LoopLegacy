using System.Numerics;

namespace LoopLegacy.Battle.RelicEffects
{
    // 경험치 획득 증가
    public class ExpBoostEffect : RelicEffect
    {
        private readonly int boostPercentage;

        public ExpBoostEffect(string effectType, int boostPercentage) : base(effectType)
        {
            this.boostPercentage = boostPercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            context.ExpMultiplier += boostPercentage / 100f;
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { boostPercentage });
    }
}
