using LoopLegacy.Battle;
using LoopLegacy.Battle.RelicEffects;
using LoopLegacy.Loader;
using LoopLegacy.State;
using LoopLegacy.UI.Controller;
using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace LoopLegacy.Manager
{
    public class BattleManager : MonoBehaviour
    {
        private static BattleManager _instance;
        public static BattleManager Instance => _instance;

        [SerializeField]
        private BattleController _battleController;
        [SerializeField]
        private BattleResultController _battleResultController;
        private const float BATTLE_STEP_DELAY = 1.0f; // 각 전투 단계 사이의 딜레이 (초)
        private const float BATTLE_STEP_DELAY_INTERVAL = 0.3f; // 옵션에 따른 딜레이 증가 (초)
        private const float HIT_DELAY = 0.6f;
        private const float HIT_DELAY_INTERVAL = 0.1f;
        private bool _giveupBattle = false;
        private MonsterData _monsterData;
        public ReactiveProperty<BattleStep> CurrentBattleStep { get; private set; }

        private ReactiveProperty<BattleContext> _currentBattleResult;
        private Action<BattleContext> _onBattleEnd;

        private BattleSimulator _simulator;

        private void Awake()
        {
            _instance = this;
            CurrentBattleStep = new ReactiveProperty<BattleStep>(BattleStep.None);
        }

        public void OnClickBattleUI()
        {
            switch (CurrentBattleStep.Value)
            {
                case BattleStep.Prepare:
                    ExecuteBattle(_monsterData);
                    break;
                case BattleStep.Simulating:
                    break;
                case BattleStep.End:
                    _battleController.HideBattleUI();
                    _battleResultController.ShowBattleResult(_currentBattleResult.Value);
                    CurrentBattleStep.Value = BattleStep.Result;
                    break;
                case BattleStep.Result:
                    _battleResultController.HideBattleResult();
                    ApplyBattleResult(_currentBattleResult.Value);
                    CurrentBattleStep.Value = BattleStep.None;
                    break;
                case BattleStep.None:
                    break;
            }
        }

        public ReactiveProperty<BattleContext> StartBattle(MonsterData monsterData, Action<BattleContext> onBattleEnd)
        {
            // 전투가 이미 진행 중이면 무시
            if (CurrentBattleStep.Value != BattleStep.None)
            {
                Debug.Log("[Battle] 이미 전투가 진행 중입니다.");
                return null;
            }
            
            if (_battleController == null)
            {
                Debug.LogError("[Battle] HUD Controller가 초기화되지 않았습니다!");
                return null;
            }
            
            _giveupBattle = false;
            GameManager.Instance.GameState.CombatInfo.IncrementCombatCount(monsterData);

            CurrentBattleStep.Value = BattleStep.Prepare;
            _monsterData = monsterData;
            _onBattleEnd = onBattleEnd;

            var weapon = PersistentGameState.Instance.GetCurrentWeapon();
            var armor = PersistentGameState.Instance.GetCurrentArmor();

            if (weapon == null || armor == null)
            {
                Debug.LogError("[Battle] 무기 혹은 방어구가 없습니다.");
                return null;
            }

            // 전투 시뮬레이터 생성
            _simulator = new BattleSimulator(
                weapon,
                armor,
                monsterData,
                _battleController,
                HIT_DELAY - HIT_DELAY_INTERVAL * OptionState.Instance.CombatSpeed.Value
            );

            // HUD 컨트롤러에 시뮬레이터 설정
            _battleController.SetBattleSimulator(_simulator);

            // 전투 UI 표시 및 시작 버튼 활성화
            _battleController.PrepareBattle(monsterData);

            // 이 전투의 결과를 위한 ReactiveProperty 생성
            _currentBattleResult = new ReactiveProperty<BattleContext>(null);
            return _currentBattleResult;
        }

        public void GiveupBattle()
        {
            if (_giveupBattle == true) return;
            _giveupBattle = true;
            var looseContext = new BattleContext
            {
                IsVictory = false,
                EarnedBattlePoint = -3,
                EarnedEXP = 0,
                EarnedGold = 0,
                DroppedItems = new List<DropEntry>(),
            };
            BattleEnd(looseContext, null, _monsterData);
        }

        private void ExecuteBattle(MonsterData monsterData)
        {
            // 전투 상태 시작
            CurrentBattleStep.Value = BattleStep.Simulating;

            // 전투 시뮬레이션 실행
            StartCoroutine(SimulateBattleCoroutine(_simulator, monsterData));
        }

        private IEnumerator SimulateBattleCoroutine(BattleSimulator simulator, MonsterData monsterData)
        {
            var result = new BattleContext();

            // 전투 시작
            while (!simulator.IsBattleEnded && !_giveupBattle)
            {
                // 플레이어의 공격
                yield return simulator.SimulatePlayerAttackCoroutine(result);
                if (simulator.IsMonsterDead) break;

                // 몬스터의 공격
                yield return new WaitForSeconds(BATTLE_STEP_DELAY - BATTLE_STEP_DELAY_INTERVAL * OptionState.Instance.CombatSpeed.Value);
                simulator.SimulateMonsterAttack(result);
                if (simulator.IsPlayerDead) break;
                yield return new WaitForSeconds(BATTLE_STEP_DELAY - BATTLE_STEP_DELAY_INTERVAL * OptionState.Instance.CombatSpeed.Value);

                // 매 턴 체력 회복
                if (GameManager.Instance.TryGetRelic<HealEveryTurnEffect>(out Relic relic))
                {
                    int healAmount = (relic.Effect as HealEveryTurnEffect).GetHealAmount();
                    simulator.CurrentPlayerHP.Value =
                        Math.Min(simulator.CurrentPlayerHP.Value + healAmount, simulator.MaxPlayerHP);
                    _battleController.RenderHeal(healAmount);
                    yield return new WaitForSeconds(BATTLE_STEP_DELAY - BATTLE_STEP_DELAY_INTERVAL * OptionState.Instance.CombatSpeed.Value);
                }
                result.TurnCount++;
            }

            BattleEnd(result, simulator, monsterData);
        }

        private void BattleEnd(BattleContext result, BattleSimulator simulator, MonsterData monsterData)
        {
            // 전투 결과 메시지
            if (simulator != null && simulator.IsMonsterDead)
            {
                if (GameManager.Instance.TryGetRelic<DamageBoostPerKillEffect>(out Relic killDamageRelic))
                {
                    killDamageRelic.Stack++;
                }

                result.IsVictory = true;
                
                // 기본 보상 계산
                BigInteger baseExp = monsterData.exp;
                int baseGold = monsterData.gold;
                
                // LUC 스탯에 따른 골드 보정
                int luc = GameManager.GetStat(StatType.LUC);
                float lucMultipllier = 1.0f + Mathf.Min(1.0f, (float)luc / (300 + luc + monsterData.level));
                Debug.Log($"Luc multiplier: {lucMultipllier}");
                

                // Relic 효과를 보상에 적용
                var context = new RelicEffectContext
                {
                    ExpMultiplier = 1.0f + PersistentGameState.Instance.GetBoostExp() / 100f,
                    GoldMultiplier = (1.0f + PersistentGameState.Instance.GetBoostGold() / 100f) * lucMultipllier,
                };
                GameManager.Instance.RelicManager.ApplyEffects(context);
                Debug.Log($"Exp multiplier: {context.ExpMultiplier}, Gold multiplier: {context.GoldMultiplier}");
                
                // 최종 보상 저장
                result.EarnedEXP = PlayerStats.CalculateAdjustedEXP(
                        baseExp,
                        GameManager.Instance.GameState.PlayerStats.Level.Value,
                        monsterData.level) * Mathf.RoundToInt(context.ExpMultiplier * 100f) / 100;
                result.EarnedGold = Mathf.RoundToInt(baseGold * context.GoldMultiplier);
                
                // 드롭 아이템 계산
                foreach (var drop in monsterData.drops)
                {
                    var randomValue = UnityEngine.Random.value;
                    var dropRate = drop.dropRate * lucMultipllier * context.DropRateMultiplier * LibraryManager.GetMonsterDropMultiplier(PersistentGameState.Instance.CodexState.GetMobKillCount(monsterData.code));
                    Debug.Log($"Drop Rate for {drop.itemType} {drop.itemId}: {dropRate}, Random Value: {randomValue}");
                    Debug.Log($"randomValue <= dropRate: {randomValue <= dropRate}");
                    if (randomValue <= dropRate)
                    {
                        // 이미 최대로 보유 갯수를 초과한 드롭에 대해서는 무시
                        switch (drop.itemType)
                        {
                            case DropType.Weapon:
                                if (PersistentGameState.Instance.InventoryState.GetOwnedEquipments(EquipmentType.Weapon)[drop.itemId] >=
                                        InventoryState.MAX_EQUIPMENT_DUPLICATE_COUNT)
                                {
                                    Debug.Log($"Max weapon duplicate count reached for {drop.itemType} {drop.itemId}");
                                    continue;
                                }
                                break;
                            case DropType.Armor:
                                if (PersistentGameState.Instance.InventoryState.GetOwnedEquipments(EquipmentType.Armor)[drop.itemId] >=
                                        InventoryState.MAX_EQUIPMENT_DUPLICATE_COUNT)
                                {
                                    Debug.Log($"Max armor duplicate count reached for {drop.itemType} {drop.itemId}");
                                    continue;
                                }
                                break;
                            case DropType.Relic:
                                if (!PersistentGameState.Instance.IsRelicFeatureUnlocked())
                                {
                                    Debug.Log($"Relic feature not unlocked, skipping drop {drop.itemType} {drop.itemId}");
                                    continue;
                                }
                                Debug.Log($"Relic feature unlocked, checking drop {drop.itemType} {drop.relicEffectName}");
                                var relicId = TableManager.GetRelicId(drop.relicEffectName);
                                var myRelic = PersistentGameState.Instance.CodexState.GetRelic(relicId);
                                if (myRelic != null && myRelic.Level >= drop.relicLevel)
                                {
                                    Debug.Log($"Relic {relicId} level {drop.relicLevel} already unlocked");
                                    continue;
                                }
                                break;
                        }

                        Debug.Log($"Dropped {drop.itemType} {drop.itemId}");
                        result.DroppedItems.Add(drop);
                    }
                }

                result.EarnedBattlePoint = monsterData.bp;
                _battleController.RenderVictory();
            }
            else
            {
                if (GameManager.Instance.TryGetRelic<DamageBoostPerDefeatEffect>(out Relic defeatDamageRelic))
                {
                    defeatDamageRelic.Stack++;
                }

                result.IsVictory = false;

                // 패배 시 배틀포인트 처리
                if (GameManager.Instance.TryGetRelic<BattlePointLossPreventDefeatEffect>(out Relic battleDefeatProtectionRelic))
                {
                    float protectionPercentage = (battleDefeatProtectionRelic.Effect as BattlePointLossPreventDefeatEffect).GetProtectionPercentage();
                    result.EarnedBattlePoint = UnityEngine.Random.Range(0, 100) < protectionPercentage ? 0 : -3;
                }
                else
                {
                    result.EarnedBattlePoint = -3;
                }
                
                _battleController.RenderDefeat();
            }

            // 전투 결과 저장
            _currentBattleResult.Value = result;
            CurrentBattleStep.Value = BattleStep.End;
            _simulator = null;
        }

        private void ApplyBattleResult(BattleContext result)
        {
            if (result.IsVictory)
            {
                // 이미 계산된 보상 적용
                Debug.Log($"Adding EXP: {result.EarnedEXP}");
                GameManager.Instance.GameState.PlayerStats.AddEXP(result.EarnedEXP);
                Debug.Log($"Adding Gold: {result.EarnedGold}");
                PersistentGameState.Instance.AddGold(result.EarnedGold);
                PersistentGameState.Instance.CodexState.AddMobKillCount(_monsterData.code);

                // 드롭 아이템 적용
                foreach (var entry in result.DroppedItems)
                {
                    Debug.Log($"Applying drop {entry.itemType} {entry.itemId}");
                    if (entry.itemType == DropType.Relic)
                    {
                        Debug.Log($"Unlocking relic {entry.relicEffectName} {entry.relicLevel}");
                        var relicId = TableManager.GetRelicId(entry.relicEffectName);
                        var relicLevel = entry.relicLevel;
                        PersistentGameState.Instance.CodexState.UnlockRelicWithLevel(relicId, relicLevel);
                        GameManager.Instance.GameState.AddDroppedItem(new DropEntryData
                        {
                            itemType = DropType.Relic,
                            itemId = relicId,
                            relicLevel = relicLevel,
                            count = 1,
                        });
                    }
                    else
                    {
                        Debug.Log($"Adding equipment {entry.itemType} {entry.itemId}");
                        PersistentGameState.Instance.InventoryState.AddEquipment(entry.itemType switch
                        {
                            DropType.Weapon => EquipmentType.Weapon,
                            DropType.Armor => EquipmentType.Armor,
                            _ => throw new NotImplementedException()
                        }, entry.itemId);
                        GameManager.Instance.GameState.AddDroppedItem(new DropEntryData
                        {
                            itemType = entry.itemType,
                            itemId = entry.itemId,
                            relicLevel = 0,
                            count = 1,
                        });
                    }
                }

                GameManager.Instance.GameState.CombatInfo.IncrementWins(_monsterData.type);

                // 몬스터 액션 실행
                foreach (var action in _monsterData.actions)
                {
                    switch (action.actionType)
                    {
                        case MonsterActionType.Teleport:
                            GameManager.Instance.MoveToMap(
                                action.values[0],
                                new UnityEngine.Vector2(float.Parse(action.values[1]), float.Parse(action.values[2])));
                            break;
                        case MonsterActionType.RewardRelic:
                            GameManager.Instance.RelicReward(int.Parse(action.values[0]), new Relic[] { });
                            break;
                        case MonsterActionType.None:
                            break;
                    }
                }
            }
            else
            {
                GameManager.Instance.GameState.CombatInfo.IncrementLosses();
            }

            GameManager.Instance.GameState.PlayerStats.AddBattlePoint(result.EarnedBattlePoint);
            GameManager.Instance.Save();
            _onBattleEnd?.Invoke(result);
        }
    }
} 