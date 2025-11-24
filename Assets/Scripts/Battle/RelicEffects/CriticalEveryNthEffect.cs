namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 매 N번째 크리티컬 효과
    /// </summary>
    public class CriticalEveryNthEffect : RelicEffect
    {
        private readonly int criticalInterval;

        public CriticalEveryNthEffect(string effectType, int criticalInterval) : base(effectType)
        {
            this.criticalInterval = criticalInterval;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            if ((context.AttackCount + 1) % criticalInterval == 0)
            {
                context.CriticalRate = 1f;
            }
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { criticalInterval });

        public int GetCriticalInterval() => criticalInterval;
    }
}
