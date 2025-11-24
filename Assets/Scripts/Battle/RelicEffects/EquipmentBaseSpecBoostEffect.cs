namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 장비 기본스펙 증가 효과 (장비의 base-damage가 배수로 증가)
    /// </summary>
    public class EquipmentBaseSpecBoostEffect : RelicEffect
    {
        private readonly float boostSpecMultipliser;

        public EquipmentBaseSpecBoostEffect(string effectType, float boostSpecMultipliser) : base(effectType)
        {
            this.boostSpecMultipliser = boostSpecMultipliser;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            // 장비 기본스펙 증가는 전투 시작 시 적용되므로 여기서는 별도 처리 없음
            // 실제 적용은 BattleSimulator에서 처리
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { boostSpecMultipliser });

        public float GetEquipmentBaseSpecBoostPercentage() => boostSpecMultipliser;
    }
}
