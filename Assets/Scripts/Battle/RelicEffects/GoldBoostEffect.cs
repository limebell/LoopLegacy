namespace LoopLegacy.Battle.RelicEffects
{
    // 골드 획득 증가
    public class GoldBoostEffect : RelicEffect
    {
        private readonly int boostPercentage;

        public GoldBoostEffect(string effectType, int boostPercentage) : base(effectType)
        {
            this.boostPercentage = boostPercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            context.GoldMultiplier += boostPercentage / 100f;
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { boostPercentage });
    }
}
