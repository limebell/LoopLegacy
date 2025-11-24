using LoopLegacy.State;

namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 아는 것이 힘이다 효과 (100번 이상 잡은 몬스터에게 피해량과 드롭률 증가)
    /// </summary>
    public class DamageDropBoostMonster100KillsEffect : RelicEffect
    {
        private readonly int damageIncreasePercentage;
        private readonly int dropRateIncreasePercentage;

        public DamageDropBoostMonster100KillsEffect(
            string effectType,
            int damageIncreasePercentage,
            int dropRateIncreasePercentage) : base(effectType)
        {
            this.damageIncreasePercentage = damageIncreasePercentage;
            this.dropRateIncreasePercentage = dropRateIncreasePercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            if (context.MonsterData == null)
            {
                return;
            }
            
            var killCount = PersistentGameState.Instance.CodexState.GetMobKillCount(context.MonsterData.code);
            if (killCount >= 100)
            {
                context.DamageMultiplier += damageIncreasePercentage / 100f;
                context.DropRateMultiplier += dropRateIncreasePercentage / 100f;
            }
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { damageIncreasePercentage, dropRateIncreasePercentage });

        public int GetDamageIncreasePercentage() => damageIncreasePercentage;
        public int GetDropRateIncreasePercentage() => dropRateIncreasePercentage;
    }
}
