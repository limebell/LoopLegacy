using LoopLegacy.Manager;

namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 패배당 피해 증가 효과 (패배한 횟수마다 피해량 증가)
    /// </summary>
    public class DamageBoostPerDefeatEffect : RelicEffect
    {
        private readonly int damageIncreasePerDefeat;

        public DamageBoostPerDefeatEffect(string effectType, int damageIncreasePerDefeat) : base(effectType)
        {
            this.damageIncreasePerDefeat = damageIncreasePerDefeat;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            // 패배한 횟수만큼 피해량 증가
            if (!GameManager.Instance.TryGetRelic<DamageBoostPerDefeatEffect>(out Relic relic))
            {
                return;
            }

            context.DamageMultiplier += relic.Stack * damageIncreasePerDefeat / 100f;
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { damageIncreasePerDefeat });

        public int GetDamageIncreasePerDefeat() => damageIncreasePerDefeat;
    }
}

