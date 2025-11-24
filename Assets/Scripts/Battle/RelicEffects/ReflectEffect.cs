using LoopLegacy.Manager;

namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 가시 갑옷 효과 (공격 받을 때 방어력의 일정 비율만큼 반사 피해)
    /// </summary>
    public class ReflectEffect : RelicEffect
    {
        private readonly int reflectPercentage;

        public ReflectEffect(string effectType, int reflectPercentage) : base(effectType)
        {
            this.reflectPercentage = reflectPercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            context.ReflectDamage = (int)(context.BaseDefense * reflectPercentage / 100f);
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { reflectPercentage });

        public int GetReflectPercentage() => reflectPercentage;
    }
}
