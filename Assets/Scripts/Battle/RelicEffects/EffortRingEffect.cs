using LoopLegacy.State;

namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 노력의 반지 효과 (용사 세대 수의 배수만큼 모든 스탯이 증가)
    /// 사용 x
    /// </summary>
    public class EffortRingEffect : RelicEffect
    {
        private readonly int statMultiplier;

        public EffortRingEffect(string effectType, int statMultiplier) : base(effectType)
        {
            this.statMultiplier = statMultiplier;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            int heroGeneration = PersistentGameState.Instance.CurrentLoopCount + 1;
            int statBoost = heroGeneration * statMultiplier;
            context.HPBoost += statBoost;
            context.ATKBoost += statBoost;
            context.DEFBoost += statBoost;
            context.LUCBoost += statBoost;
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { statMultiplier });

        public int GetStatMultiplier() => statMultiplier;
    }
}
