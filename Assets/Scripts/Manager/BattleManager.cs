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
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LoopLegacy.Manager
{
    public class BattleManager : MonoBehaviour
    {
        private static BattleManager _instance;
        public static BattleManager Instance => _instance;

        public event Action OnBattleEnd;

        [SerializeField]
        private BattleController _battleController;
        [SerializeField]
        private BattleResultController _battleResultController;
        private const float BATTLE_STEP_DELAY = 1.0f; // 각 전투 단계 사이의 딜레이 (초)
        private const float BATTLE_STEP_DELAY_INTERVAL = 0.3f; // 옵션에 따른 딜레이 증가 (초)
        private const float HIT_DELAY = 0.6f;
        private const float HIT_DELAY_INTERVAL = 0.1f;
        private MonsterData _monsterData;
        private RegionEffectType _regionEffect;
        public ReactiveProperty<BattleStep> CurrentBattleStep { get; private set; }

        private ReactiveProperty<BattleContext> _currentBattleResult;
        private Action<BattleContext> _onBattleEnd;

        private BattleSimulator _simulator;
        private Task<LevelUpResult> _levelUpTask; // 비동기 레벨업 계산 Task
        private CancellationTokenSource _levelUpCts; // 레벨업 계산 취소용 CTS
        private BigInteger _expectedEXP; // 미리 계산된 예상 경험치
        private int _expectedGold; // 미리 계산된 예상 골드
        private LevelUpResult? _completedLevelUpResult; // ShowBattleResult에서 완료된 레벨업 결과 (ApplyBattleResult에서 재사용)
        private Coroutine _simulateBattleCoroutine;

        private void Awake()
        {
            _instance = this;
            CurrentBattleStep = new ReactiveProperty<BattleStep>(BattleStep.None);
        }
        
        private void OnDestroy()
        {
            // BattleManager가 파괴될 때 진행 중인 레벨업 계산 취소
            CancelLevelUpCalculation();
        }

        public void OnClickBattleUI()
        {
            switch (CurrentBattleStep.Value)
            {
                case BattleStep.PrepareComplete:
                    ExecuteBattle(_monsterData);
                    break;
                case BattleStep.Simulating:
                    break;
                case BattleStep.PreEnd:
                    break;
                case BattleStep.End:
                    _battleController.HideBattleUI();
                    // Task 완료를 기다린 후 BattleStep.Result로 전환
                    StartCoroutine(ShowBattleResultWithTaskCoroutine());
                    break;
                case BattleStep.Result:
                    _battleResultController.HideBattleResult();
                    ApplyBattleResult(_currentBattleResult.Value);
                    GameManager.Instance.EncounterManager.ResetGauge();
                    break;
                case BattleStep.None:
                    break;
            }
        }
        
        private IEnumerator ShowBattleResultWithTaskCoroutine()
        {
            CurrentBattleStep.Value = BattleStep.PreResult;
            var result = _currentBattleResult.Value;
            
            // Task가 있고 승리한 경우 완료될 때까지 대기
            if (result.IsVictory && _levelUpTask != null)
            {
                Debug.Log("[BattleManager] ShowBattleResultWithTaskCoroutine: Waiting for level up task to complete");
                // TODO: 로딩 UI 표시
                
                // Task 완료 대기 (UI 응답 유지)
                while (!_levelUpTask.IsCompleted)
                {
                    yield return null;
                }
                
                // TODO: 로딩 UI 숨김
                
                // Task 결과 저장
                try
                {
                    _completedLevelUpResult = _levelUpTask.Result;
                }
                catch (AggregateException ex)
                {
                    if (ex.InnerException is OperationCanceledException)
                    {
                        Debug.Log("[BattleManager] Level up calculation was cancelled.");
                    }
                    else
                    {
                        Debug.LogError($"[BattleManager] Level up calculation failed: {ex}");
                    }
                    _completedLevelUpResult = null;
                }
            }
            else if (!result.IsVictory && _levelUpTask != null)
            {
                // 패배했는데 경험치 계산이 진행 중이면 취소
                Debug.Log("[BattleManager] Battle lost - cancelling level up calculation");
                CancelLevelUpCalculation();
            }
            
            // 결과 UI 표시 (Task 결과 전달)
            _battleResultController.ShowBattleResult(result, _completedLevelUpResult);
            
            // 이제 BattleStep.Result로 전환 (Task가 확실히 완료된 상태)
            CurrentBattleStep.Value = BattleStep.Result;
        }
        
        /// <summary>
        /// 레벨업 계산 Task를 취소하고 리소스 정리
        /// </summary>
        private void CancelLevelUpCalculation()
        {
            if (_levelUpCts != null)
            {
                Debug.Log("[BattleManager] CancelLevelUpCalculation");
                _levelUpCts.Cancel();
                _levelUpCts.Dispose();
                _levelUpCts = null;
            }
            _levelUpTask = null;
            _expectedEXP = 0;
            _expectedGold = 0;
            _completedLevelUpResult = null;
        }
        
        /// <summary>
        /// 몬스터 보상을 계산하고 비동기 레벨업 계산 시작
        /// </summary>
        private void StartLevelUpCalculation(MonsterData monsterData)
        {
            // 몬스터 보상 계산
            BigInteger baseExp = monsterData.exp;
            int baseGold = monsterData.gold;
            
            // LUC 스탯에 따른 보정
            int luc = GameManager.GetStat(StatType.LUC);
            float lucMultiplier = CalculateLucMultiplier(luc, monsterData.level);
            lucMultiplier = Mathf.Min(lucMultiplier, 2.0f);
            
            // Relic 효과를 보상에 적용
            var context = new RelicEffectContext
            {
                ExpMultiplier = 1.0f + PersistentGameState.Instance.GetBoostExp() / 100f,
                GoldMultiplier = (1.0f + PersistentGameState.Instance.GetBoostGold() / 100f) * lucMultiplier,
            };
            GameManager.Instance.RelicManager.ApplyEffects(context);
            
            // 최종 보상 계산 및 저장
            _expectedEXP = PlayerStats.CalculateAdjustedEXP(
                    baseExp,
                    GameManager.Instance.GameState.PlayerStats.Level.Value,
                    monsterData.level) * Mathf.RoundToInt(context.ExpMultiplier * 100f) / 100;
            if (_regionEffect == RegionEffectType.BoostExpSmall || _regionEffect == RegionEffectType.BoostExpLarge)
            {
                _expectedEXP = _expectedEXP * (_regionEffect == RegionEffectType.BoostExpSmall ? 15 : 20) / 10;
            }
            _expectedGold = Mathf.RoundToInt(baseGold * context.GoldMultiplier);
            if (_regionEffect == RegionEffectType.BoostGoldSmall || _regionEffect == RegionEffectType.BoostGoldLarge)
            {
                _expectedGold = _expectedGold * (_regionEffect == RegionEffectType.BoostGoldSmall ? 15 : 20) / 10;
            }
            
            // CancellationTokenSource 생성
            _levelUpCts = new CancellationTokenSource();
            
            // 비동기로 레벨업 계산 시작
            _levelUpTask = GameManager.Instance.GameState.PlayerStats.CalculateLevelUpAsync(_expectedEXP, _levelUpCts.Token);
        }

        public ReactiveProperty<BattleContext> StartBattle(MonsterData monsterData, RegionEffectType regionEffect, Action<BattleContext> onBattleEnd)
        {
            // 전투가 이미 진행 중이면 무시
            if (CurrentBattleStep.Value != BattleStep.None)
            {
                Debug.Log("[BattleManager] Already in battle.");
                return null;
            }
            
            if (_battleController == null)
            {
                Debug.LogError("[BattleManager] HUD Controller not initialized.");
                return null;
            }

            CurrentBattleStep.Value = BattleStep.Prepare;

            _monsterData = monsterData;
            _regionEffect = regionEffect;
            _onBattleEnd = onBattleEnd;

            var weapon = PersistentGameState.Instance.GetCurrentWeapon();
            var armor = PersistentGameState.Instance.GetCurrentArmor();

            if (weapon == null || armor == null)
            {
                Debug.LogError("[BattleManager] Weapon or armor not found.");
                return null;
            }

            // 전투 시뮬레이터 생성
            _simulator = new BattleSimulator(
                weapon,
                armor,
                monsterData,
                regionEffect,
                _battleController,
                HIT_DELAY - HIT_DELAY_INTERVAL * OptionState.Instance.CombatSpeed.Value
            );

            // HUD 컨트롤러에 시뮬레이터 설정
            _battleController.SetBattleSimulator(_simulator);

            // 전투 UI 표시 및 시작 버튼 활성화
            _battleController.PrepareBattle(monsterData);

            // 몬스터 보상 계산 및 비동기 레벨업 계산 시작 (전투 준비 단계부터 계산)
            StartLevelUpCalculation(monsterData);
            StartCoroutine(SetBattleStepDelayedCoroutine());

            // 이 전투의 결과를 위한 ReactiveProperty 생성
            _currentBattleResult = new ReactiveProperty<BattleContext>(null);
            return _currentBattleResult;
        }
        
        private IEnumerator SetBattleStepDelayedCoroutine()
        {
            yield return new WaitForSeconds(0.5f);
            CurrentBattleStep.Value = BattleStep.PrepareComplete;
        }

        public void GiveupBattle()
        {
            if (CurrentBattleStep.Value != BattleStep.Prepare &&
                CurrentBattleStep.Value != BattleStep.PrepareComplete &&
                CurrentBattleStep.Value != BattleStep.Simulating &&
                CurrentBattleStep.Value != BattleStep.PreEnd &&
                CurrentBattleStep.Value != BattleStep.End) return;
            
            // 레벨업 계산 Task 취소
            CancelLevelUpCalculation();
            
            var looseContext = new BattleContext
            {
                IsVictory = false,
                EarnedBattlePoint = -3,
                EarnedEXP = 0,
                EarnedGold = 0,
                DroppedItems = new List<DropEntry>(),
            };
            EndBattle(looseContext, null, _monsterData, false);
            OnClickBattleUI();
        }

        private void ExecuteBattle(MonsterData monsterData)
        {
            // 전투 상태 시작
            CurrentBattleStep.Value = BattleStep.Simulating;

            // 전투 시뮬레이션 실행
            _simulateBattleCoroutine = StartCoroutine(SimulateBattleCoroutine(_simulator, monsterData));
        }

        private IEnumerator SimulateBattleCoroutine(BattleSimulator simulator, MonsterData monsterData)
        {
            var result = new BattleContext();

            // 전투 시작
            while (!simulator.IsBattleEnded)
            {
                // 플레이어의 공격
                yield return simulator.SimulatePlayerAttackCoroutine(result);
                if (simulator.IsMonsterDead) break;

                // 몬스터의 공격
                yield return new WaitForSeconds(BATTLE_STEP_DELAY - BATTLE_STEP_DELAY_INTERVAL * OptionState.Instance.CombatSpeed.Value);
                simulator.SimulateMonsterAttack(result);
                if (simulator.IsPlayerDead) break;

                // 매 턴 체력 회복
                if (GameManager.Instance.TryGetRelic<HealEveryTurnEffect>(out Relic relic))
                {
                    yield return new WaitForSeconds((BATTLE_STEP_DELAY - BATTLE_STEP_DELAY_INTERVAL * OptionState.Instance.CombatSpeed.Value) / 2);
                    int healAmount = (relic.Effect as HealEveryTurnEffect).GetHealAmount();
                    simulator.CurrentPlayerHP.Value =
                        Math.Min(simulator.CurrentPlayerHP.Value + healAmount, simulator.MaxPlayerHP);
                    _battleController.RenderHeal(healAmount);
                    yield return new WaitForSeconds((BATTLE_STEP_DELAY - BATTLE_STEP_DELAY_INTERVAL * OptionState.Instance.CombatSpeed.Value) / 2);
                }
                else
                {
                    yield return new WaitForSeconds(BATTLE_STEP_DELAY - BATTLE_STEP_DELAY_INTERVAL * OptionState.Instance.CombatSpeed.Value);
                }
                
                result.TurnCount++;
            }

            EndBattle(result, simulator, monsterData, true);
        }

        private void EndBattle(BattleContext result, BattleSimulator simulator, MonsterData monsterData, bool wait)
        {
            if (_simulateBattleCoroutine != null)
            {
                StopCoroutine(_simulateBattleCoroutine);
            }

            _simulateBattleCoroutine = null;

            CurrentBattleStep.Value = BattleStep.PreEnd;
            
            // 전투 결과 메시지
            if (simulator != null && simulator.IsMonsterDead)
            {
                if (GameManager.Instance.TryGetRelic<DamageBoostPerKillEffect>(out Relic killDamageRelic))
                {
                    killDamageRelic.Stack++;
                }

                result.IsVictory = true;
                
                // ExecuteBattle에서 미리 계산된 보상 사용 (중복 계산 방지)
                result.EarnedEXP = _expectedEXP;
                result.EarnedGold = _expectedGold;
                
                // 드롭 아이템 계산 (LUC 스탯 및 Relic 효과 적용)
                int luc = GameManager.GetStat(StatType.LUC);
                float lucMultiplier = CalculateLucMultiplier(luc, monsterData.level);
                Debug.Log($"Luc Multiplier: {lucMultiplier}");
                var context = new RelicEffectContext
                {
                    ExpMultiplier = 1.0f,
                    GoldMultiplier = 1.0f,
                };
                GameManager.Instance.RelicManager.ApplyEffects(context);
                
                // 드롭 아이템 계산
                foreach (var drop in monsterData.drops)
                {
                    var randomValue = UnityEngine.Random.value;
                    var dropRate = drop.dropRate * lucMultiplier * context.DropRateMultiplier * LibraryManager.GetMonsterDropMultiplier(PersistentGameState.Instance.CodexState.GetMobKillCount(monsterData.code));
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
                                Debug.Log($"Checking drop {drop.itemType} {drop.relicEffectName}");
                                var myRelic = PersistentGameState.Instance.CodexState.GetRelic(drop.relicEffectName);
                                if (myRelic != null && myRelic.Level >= drop.relicLevel)
                                {
                                    Debug.Log($"Relic {drop.relicEffectName} level {drop.relicLevel} already unlocked");
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
                
                // 레벨업 계산 Task 취소
                CancelLevelUpCalculation();

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
            _simulator = null;
            
            if (wait)
            {
                StartCoroutine(SetBattleStepEndDelayedCoroutine());
            }
            else
            {
                CurrentBattleStep.Value = BattleStep.End;
            }
        }
        
        private IEnumerator SetBattleStepEndDelayedCoroutine()
        {
            yield return new WaitForSeconds(0.5f);
            CurrentBattleStep.Value = BattleStep.End;
        }

        private void ApplyBattleResult(BattleContext result)
        {
            GameManager.Instance.GameState.CombatInfo.IncrementCombatCount(_monsterData);
            if (result.IsVictory)
            {
                // 경험치 적용 (ShowBattleResult에서 이미 완료된 결과 사용)
                if (_completedLevelUpResult.HasValue)
                {
                    GameManager.Instance.GameState.PlayerStats.ApplyLevelUpResult(_completedLevelUpResult.Value);
                    
                    // CTS 정리
                    _levelUpCts?.Dispose();
                    _levelUpCts = null;
                    _levelUpTask = null;
                    _completedLevelUpResult = null;
                }
                else
                {
                    // fallback: 기존 방식 사용
                    GameManager.Instance.GameState.PlayerStats.AddEXP(result.EarnedEXP);
                }
                
                PersistentGameState.Instance.AddGold(result.EarnedGold);
                PersistentGameState.Instance.CodexState.AddMobKillCount(_monsterData.code);

                // 드롭 아이템 적용
                foreach (var entry in result.DroppedItems)
                {
                    Debug.Log($"Applying drop {entry.itemType} {entry.itemId}");
                    if (entry.itemType == DropType.Relic)
                    {
                        Debug.Log($"Unlocking relic {entry.relicEffectName} {entry.relicLevel}");
                        var relicLevel = entry.relicLevel;
                        PersistentGameState.Instance.CodexState.UnlockRelicWithLevel(entry.relicEffectName, relicLevel);
                        GameManager.Instance.GameState.AddDroppedItem(new DropEntryData
                        {
                            itemType = DropType.Relic,
                            relicEffectName = entry.relicEffectName,
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
                            CurrentBattleStep.Value = BattleStep.RelicReward;
                            GameManager.Instance.RelicReward(int.Parse(action.values[0]), new Relic[] { }, () => CurrentBattleStep.Value = BattleStep.None);
                            break;
                        case MonsterActionType.None:
                            break;
                    }
                }
                
                if (_monsterData.type == MonsterType.Boss)
                {
                    PersistentGameState.Instance.AddSlainedBoss(_monsterData.code);
                    
                    // TODO: 임시 - 최종 보스(boss_11) 처치 시 경고 표시
                    if (_monsterData.code == "boss_11")
                    {
                        ConfirmationController.Instance.ShowWarning(Utils.GetUIString("wait-for-update"));
                    }
                }

            }
            else
            {
                GameManager.Instance.GameState.CombatInfo.IncrementLosses();
            }

            GameManager.Instance.GameState.PlayerStats.AddBattlePoint(result.EarnedBattlePoint);
            GameManager.Instance.GameState.UpdateRegionEffect();
            _onBattleEnd?.Invoke(result);   
            OnBattleEnd?.Invoke();
            GameManager.Instance.Save();
            if (CurrentBattleStep.Value != BattleStep.RelicReward)
            {
                CurrentBattleStep.Value = BattleStep.None;
            }
        }

        private float CalculateLucMultiplier(int luc, int monsterLevel)
        {
            float level = Mathf.Max(monsterLevel, 1f);
            return 1.0f + 1.66f * Mathf.Log10(1f + (float)luc / level);
        }
    }
} 