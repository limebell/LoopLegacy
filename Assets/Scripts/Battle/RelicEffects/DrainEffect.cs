namespace LoopLegacy.Battle.RelicEffects
{
    // 상대에게 가한 대미지의 일정 퍼센트만큼 회복
    public class DrainEffect : RelicEffect
    {
        private readonly int drainPercentage;

        public DrainEffect(string effectType, int drainPercentage) : base(effectType)
        {
            this.drainPercentage = drainPercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            // BattleSimulator에서 사용되는 흡혈 퍼센트
            context.DrainPercentage = drainPercentage;
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { drainPercentage });
    }
}
