using UnityEngine;

namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 점차 약화 효과 (첫 합에서는 피해 증가, 합이 지날수록 피해 감소)
    /// </summary>
    public class DamageGradualDecreaseEffect : RelicEffect
    {
        private readonly int initialDamageIncrease;
        private readonly int damageDecreasePerTurn;

        public DamageGradualDecreaseEffect(string effectType, int initialDamageIncrease, int damageDecreasePerTurn) : base(effectType)
        {
            this.initialDamageIncrease = initialDamageIncrease;
            this.damageDecreasePerTurn = damageDecreasePerTurn;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            // 첫 턴에서는 피해 증가, 턴이 지날수록 피해 감소
            float damageMultiplierIncrease = (initialDamageIncrease / 100f) - (context.TurnCount * damageDecreasePerTurn / 100f);
            damageMultiplierIncrease = Mathf.Max(-0.9f, damageMultiplierIncrease);
            context.DamageMultiplier += damageMultiplierIncrease;
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { initialDamageIncrease, damageDecreasePerTurn });

        public int GetInitialDamageIncrease() => initialDamageIncrease;
        public int GetDamageDecreasePerTurn() => damageDecreasePerTurn;
    }
}
