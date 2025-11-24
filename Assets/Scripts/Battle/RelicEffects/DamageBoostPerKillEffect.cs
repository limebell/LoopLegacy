using System.Linq;
using LoopLegacy.Manager;

namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 처치당 피해 증가 효과 (처치한 적마다 피해량 증가)
    /// </summary>
    public class DamageBoostPerKillEffect : RelicEffect
    {
        private readonly float damageIncreasePerKill;

        public DamageBoostPerKillEffect(string effectType, float damageIncreasePerKill) : base(effectType)
        {
            this.damageIncreasePerKill = damageIncreasePerKill;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            // 처치한 적 수만큼 피해량 증가
            var ownedRelics = GameManager.Instance.GameState.OwnedRelics.Value;
            if (!ownedRelics.Any(relic => relic.Effect is DamageBoostPerKillEffect))
            {
                return;
            }
            
            int killCount = ownedRelics.First(relic => relic.Effect is DamageBoostPerKillEffect).Stack;
            context.DamageMultiplier += killCount * damageIncreasePerKill / 100f;
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { damageIncreasePerKill });

        public float GetDamageIncreasePerKill() => damageIncreasePerKill;
    }
}

