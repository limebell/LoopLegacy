using System.Numerics;
using LoopLegacy.Loader;
using LoopLegacy.State;

namespace LoopLegacy.Battle.RelicEffects
{
    // Relic 효과 컨텍스트 (전투 중 필요한 정보들)
    public class RelicEffectContext
    {
        // 수정되지 않는 고정 컨텍스트
        public MonsterData MonsterData { get; set; }
        public int MaxPlayerHP { get; set; }
        public int CurrentPlayerHP { get; set; }
        public int CurrentMonsterHP { get; set; }

        // 공격 관련 컨텍스트
        public float BaseDamage { get; set; }
        public bool ShouldExecute { get; set; } = false;
        public int AttackCount { get; set; } = 0;
        public int TurnCount { get; set; } = 0;
        public float DamageMultiplier { get; set; } = 1.0f;
        public float CriticalRate { get; set; } = 0f;
        public float CriticalDamageMultiplier { get; set; } = PlayerStats.CRIT_MULTIPLIER;
        public int FixedDamage { get; set; } = 0;
        public int DrainPercentage { get; set; } = 0;

        // 방어 관련 컨텍스트
        public float BaseDefense { get; set; }
        public bool Evaded { get; set; } = false;
        public int ReflectDamage { get; set; } = 0;
        public float EnemyAttackReductionRate { get; set; } = 1.0f;

        // 보상 관련
        public float ExpMultiplier { get; set; } = 1.0f;
        public float GoldMultiplier { get; set; } = 1.0f;
        public float DropRateMultiplier { get; set; } = 1.0f;

        // 스탯 관련
        public int HPBoost { get; set; } = 0;
        public int DEFBoost { get; set; } = 0;
        public int ATKBoost { get; set; } = 0;
        public int LUCBoost { get; set; } = 0;
    }
}
