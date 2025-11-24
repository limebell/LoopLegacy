namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 배틀포인트 증가 효과
    /// </summary>
    public class BattlePointBoostEffect : RelicEffect
    {
        private readonly int battlePointIncrease;

        public BattlePointBoostEffect(string effectType, int battlePointIncrease) : base(effectType)
        {
            this.battlePointIncrease = battlePointIncrease;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            // 배틀포인트 증가는 획득 시점에 적용
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { battlePointIncrease });

        public int GetBattlePointIncrease() => battlePointIncrease;
    }
}
