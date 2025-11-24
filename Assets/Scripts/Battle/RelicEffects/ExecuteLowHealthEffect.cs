namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 피낮은 적 처형 효과 (최대 체력이 일정 비율 이하인 적을 즉시 처형)
    /// </summary>
    public class ExecuteLowHealthEffect : RelicEffect
    {
        private readonly int executeThresholdPercentage;

        public ExecuteLowHealthEffect(string effectType, int executeThresholdPercentage) : base(effectType)
        {
            this.executeThresholdPercentage = executeThresholdPercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            if (context.MonsterData == null)
            {
                return;
            }
            
            if (context.CurrentMonsterHP <= context.MonsterData.hp * executeThresholdPercentage / 100f)
            {
                context.ShouldExecute = true;
            }
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { executeThresholdPercentage });

        public int GetExecuteThresholdPercentage() => executeThresholdPercentage;
    }
}
