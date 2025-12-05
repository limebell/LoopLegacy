using LoopLegacy.Battle.RelicEffects;
using UnityEngine;

namespace LoopLegacy.Battle
{
    public static class RelicEffectFactory
    {
        public static RelicEffect CreateEffect(string effectType, string value)
        {
            switch (effectType)
            {
                case "stat_boost_hp":
                    return new StatBoostEffect(effectType, StatType.HP, int.Parse(value));
                case "stat_boost_atk":
                    return new StatBoostEffect(effectType, StatType.ATK, int.Parse(value));
                case "stat_boost_def":
                    return new StatBoostEffect(effectType, StatType.DEF, int.Parse(value));
                case "stat_boost_luc":
                    return new StatBoostEffect(effectType, StatType.LUC, int.Parse(value));
                
                case "exp_boost":
                    return new ExpBoostEffect(effectType, int.Parse(value));
                case "gold_boost":
                    return new GoldBoostEffect(effectType, int.Parse(value));
                case "drop_boost":
                    return new DropBoostEffect(effectType, float.Parse(value));
                case "evade_rate":
                    return new EvadeEffect(effectType, int.Parse(value));
                case "crit_rate":
                    return new CriticalRateEffect(effectType, int.Parse(value));
                case "crit_dmg":
                    return new CriticalDamageEffect(effectType, int.Parse(value));
                case "crit_every_n":
                    return new CriticalEveryNthEffect(effectType, int.Parse(value));
                case "bp_boost":
                    return new BattlePointBoostEffect(effectType, int.Parse(value));
                case "speed_boost":
                    return new SpeedBoostEffect(effectType, int.Parse(value));

                case "dmg_boost_boss":
                    return new DamageBoostEffect(effectType, MonsterType.Boss, int.Parse(value));
                case "dmg_boost_normal":
                    return new DamageBoostEffect(effectType, MonsterType.Normal, int.Parse(value));

                case "bp_loss_prevent_defeat":
                {
                    var values = value.Split(':');
                    return new BattlePointLossPreventDefeatEffect(effectType, float.Parse(values[0]), int.Parse(values[1]));
                }
                case "bp_loss_prevent_victory":
                {
                    var values = value.Split(':');
                    return new BattlePointLossPreventVictoryEffect(effectType, float.Parse(values[0]), int.Parse(values[1]));
                }
                case "decrease_encounter_increment":
                    return new DecreaseEncounterIncrementEffect(effectType, int.Parse(value));
                case "stat_boost_per_relic_count":
                    return new StatBoostPerRelicEffect(effectType, int.Parse(value));
                case "fixed_dmg_per_gold":
                    return new FixedDamagePerGoldEffect(effectType, float.Parse(value));
                case "equipment_base_spec_boost":
                    return new EquipmentBaseSpecBoostEffect(effectType, float.Parse(value));
                case "fixed_dmg":
                    return new FixedDamageEffect(effectType, int.Parse(value));

                case "dmg_boost_any":
                    return new DamageBoostEffect(effectType, MonsterType.Any, int.Parse(value));
                case "chain_rate":
                    return new ChainRateEffect(effectType, int.Parse(value));
                case "execute_low_health":
                    return new ExecuteLowHealthEffect(effectType, int.Parse(value));
                case "dmg_boost_per_defeat":
                    return new DamageBoostPerDefeatEffect(effectType, int.Parse(value));
                case "evade_first_n":
                    return new EvadeFirstNEffect(effectType, int.Parse(value));
                case "dmg_gradual_increase":
                {
                    var values = value.Split(':');
                    return new DamageGradualIncreaseEffect(effectType, int.Parse(values[0]), int.Parse(values[1]));
                }
                case "dmg_gradual_decrease":
                {
                    var values = value.Split(':');
                    return new DamageGradualDecreaseEffect(effectType, int.Parse(values[0]), int.Parse(values[1]));
                }
                case "dmg_drop_boost_monster_codex":
                {
                    var values = value.Split(':');
                    return new DamageDropBoostMonster100KillsEffect(effectType, int.Parse(values[0]), int.Parse(values[1]));
                }
                case "reduce_dmg_higher_level":
                {
                    var values = value.Split(':');
                    return new ReduceDamageHigherLevelEffect(effectType, int.Parse(values[0]), int.Parse(values[1]));
                }
                case "execute_lower_level":
                {
                    var values = value.Split(':');
                    return new ExecuteLowerLevelEffect(effectType, int.Parse(values[0]), int.Parse(values[1]));
                }
                case "stat_boost_atk_per_hp":
                    return new StatBoostATKPerHPEffect(effectType, int.Parse(value));
                case "heal_every_turn":
                    return new HealEveryTurnEffect(effectType, int.Parse(value));
                case "weak_every_hit":
                    return new WeakEveryHitEffect(effectType, int.Parse(value));
                case "dmg_boost_when_low_hp":
                {
                    var values = value.Split(':');
                    return new DamageBoostWhenLowHealthEffect(effectType, int.Parse(values[0]), int.Parse(values[1]));
                }
                case "crit_first_n":
                    return new CriticalFirstNEffect(effectType, int.Parse(value));
                case "chain_first":
                    return new ChainFirstEffect(effectType, int.Parse(value));
                case "reflect":
                    return new ReflectEffect(effectType, int.Parse(value));
                case "execute":
                    return new ExecuteEffect(effectType, float.Parse(value));
                case "drain":
                    return new DrainEffect(effectType, int.Parse(value));
                case "dmg_boost_when_full_hp":
                    return new DamageBoostWhenFullHealthEffect(effectType, int.Parse(value));
                case "dmg_boost_weak":
                    return new DamageBoostWeakEffect(effectType, int.Parse(value));
                case "fixed_dmg_per_hit":
                    return new FixedDamagePerHitEffect(effectType, int.Parse(value));

                case "stat_boost_collect_weapon":
                    return new StatBoostCollectEffect(effectType, EquipmentType.Weapon, int.Parse(value));
                case "stat_boost_collect_armor":
                    return new StatBoostCollectEffect(effectType, EquipmentType.Armor, int.Parse(value));
                    
                case "stat_point_boost":
                    return new StatPointIncreaseEffect(effectType, int.Parse(value));
                case "fixed_dmg_enemy_hp":
                    return new FixedDamageEnemyHealthEffect(effectType, int.Parse(value));
                case "dmg_boost_per_kill":
                    return new DamageBoostPerKillEffect(effectType, float.Parse(value));
                case "restart_with_gold":
                    return new RestartWithGoldEffect(effectType);
                default:
                    Debug.LogWarning($"Unknown effect type: {effectType}");
                    return new NoEffect();
            }
        }
    }
}
