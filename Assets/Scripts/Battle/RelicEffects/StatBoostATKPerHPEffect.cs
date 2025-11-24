using LoopLegacy.Manager;

namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 체력이 힘 효과 (직접 투자한 HP에 비례해 ATK가 증가)
    /// </summary>
    public class StatBoostATKPerHPEffect : RelicEffect
    {
        private readonly int attackIncreasePercentage;

        public StatBoostATKPerHPEffect(string effectType, int attackIncreasePercentage) : base(effectType)
        {
            this.attackIncreasePercentage = attackIncreasePercentage;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            var hp = GameManager.Instance.GameState.PlayerStats.Stats[(int)StatType.HP].Value;
            context.ATKBoost += (int)(hp * attackIncreasePercentage / 100f);
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { attackIncreasePercentage });

        public int GetAttackIncreasePercentage() => attackIncreasePercentage;
    }
}
