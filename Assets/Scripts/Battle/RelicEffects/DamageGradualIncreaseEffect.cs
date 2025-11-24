namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 점차 강화 효과 (첫 합에서는 피해 감소, 합이 지날수록 피해 증가)
    /// </summary>
    public class DamageGradualIncreaseEffect : RelicEffect
    {
        private readonly int initialDamageReduction;
        private readonly int damageIncreasePerTurn;

        public DamageGradualIncreaseEffect(string effectType, int initialDamageReduction, int damageIncreasePerTurn) : base(effectType)
        {
            this.initialDamageReduction = initialDamageReduction;
            this.damageIncreasePerTurn = damageIncreasePerTurn;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            // 첫 턴에서는 피해 감소, 턴이 지날수록 피해 증가
            float damageMultiplierIncrease = (initialDamageReduction / 100f) + (context.TurnCount * damageIncreasePerTurn / 100f);
            context.DamageMultiplier += damageMultiplierIncrease;
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { initialDamageReduction, damageIncreasePerTurn });

        public int GetInitialDamageReduction() => initialDamageReduction;
        public int GetDamageIncreasePerTurn() => damageIncreasePerTurn;
    }
}
