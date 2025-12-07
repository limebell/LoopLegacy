using LoopLegacy.Battle;
using LoopLegacy.Battle.RelicEffects;
using LoopLegacy.Manager;
using R3;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LoopLegacy.State
{
    public class PlayerStats : IDisposable
    {
        public ReactiveProperty<int> BattlePoint { get; private set; }
        public ReactiveProperty<int> StatPoints { get; private set; }
        public ReactiveProperty<int>[] Stats { get; private set; }
        
        public ReactiveProperty<BigInteger> EXP { get; private set; }
        public ReactiveProperty<int> Level { get; private set; }

        public static float CRIT_MULTIPLIER = 1.5f;

        public PlayerStats()
        {
            StatPoints = new ReactiveProperty<int>(0);
            BattlePoint = new ReactiveProperty<int>(30);
            EXP = new ReactiveProperty<BigInteger>(0);
            Level = new ReactiveProperty<int>(1);
            Stats = new ReactiveProperty<int>[Enum.GetValues(typeof(StatType)).Length];
            for (int i = 0; i < Stats.Length; i++)
            {
                Stats[i] = new ReactiveProperty<int>(0);
            }
        }

        public PlayerStats(string json)
        {
            var saveData = JsonUtility.FromJson<PlayerStatsSaveData>(json);
            Stats = saveData.stats.Select(stat => new ReactiveProperty<int>(stat)).ToArray();
            StatPoints = new ReactiveProperty<int>(saveData.statPoints);
            BattlePoint = new ReactiveProperty<int>(saveData.battlePoint);
            EXP = new ReactiveProperty<BigInteger>(saveData.exp);
            Level = new ReactiveProperty<int>(saveData.level);
        }

        public void Dispose()
        {
            BattlePoint?.Dispose();
            StatPoints?.Dispose();
            EXP?.Dispose();
            Level?.Dispose();
            foreach (var stat in Stats)
            {
                stat?.Dispose();
            }
        }

        public void InitializeDefaultValues()
        {
            StatPoints.Value = 0;
            BattlePoint.Value = 30;
            EXP.Value = 0;
            Level.Value = 1;
        }

        public void InitializeStats(StatType stat, int amount)
        {
            Stats[(int)stat].Value = amount;
        }

        public void AddStat(StatType stat, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            if (StatPoints.Value < amount)
            {
                Debug.LogError("Not enough stat points");
                return;
            }

            StatPoints.Value -= amount;
            int statMultiplier = 1;
            switch (stat)
            {
                case StatType.HP:
                    statMultiplier = 4;
                    break;
                case StatType.ATK:
                    statMultiplier = 2;
                    break;
                case StatType.DEF:
                    statMultiplier = 2;
                    break;
                case StatType.LUC:
                    statMultiplier = 1;
                    break;
                default:
                    break;
            }
            Stats[(int)stat].Value += amount * statMultiplier;
        }

        /// <summary>
        /// 경험치를 추가하고 즉시 레벨업 처리 (O(레벨업 횟수))
        /// 
        /// 주의: 많은 레벨업이 예상되면 CalculateLevelUpAsync + ApplyLevelUpResult 사용 권장
        /// </summary>
        public void AddEXP(BigInteger amount)
        {
            EXP.Value += amount;
            CheckLevelUp();

            if (PersistentGameState.Instance.AutoDistributeStats)
            {
                // 남는 스탯은 가장 높은 비율의 스탯에 더해줌
                int totalStats = StatPoints.Value;
                int[] autoDistributeRate = PersistentGameState.Instance.GetAutoDistributeRate();
                int sum = autoDistributeRate.Sum();
                int[] statsToAdd = autoDistributeRate.Select(rate => (int)(rate * (double)totalStats / sum)).ToArray();
                int heighest = -1;
                int heighestIndex = 0;

                for (int i = 0; i < autoDistributeRate.Length; i++)
                {
                    if (statsToAdd[i] > heighest)
                    {
                        heighest = statsToAdd[i];
                        heighestIndex = i;
                    }
                }

                int statsLeft = totalStats - statsToAdd.Sum();
                if (statsLeft > 0)
                {
                    statsToAdd[heighestIndex] += statsLeft;
                }

                for (int i = 0; i < autoDistributeRate.Length; i++)
                {
                    AddStat((StatType)i, statsToAdd[i]);
                }
            }
        }

        public void AddBattlePoint(int amount)
        {
            BattlePoint.Value = Mathf.Max(0, BattlePoint.Value + amount); // BattlePoint는 0 이하로 내려가지 않도록
        }
    
        private void CheckLevelUp()
        {
            int statPointPerLevel = 4;
            // 유물 효과 적용
            if (GameManager.Instance?.TryGetRelic<StatPointIncreaseEffect>(out Relic relic) ?? false)
            {
                statPointPerLevel += (relic.Effect as StatPointIncreaseEffect).GetIncreaseAmount();
            }
            
            // 레벨업 가능한 만큼 처리
            while (EXP.Value > 0)
            {
                BigInteger needed = new BigInteger((long)Math.Pow(Level.Value, 1.4));
                if (EXP.Value >= needed)
                {
                    EXP.Value -= needed;
                    Level.Value++;
                    StatPoints.Value += statPointPerLevel;
                }
                else
                {
                    break;
                }
            }
        }
        
        /// <summary>
        /// 현재 레벨과 EXP로 도달 가능한 최대 레벨을 계산 (n^1.4 공식)
        /// 점진적 계산으로 O(레벨업 횟수) 시간 복잡도
        /// </summary>
        private int CalculateMaxLevelFromEXP(int currentLevel, BigInteger currentEXP)
        {
            BigInteger remainingEXP = currentEXP;
            int level = currentLevel;
            
            // 레벨업 가능한 만큼 계산
            while (remainingEXP > 0)
            {
                BigInteger needed = new BigInteger((long)Math.Pow(level, 1.4));
                if (remainingEXP >= needed)
                {
                    remainingEXP -= needed;
                    level++;
                }
                else
                {
                    break;
                }
            }
            
            return level;
        }
        
        public int ExpectedLevel(BigInteger exp)
        {
            // Return the level that the player will reach when the exp is added without level up
            // 추가될 EXP와 현재 EXP를 합쳐서 도달 가능한 최대 레벨을 한번에 계산
            BigInteger totalExp = EXP.Value + exp;
            return CalculateMaxLevelFromEXP(Level.Value, totalExp);
        }
        
        /// <summary>
        /// 비동기로 레벨업 결과를 계산 (전투 중 미리 계산용)
        /// 
        /// 사용 예시:
        /// 1. 몬스터 조우 시:
        ///    var cts = new CancellationTokenSource();
        ///    var task = playerStats.CalculateLevelUpAsync(monsterExp, cts.Token);
        /// 2. 전투 결과창에서:
        ///    var result = await task;
        ///    displayExpectedLevel(result.FinalLevel);
        /// 3. 경험치 적용 시:
        ///    playerStats.ApplyLevelUpResult(result);
        /// 4. 취소 시:
        ///    cts.Cancel();
        /// </summary>
        public async Task<LevelUpResult> CalculateLevelUpAsync(BigInteger expToAdd, CancellationToken cancellationToken = default)
        {
            // 비동기 작업으로 실행 (메인 스레드 블로킹 방지)
            return await Task.Run(() =>
            {
                Debug.Log("[PlayerStats] CalculateLevelUpAsync Started: " + EXP.Value + " + " + expToAdd + " = " + (EXP.Value + expToAdd));
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                BigInteger totalExp = EXP.Value + expToAdd;
                BigInteger remainingEXP = totalExp;
                int currentLevel = Level.Value;
                
                // 레벨업 계산
                while (remainingEXP > 0)
                {
                    // 취소 요청 확인
                    if (cancellationToken.IsCancellationRequested)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    
                    BigInteger needed = new BigInteger((long)Math.Pow(currentLevel, 1.4));
                    if (remainingEXP >= needed)
                    {
                        remainingEXP -= needed;
                        currentLevel++;
                    }
                    else
                    {
                        break;
                    }
                }
                
                int levelUps = currentLevel - Level.Value;
                Debug.Log("[PlayerStats] CalculateLevelUpAsync Finished: " + Level.Value + " -> " + currentLevel + " (" + levelUps + " levels) in " + stopwatch.Elapsed.TotalMilliseconds + "ms");
                return new LevelUpResult
                {
                    InitialLevel = Level.Value,
                    FinalLevel = currentLevel,
                    LevelUps = levelUps,
                    InitialEXP = EXP.Value,
                    AddedEXP = expToAdd,
                    RemainingEXP = remainingEXP
                };
            }, cancellationToken);
        }
        
        /// <summary>
        /// 미리 계산된 레벨업 결과를 O(1) 시간에 적용
        /// </summary>
        public void ApplyLevelUpResult(LevelUpResult result)
        {
            if (result.LevelUps <= 0) return;
            
            // 스탯 포인트 계산
            int statPointPerLevel = 4;
            if (GameManager.Instance?.TryGetRelic<StatPointIncreaseEffect>(out Relic relic) ?? false)
            {
                statPointPerLevel += (relic.Effect as StatPointIncreaseEffect).GetIncreaseAmount();
            }
            
            // O(1) 시간에 한번에 적용
            Level.Value = result.FinalLevel;
            EXP.Value = result.RemainingEXP;
            StatPoints.Value += result.LevelUps * statPointPerLevel;
            
            // 자동 스탯 분배
            if (PersistentGameState.Instance.AutoDistributeStats)
            {
                int totalStats = StatPoints.Value;
                int[] autoDistributeRate = PersistentGameState.Instance.GetAutoDistributeRate();
                int sum = autoDistributeRate.Sum();
                int[] statsToAdd = autoDistributeRate.Select(rate => (int)(rate * (double)totalStats / sum)).ToArray();
                int highest = -1;
                int highestIndex = 0;

                for (int i = 0; i < autoDistributeRate.Length; i++)
                {
                    if (statsToAdd[i] > highest)
                    {
                        highest = statsToAdd[i];
                        highestIndex = i;
                    }
                }

                int statsLeft = totalStats - statsToAdd.Sum();
                if (statsLeft > 0)
                {
                    statsToAdd[highestIndex] += statsLeft;
                }

                for (int i = 0; i < autoDistributeRate.Length; i++)
                {
                    AddStat((StatType)i, statsToAdd[i]);
                }
            }
        }

        public static BigInteger CalculateRequiredEXP(int level)
        {
            return new BigInteger((long)Math.Pow(level, 1.4));
        }
        
        public static BigInteger CalculateAdjustedEXP(BigInteger baseExp, int playerLevel, int monsterLevel)
        {
            double levelRatio = Math.Pow((double)monsterLevel / playerLevel, 2);
            
            // 지수적 완화: (1 - 0.99^level) 공식 사용
            // 레벨 1: 0.01, 레벨 10: 0.096, 레벨 50: 0.39, 레벨 100: 0.63
            double easingFactor = 1.0 - Math.Pow(0.99, playerLevel);
            // 최소 0.3배, 최대 2배
            double adjustedRatio = Math.Clamp(1.0 + (levelRatio - 1.0) * easingFactor, 0.3, 2.0);
            
            return new BigInteger((double)baseExp * adjustedRatio);
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(new PlayerStatsSaveData
            {
                stats = Stats.Select(stat => stat.Value).ToArray(),
                statPoints = StatPoints.Value,
                battlePoint = BattlePoint.Value,
                exp = EXP.Value,
                level = Level.Value,
            });
        }

        [Serializable]
        public class PlayerStatsSaveData
        {
            public int[] stats;
            public int statPoints;
            public int battlePoint;
            public BigInteger exp;
            public int level;
        }
    }
    
    /// <summary>
    /// 레벨업 계산 결과 (비동기로 미리 계산용)
    /// </summary>
    public struct LevelUpResult
    {
        public int InitialLevel;      // 시작 레벨
        public int FinalLevel;        // 최종 레벨
        public int LevelUps;          // 레벨업 횟수
        public BigInteger InitialEXP; // 시작 경험치
        public BigInteger AddedEXP;   // 추가된 경험치
        public BigInteger RemainingEXP; // 남은 경험치
    }
}
