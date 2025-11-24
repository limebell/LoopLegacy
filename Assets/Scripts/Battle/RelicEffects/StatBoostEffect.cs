namespace LoopLegacy.Battle.RelicEffects
{
    // 스탯 합적용 효과
    public class StatBoostEffect : RelicEffect
    {
        private readonly StatType statType;
        private readonly int amount;

        public StatBoostEffect(string effectType, StatType statType, int amount) : base(effectType)
        {
            this.statType = statType;
            this.amount = amount;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            switch (statType)
            {
                case StatType.HP:
                    context.HPBoost += amount;
                    break;
                case StatType.DEF:
                    context.DEFBoost += amount;
                    break;
                case StatType.ATK:
                    context.ATKBoost += amount;
                    break;
                case StatType.LUC:
                    context.LUCBoost += amount;
                    break;
            }
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { amount });

        public StatType GetStatType() => statType;
        public int GetAmount() => amount;
    }
}
