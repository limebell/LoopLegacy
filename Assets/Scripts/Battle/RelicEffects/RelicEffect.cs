using UnityEngine.Localization;

namespace LoopLegacy.Battle.RelicEffects
{
    // Relic 효과 인터페이스
    public abstract class RelicEffect
    {
        private readonly string effectType;
        
        public RelicEffect(string effectType)
        {
            this.effectType = effectType;
        }

        public abstract void ApplyEffect(RelicEffectContext context);

        public string GetName() => Utils.GetRelicName(effectType);

        public abstract string GetDescription();

        protected string GetDescriptionWithArgs(object[] args) =>
            Utils.GetRelicDescription(effectType, args);
    }
}
