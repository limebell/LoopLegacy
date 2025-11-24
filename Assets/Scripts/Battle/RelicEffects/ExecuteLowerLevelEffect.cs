using LoopLegacy.Manager;

namespace LoopLegacy.Battle.RelicEffects
{
    /// <summary>
    /// 약자 멸시 효과 (자신보다 레벨이 더 낮은 몬스터 확률로 처형)
    /// </summary>
    public class ExecuteLowerLevelEffect : RelicEffect
    {
        private readonly int executeProbability;

        public ExecuteLowerLevelEffect(string effectType, int executeProbability) : base(effectType)
        {
            this.executeProbability = executeProbability;
        }

        public override void ApplyEffect(RelicEffectContext context)
        {
            if (context.MonsterData == null)
            {
                return;
            }
            
            if (context.MonsterData.level < GameManager.Instance.GameState.PlayerStats.Level.Value)
            {
                if (UnityEngine.Random.Range(0, 100) < executeProbability)
                {
                    context.ShouldExecute = true;
                }
            }
        }

        public override string GetDescription() =>
            GetDescriptionWithArgs(new object[] { executeProbability });

        public int GetExecuteProbability() => executeProbability;
    }
}
