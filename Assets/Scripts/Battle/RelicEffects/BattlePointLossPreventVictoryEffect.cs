namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 전투 승리 방어 효과 (전투에서 승리했을 때 일정 확률로 BP 감소 방지)
    /// </summary>
    public class BattlePointLossPreventVictoryEffect : RelicEffect
    {
        private readonly float damageIncrease;
        private readonly int protectionPercentage;

        public BattlePointLossPreventVictoryEffect(string effectType, float damageIncrease, int protectionPercentage) : base(effectType)
        {
            this.damageIncrease = damageIncrease;
            this.protectionPercentage = protectionPercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            context.DamageMultiplier += damageIncrease / 100f;
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { damageIncrease, protectionPercentage });

        public float GetDamageIncrease() => damageIncrease;

        public int GetProtectionPercentage() => protectionPercentage;
    }
}
