namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 첫 N번째 공격 회피 효과 (몬스터의 첫 N번째 공격에서 피해를 입지 않음)
    /// </summary>
    public class EvadeFirstNEffect : RelicEffect
    {
        private readonly int evadeCount;

        public EvadeFirstNEffect(string effectType, int evadeCount) : base(effectType)
        {
            this.evadeCount = evadeCount;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            if (context.TurnCount < evadeCount)
            {
                context.Evaded = true;
            }
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { evadeCount });

        public int GetEvadeCount() => evadeCount;
    }
}
