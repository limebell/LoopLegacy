namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 회피 효과
    /// </summary>
    public class EvadeEffect : RelicEffect
    {
        private readonly int evasionPercentage;

        public EvadeEffect(string effectType, int evasionPercentage) : base(effectType)
        {
            this.evasionPercentage = evasionPercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            if (UnityEngine.Random.Range(0, 100) < evasionPercentage)
            {
                context.Evaded = true;
            }
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { evasionPercentage });

        public int GetEvasionPercentage() => evasionPercentage;
    }
}
