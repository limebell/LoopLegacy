namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 연속 공격 효과 (공격이 성공했을 때 한 번 더 공격할 확률)
    /// </summary>
    public class ChainRateEffect : RelicEffect
    {
        private readonly int chainProbability;

        public ChainRateEffect(string effectType, int chainPropability) : base(effectType)
        {
            this.chainProbability = chainPropability;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            // 연속 공격 확률은 BattleSimulator에서 계산
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { chainProbability });

        public int GetChainProbability() => chainProbability;
    }
}
