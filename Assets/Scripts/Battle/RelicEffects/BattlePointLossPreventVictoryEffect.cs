namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 전투 승리 방어 효과 (전투에서 승리했을 때 일정 확률로 BP 감소 방지)
    /// </summary>
    public class BattlePointLossPreventVictoryEffect : RelicEffect
    {
        private readonly int protectionPercentage;

        public BattlePointLossPreventVictoryEffect(string effectType, int protectionPercentage) : base(effectType)
        {
            this.protectionPercentage = protectionPercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            // 전투 패배 방어는 전투 패배 시 적용되므로 여기서는 별도 처리 없음
            // 실제 적용은 GameManager에서 처리
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { protectionPercentage });

        public int GetProtectionPercentage() => protectionPercentage;
    }
}
