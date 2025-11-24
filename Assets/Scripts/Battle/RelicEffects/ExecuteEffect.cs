namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 낮은 확률 적 즉사 효과 (매 타격 시 일정 확률로 적을 처형시킴)
    /// </summary>
    public class ExecuteEffect : RelicEffect
    {
        private readonly float executeProbability;

        public ExecuteEffect(string effectType, float executeProbability) : base(effectType)
        {
            this.executeProbability = executeProbability;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            if (UnityEngine.Random.Range(0f, 100f) < executeProbability)
            {
                context.ShouldExecute = true;
            }
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { executeProbability });

        public float GetExecuteProbability() => executeProbability;
    }
}
