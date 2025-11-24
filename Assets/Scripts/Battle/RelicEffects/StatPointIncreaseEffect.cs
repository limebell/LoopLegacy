namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 스탯 크리스탈 효과 (레벨 업 당 얻는 스탯 포인트 증가)
    /// </summary>
    public class StatPointIncreaseEffect : RelicEffect
    {
        private readonly int increaseAmount;

        public StatPointIncreaseEffect(string effectType, int increaseAmount) : base(effectType)
        {
            this.increaseAmount = increaseAmount;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            // 스탯 크리스탈은 레벨 업 시 적용되므로 여기서는 별도 처리 없음
            // 실제 적용은 GameManager에서 처리
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { increaseAmount });

        public int GetIncreaseAmount() => increaseAmount;
    }
}
