using LoopLegacy.Manager;

namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 자이언트 킬러 효과 (자신보다 레벨이 더 높은 몬스터에게 받는 피해 감소)
    /// </summary>
    public class ReduceDamageHigherLevelEffect : RelicEffect
    {
        private readonly int damageIncrease;
        private readonly int damageReductionPercentage;

        public ReduceDamageHigherLevelEffect(string effectType, int damageIncrease, int damageReductionPercentage) : base(effectType)
        {
            this.damageIncrease = damageIncrease;
            this.damageReductionPercentage = damageReductionPercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            if (context.MonsterData == null)
            {
                return;
            }
            
            if (context.MonsterData.level > GameManager.Instance.GameState.PlayerStats.Level.Value)
            {
                context.EnemyAttackReductionRate += damageIncrease / 100f;
                context.EnemyAttackReductionRate *= 1.0f - damageReductionPercentage / 100f;
            }
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { damageIncrease, damageReductionPercentage });
            
        public int GetDamageIncrease() => damageIncrease;
        public int GetDamageReductionPercentage() => damageReductionPercentage;
    }
}
