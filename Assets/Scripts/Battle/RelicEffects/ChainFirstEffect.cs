namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 첫 공격 연타 효과 (첫 합에 N번 연속으로 공격)
    /// </summary>
    public class ChainFirstEffect : RelicEffect
    {
        private readonly int comboCount;

        public ChainFirstEffect(string effectType, int comboCount) : base(effectType)
        {
            this.comboCount = comboCount;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            // 첫 공격 연타는 전투 중 첫 공격 시 적용되므로 여기서는 별도 처리 없음
            // 실제 적용은 BattleSimulator에서 처리
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { comboCount });

        public int GetComboCount() => comboCount;
    }
}
