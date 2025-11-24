using UnityEngine;

namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 치명타 적중률 증가 효과
    /// </summary>
    public class CriticalRateEffect : RelicEffect
    {
        private readonly int criticalRatePercentage;

        public CriticalRateEffect(string effectType, int criticalRatePercentage) : base(effectType)
        {
            this.criticalRatePercentage = criticalRatePercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            context.CriticalRate = Mathf.Clamp01(context.CriticalRate + (criticalRatePercentage / 100f));
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { criticalRatePercentage });

        public int GetCriticalRatePercentage() => criticalRatePercentage;
    }
}
