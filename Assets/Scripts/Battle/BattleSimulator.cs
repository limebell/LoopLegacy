using LoopLegacy.Battle.RelicEffects;
using LoopLegacy.Loader;
using LoopLegacy.Manager;
using LoopLegacy.State;
using LoopLegacy.UI.Controller;
using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

namespace LoopLegacy.Battle
{
    public class BattleSimulator
    {
        private readonly Weapon weapon;
        private readonly Armor armor;
        private readonly string monsterName;
        private readonly MonsterData monster;
        private readonly BattleController battleController;
        public readonly float hitDelay;

        public ReactiveProperty<int> CurrentPlayerHP { get; private set; }
        public ReactiveProperty<int> CurrentMonsterHP { get; private set; }
        public int MaxPlayerHP { get; private set; }
        public int MaxMonsterHP { get; private set; }
        public int MonsterATK { get; private set; }

        public bool IsMonsterDead => CurrentMonsterHP.Value <= 0;
        public bool IsPlayerDead => CurrentPlayerHP.Value <= 0;
        public bool IsBattleEnded => IsMonsterDead || IsPlayerDead;

        public BattleSimulator(
            Weapon weapon,
            Armor armor,
            MonsterData monster,
            BattleController battleController,
            float hitDelay)
        {
            this.weapon = weapon;
            this.armor = armor;
            this.monster = monster;
            this.monsterName = Utils.GetMonsterName(monster);
            this.battleController = battleController;
            this.hitDelay = hitDelay;
            this.MaxPlayerHP = GameManager.GetStat(StatType.HP);
            if (monster.code == "boss_11")
            {
                // 처치한 수호자의 수를 세기 (ID: 7, 8, 9, 10)
                int defeatedGuardiansCount = GameManager.Instance.GameState.DefeatedBosses.Count(code => code == "boss_7" || code == "boss_8" || code == "boss_9" || code == "boss_10");
                // 남은 수호자 = 전체 4명 - 처치한 수
                int leftGuardians = 4 - defeatedGuardiansCount;
                this.MaxMonsterHP = monster.hp * (1 + leftGuardians);
                this.MonsterATK = (int)(monster.atk * (1 + leftGuardians * 0.25f));
            }
            else
            {
                this.MaxMonsterHP = monster.hp;
                this.MonsterATK = monster.atk;
            }

            // 체력 ReactiveProperty 초기화
            CurrentPlayerHP = new ReactiveProperty<int>(MaxPlayerHP);
            CurrentMonsterHP = new ReactiveProperty<int>(MaxMonsterHP);
        }

        private void ProcessHit(float baseDamage, BattleContext battleContext, int multiHitCount)
        {
            // Relic 효과 컨텍스트 생성
            var relicContext = new RelicEffectContext
            {
                MonsterData = monster,
                MaxPlayerHP = MaxPlayerHP,
                CurrentPlayerHP = CurrentPlayerHP.Value,
                CurrentMonsterHP = CurrentMonsterHP.Value,
                TurnCount = battleContext.TurnCount,
                AttackCount = battleContext.AttackCount,
                BaseDamage = baseDamage,
                CriticalDamageMultiplier = PlayerStats.CRIT_MULTIPLIER,
            };

            // Relic 효과 적용
            GameManager.Instance.RelicManager.ApplyEffects(relicContext);
            
            // 대미지 계산
            // 변동 대미지 폭 80%~120%
            float damageMultiplier = 0.8f + (float)(UnityEngine.Random.Range(0, 1f) * 0.4f);
            int damage = (int)Math.Ceiling(baseDamage * relicContext.DamageMultiplier * damageMultiplier);
            bool isCritical = UnityEngine.Random.value <= relicContext.CriticalRate;
            if (isCritical)
            {
                damage = (int)(damage * relicContext.CriticalDamageMultiplier);
                battleContext.CritHits++;
            }

            // 몬스터 ATK 기반 대미지 감소 (실험적 기능)
            if (damage > 0 && MonsterATK > 0)
            {
                float reductionRate = MonsterATK / (damage + MonsterATK);
                damage = (int)(damage * (1 - reductionRate));
            }

            // 고정 피해 추가 (대미지 감소 영향 받지 않음)
            damage += relicContext.FixedDamage;

            // 대미지 적용
            CurrentMonsterHP.Value = Math.Max(0, CurrentMonsterHP.Value - damage);
            battleContext.TotalDamageDealt += damage;
            battleContext.AttackCount++;
            if (GameManager.Instance.TryGetRelic<FixedDamagePerHitEffect>(out Relic hfdRelic))
            {
                hfdRelic.Stack++;
            }

            // 처형 확인
            if (relicContext.ShouldExecute)
            {
                CurrentMonsterHP.Value = 0;
                battleController?.RenderExecution(monsterName);
                return;
            }

            battleController?.RenderPlayerAttack(damage, isCritical, multiHitCount, monsterName);

            // 흡혈 적용
            if (relicContext.DrainPercentage > 0)
            {
                CurrentPlayerHP.Value = Math.Min(MaxPlayerHP, CurrentPlayerHP.Value + damage * relicContext.DrainPercentage / 100);
                battleController?.RenderHeal(damage * relicContext.DrainPercentage / 100);
            }
        }

