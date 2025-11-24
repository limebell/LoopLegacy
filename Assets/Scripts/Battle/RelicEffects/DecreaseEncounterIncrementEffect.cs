namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 위험도 감소 효과 (위험도 게이지 증가량 감소)
    /// </summary>
    public class DecreaseEncounterIncrementEffect : RelicEffect
    {
        private readonly int encounterIncrementReductionPercentage;

        public DecreaseEncounterIncrementEffect(string effectType, int encounterIncrementReductionPercentage) : base(effectType)
        {
            this.encounterIncrementReductionPercentage = encounterIncrementReductionPercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            // 위험도 감소는 게임 플레이에서 적용되므로 여기서는 별도 처리 없음
            // 실제 적용은 EncounterManager에서 처리
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { encounterIncrementReductionPercentage });

        public int GetEncounterIncrementReductionPercentage() => encounterIncrementReductionPercentage;
    }
}
