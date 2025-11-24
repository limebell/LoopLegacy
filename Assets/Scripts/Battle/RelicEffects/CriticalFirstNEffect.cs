namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 최초 N번째 공격이 반드시 치명타로 적중하는 효과
    /// </summary>
    public class CriticalFirstNEffect : RelicEffect
    {
        private readonly int criticalCount;

        public CriticalFirstNEffect(string effectType, int criticalCount) : base(effectType)
        {
            this.criticalCount = criticalCount;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            // 최초 N번째 공격동안 반드시 치명타 발생
            if (context.AttackCount < criticalCount)
            {
                context.CriticalRate = 1f;
            }
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { criticalCount });

        public int GetCriticalCount() => criticalCount;
    }
}