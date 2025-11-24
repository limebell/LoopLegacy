namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 보유한 골드 들고 처음부터 다시 시작 효과 (이 유물을 획득하는 순간 보유한 골드를 유지한 채로 이 루프를 다시 시작)
    /// </summary>
    public class RestartWithGoldEffect : RelicEffect
    {
        public RestartWithGoldEffect(string effectType) : base(effectType)
        {
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            // 골드 재시작은 유물 획득 시 적용되므로 여기서는 별도 처리 없음
            // 실제 적용은 GameManager에서 처리
        }

        public override string GetDescription() => GetDescriptionWithArgs(new object[] { });
    }
}
