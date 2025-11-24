namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 드롭률 증가 효과 (네잎클로버)
    /// </summary>
    public class DropBoostEffect : RelicEffect
    {
        private readonly float dropRateMultiplier;

        public DropBoostEffect(string effectType, float dropRateMultiplier) : base(effectType)
        {
            this.dropRateMultiplier = dropRateMultiplier;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            context.DropRateMultiplier += dropRateMultiplier / 100f;
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { dropRateMultiplier });

        public float GetDropRateMultiplier() => dropRateMultiplier;
    }
}
