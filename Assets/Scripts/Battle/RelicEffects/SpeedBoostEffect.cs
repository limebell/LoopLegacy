namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 이동 속도 증가 효과
    /// </summary>
    public class SpeedBoostEffect : RelicEffect
    {
        private readonly int speedIncreasePercentage;

        public SpeedBoostEffect(string effectType, int speedIncreasePercentage) : base(effectType)
        {
            this.speedIncreasePercentage = speedIncreasePercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            // 이동 속도 증가는 게임 플레이에서 적용되므로 여기서는 별도 처리 없음
            // 실제 적용은 GameManager에서 처리
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { speedIncreasePercentage });

        public int GetSpeedIncreasePercentage() => speedIncreasePercentage;
    }
}
