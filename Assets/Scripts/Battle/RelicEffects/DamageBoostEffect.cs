namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 보스 대미지 증가 효과
    /// </summary>
    public class DamageBoostEffect : RelicEffect
    {
        private readonly MonsterType monsterType;
        private readonly int damageIncrease;

        public DamageBoostEffect(string effectType, MonsterType monsterType, int damageIncrease) : base(effectType)
        {
            this.monsterType = monsterType;
            this.damageIncrease = damageIncrease;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            if (context.MonsterData == null)
            {
                return;
            }
            
            if (context.MonsterData.type == monsterType)
            {
                context.DamageMultiplier += damageIncrease / 100f;
            }
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { damageIncrease });

        public MonsterType GetMonsterType() => monsterType;

        public int GetDamageIncrease() => damageIncrease;
    }
}
