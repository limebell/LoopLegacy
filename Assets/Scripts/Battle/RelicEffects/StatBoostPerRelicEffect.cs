using LoopLegacy.Manager;

namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 유물 비례 스탯 효과 (보유한 유물에 비례해 각 스탯이 증가)
    /// </summary>
    public class StatBoostPerRelicEffect : RelicEffect
    {
        private readonly int statBoostAmount;

        public StatBoostPerRelicEffect(string effectType, int statBoostAmount) : base(effectType)
        {
            this.statBoostAmount = statBoostAmount;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            var relicCount = GameManager.Instance.GameState.OwnedRelics.Value.Count;
            int amount = relicCount * statBoostAmount;
            context.HPBoost += amount;
            context.DEFBoost += amount;
            context.ATKBoost += amount;
            context.LUCBoost += amount;
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { statBoostAmount });

        public int GetStatIncreasePercentage() => statBoostAmount;
    }
}