        public IEnumerator SimulatePlayerAttackCoroutine(BattleContext battleContext)
        {
            float baseDamageSpecMultiplier = 1.0f;
            if (GameManager.Instance.TryGetRelic<EquipmentBaseSpecBoostEffect>(out Relic ebsbRelic))
            {
                baseDamageSpecMultiplier = (ebsbRelic.Effect as EquipmentBaseSpecBoostEffect).GetEquipmentBaseSpecBoostPercentage() / 100f;
            }

            // 기본 대미지 계산
            float baseDamage = weapon.GetCalculatedBaseDamage() * baseDamageSpecMultiplier +
                (GameManager.GetStat(StatType.ATK) * (weapon.GetCalculatedDamageMultiplier() * 0.01f));

            // 멀티 히트 계산
            const int MAX_HITS = 10;
            int multiHits = 1, maxMultiHits = 1;
            if (battleContext.TurnCount == 0 && GameManager.Instance.TryGetRelic<ChainFirstEffect>(out Relic facRelic))
            {
                maxMultiHits = (facRelic.Effect as ChainFirstEffect).GetComboCount();
            }

            if (GameManager.Instance.TryGetRelic<ChainRateEffect>(out Relic maRelic))
            {
                float probability = (maRelic.Effect as ChainRateEffect).GetChainProbability() / 100f;
                while (maxMultiHits < MAX_HITS)
                {
                    if (UnityEngine.Random.value <= Math.Pow(probability, maxMultiHits))
                    {
                        maxMultiHits++;
                    }
                    else break;
                }
            }

            maxMultiHits = Math.Min(maxMultiHits, MAX_HITS);

            // 첫 번째 타격 처리
            ProcessHit(baseDamage, battleContext, multiHits);

            // 추가 타격 처리
            while (multiHits < maxMultiHits)
            {
                if (IsMonsterDead) break;
                yield return new WaitForSeconds(hitDelay);
                multiHits++;
                ProcessHit(baseDamage, battleContext, multiHits);
            }
            
            if (multiHits > 1)
            {
                battleContext.MultiHits++;
            }
        }

        public void SimulateMonsterAttack(BattleContext battleContext)
        {
            float totalDefense = armor.GetFinalDefense(GameManager.GetStat(StatType.DEF));
            
            // Relic 효과 컨텍스트 생성
            var context = new RelicEffectContext
            {
                MonsterData = monster,
                BaseDefense = totalDefense,
                TurnCount = battleContext.TurnCount,
                AttackCount = battleContext.AttackCount,
                DamageMultiplier = 1.0f,
            };

            // Relic 효과 적용
            GameManager.Instance.RelicManager.ApplyEffects(context);
            
            float baseDamage = 0;
            // 받는 피해 계산
            float damageReduction = totalDefense / (1 + totalDefense + MonsterATK);
            baseDamage = MonsterATK * (1 - damageReduction) * context.EnemyAttackReductionRate;
            
            // 랜덤 대미지 변동 (80%~120%)
            float damageMultiplier = 0.8f + (float)(UnityEngine.Random.Range(0, 1f) * 0.4f);
            int damage = (int)Math.Ceiling(baseDamage * damageMultiplier);
            
            if (context.Evaded)
            {
                damage = 0;
            }

            CurrentPlayerHP.Value = Math.Max(0, CurrentPlayerHP.Value - damage);
            battleContext.TotalDamageTaken += damage;

            // 반사 피해 적용
            if (context.ReflectDamage > 0)
            {
                CurrentMonsterHP.Value = Math.Max(0, CurrentMonsterHP.Value - context.ReflectDamage);
                // 각종 추가 피해가 반사 피해에도 적용
                battleContext.TotalDamageDealt += (int)Math.Ceiling(context.ReflectDamage * context.DamageMultiplier);
                battleController?.RenderPlayerAttack(context.ReflectDamage, false, 1, monsterName);
            }

            battleController?.RenderEnemyAttack(damage, context.Evaded, monsterName);
        }
    }

    public class BattleContext
    {
        public int TurnCount { get; set; }
        public int AttackCount { get; set; }
        public bool IsVictory { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalDamageTaken { get; set; }
        public int CritHits { get; set; }
        public int MultiHits { get; set; }
        public int EarnedBattlePoint { get; set; }
        public BigInteger EarnedEXP { get; set; }
        public int EarnedGold { get; set; }
        public List<DropEntry> DroppedItems { get; set; } = new List<DropEntry>();
    }
}
