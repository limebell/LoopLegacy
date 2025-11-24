namespace LoopLegacy.Battle.RelicEffects
{
    // 효과 없음 (기본값)
    public class NoEffect : RelicEffect
    {
        public NoEffect() : base("no_effect")
        {
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
        }

        public override string GetDescription()
        {
            return GetDescriptionWithArgs(new object[] { });
        }
    }
}
