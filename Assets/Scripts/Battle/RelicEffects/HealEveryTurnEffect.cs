namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 매 턴 회복 효과 (매 턴 최대 체력의 일정 비율만큼 회복)
    /// </summary>
    public class HealEveryTurnEffect : RelicEffect
    {
        private readonly int healAmount;

        public HealEveryTurnEffect(string effectType, int healAmount) : base(effectType)
        {
            this.healAmount = healAmount;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            // 매 턴 회복은 전투 중 턴 시작 시 적용되므로 여기서는 별도 처리 없음
            // 실제 적용은 BattleManager에서 처리
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { healAmount });

        public int GetHealAmount() => healAmount;
    }
}
