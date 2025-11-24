namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 치명타 피해 증가 효과
    /// </summary>
    public class CriticalDamageEffect : RelicEffect
    {
        private readonly int criticalDamagePercentage;

        public CriticalDamageEffect(string effectType, int criticalDamagePercentage) : base(effectType)
        {
            this.criticalDamagePercentage = criticalDamagePercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            context.CriticalDamageMultiplier += criticalDamagePercentage / 100f;
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { criticalDamagePercentage });

        public int GetCriticalDamagePercentage() => criticalDamagePercentage;
    }
}
