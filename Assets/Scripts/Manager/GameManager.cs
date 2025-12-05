using LoopLegacy.Battle;
using LoopLegacy.Battle.RelicEffects;
using LoopLegacy.Loader;
using LoopLegacy.Player;
using LoopLegacy.Region;
using LoopLegacy.State;
using LoopLegacy.UI.Controller;
using R3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LoopLegacy.Manager
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance => _instance;

        public ReactiveProperty<bool> IsMovingMap { get; private set; }

        public GameState GameState { get; private set; }
        public EncounterManager EncounterManager { get; private set; }
        public RegionDetector RegionDetector { get; private set; }
        public RelicManager RelicManager { get; private set; }

        [SerializeField]
        private GameplayHUDController _hudController;
        public GameplayHUDController HUDController => _hudController;

        [SerializeField]
        private GameOverController _gameOverController;
        public GameOverController GameOverController => _gameOverController;

        [SerializeField]
        private RelicRewardController _relicRewardController;

        private IDisposable _battlePointSubscription;

        private void Awake()
        {
            _instance = this;
            IsMovingMap = new ReactiveProperty<bool>(false);
            InitializeGame();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                // 게임 종료 시 정리
                EncounterManager?.Dispose();
                _battlePointSubscription?.Dispose();
                _instance = null;
            }
        }

        private void InitializeGame()
        {
            Debug.Log("[GameManager] InitializeGame");
            bool isInGame = PersistentGameState.Instance.IsInGame;
            Debug.Log("[GameManager] InitializeGame isInGame: " + isInGame);
            if (isInGame)
            {
                var path = GetSavePath(PersistentGameState.Instance.CurrentSlotIndex);
                Debug.Log("[GameManager] InitializeGame path: " + path);
                if (!File.Exists(path))
                {
                    Debug.LogError($"[GameManager] Game state file not found. (path: {path})");
                    isInGame = false;
                    GameState = new GameState();
                }
                else
                {
                    try
                    {
                        GameState = new GameState(File.ReadAllText(path));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[GameManager] Error loading game state: {e.Message}");
                        isInGame = false;
                        GameState = new GameState();
                    }
                }
            }
            else
            {
                Debug.Log("[GameManager] InitializeGame isInGame is false. Create new game state.");
                GameState = new GameState();
            }

            // RegionDetector 초기화
            RegionDetector = new RegionDetector();
            
            // EncounterManager 초기화
            EncounterManager = new EncounterManager(RegionDetector);

            RelicManager = new RelicManager();

            if (isInGame)
            {
                Debug.Log("[GameManager] InitializeGame isInGame is true. Load game state.");
                EncounterManager.EncounterGauge.Value = GameState.CurrentEncounterGauge;
            }
            else
            {
                Debug.Log("[GameManager] InitializeGame isInGame is false. Start new loop.");
                StartNewLoop();
            }

            _battlePointSubscription = GameState.PlayerStats.BattlePoint.Subscribe(battlePoint => {
                if (battlePoint <= 0)
                {
                    OnBattlePointZero();
                }
            });
        }

        public void StartNewLoop()
        {
            PersistentGameState.Instance.SetIsInGame(true);
            GameState.CurrentMapCode = "Start";

            // BP 초기화
            GameState.PlayerStats.BattlePoint.Value = 30;
            
            // 골드 초기화
            PersistentGameState.Instance.Gold.Value = 0;
            
            // 레벨 초기화
            GameState.PlayerStats.Level.Value = 1;
            GameState.PlayerStats.EXP.Value = 0;
            InitializeStats();

            // 초기 유물
            string initialRelicEffectName = PersistentGameState.Instance.HouseState.InitialRelic.Value;
            if (initialRelicEffectName != string.Empty)
            {
                AcquireRelic(initialRelicEffectName);
            }

            Save();

            #if UNITY_EDITOR
            EncounterManager.SetEnabled(false);
            #endif
        }

        public void InitializeStats()
        {
            // 기본 스탯 초기화 (기본값 + 영지에서 업그레이드한 값)
            foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            {
                GameState.PlayerStats.InitializeStats(stat, PersistentGameState.Instance.GetBaseStat(stat));
            }
        }

        public void OnBattlePointZero()
        {
            _gameOverController.Show(
                PersistentGameState.Instance.CurrentLoopCount + 1,
                GameState.DroppedItems.ToArray(),
                GameState.CombatInfo,
                GameState.PlayerStats.Level.Value,
                PersistentGameState.Instance.Gold.Value
            );
        }

        public void EndLoop()
        {
            EncounterManager.Dispose();
            PersistentGameState.Instance.IncrementLoopCount();
            PersistentGameState.Instance.IncrementAccumulatedLevel(GameState.PlayerStats.Level.Value);
            PersistentGameState.Instance.SetIsInGame(false);

            // 현재 열린 슬롯에 PersistentGameState 저장
            PersistentGameState.Instance.SaveState();
            GameEssentials.Instance.DestroyEssentials();
        }

        public void Save()
        {
            PersistentGameState.Instance.SaveState();
            if (PersistentGameState.Instance.IsInGame)
            {
                var slotIndex = PersistentGameState.Instance.CurrentSlotIndex;
                if (slotIndex == -1)
                {
                    Debug.Log("[GameState] Debug instance does not save.");
                    return;
                }

                try
                {
                    if (PlayerMovement.Instance != null)
                    {
                        GameState.PlayerPosition = PlayerMovement.Instance.transform.position;
                    }
                    
                    GameState.CurrentEncounterGauge = EncounterManager.EncounterGauge.Value;
                    var saveData = GameState.ToJson();
                    string path = GetSavePath(slotIndex);
                    File.WriteAllText(path, saveData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[GameState] Error saving game state: {e.Message} {e.StackTrace}");
                    throw e;
                }
            }
        }

        public static string GetSavePath(int slotIndex)
        {
            return Path.Combine(Application.persistentDataPath, $"game_state_{slotIndex}.json");
        }

        public void MoveToMap(string mapCode, Vector2 position)
        {
            IsMovingMap.Value = true;
            FadeController.LoadScene(
                $"G-{mapCode}",
                FadeController.MAP_MOVE_DELAY,
                onLoadComplete: () =>
                {
                    Debug.Log("[GameManager] MoveToMap: onLoadComplete");
                    PlayerMovement.Instance.SetPosition(position);
                    CameraFollow.Instance.SetPosition(position);
                },
                onFadeComplete:() => IsMovingMap.Value = false);
        }

#region Relic related methods
        public static int GetStat(StatType statType)
        {
            if (Instance == null)
            {
                return Mathf.RoundToInt(PersistentGameState.Instance.GetBaseStat(statType) * LibraryManager.GetStatMultiplier(PersistentGameState.Instance.AccumulatedLevel));
            }

            return Mathf.RoundToInt((Instance.GameState.PlayerStats.Stats[(int)statType].Value + Instance.RelicManager.GetStatBoost()[statType]) * LibraryManager.GetStatMultiplier(PersistentGameState.Instance.AccumulatedLevel));
        }

        public void RelicReward(int weight, Relic[] alreadyRolledRelics, Action onComplete)
        {
            int relicCount = PersistentGameState.Instance.HouseState.GetUpgradeValue(UpgradeType.RelicRewardChoiceCount);
            List<Relic> relics = PersistentGameState.Instance.CodexState.GetAvailableRelics();

            if (GameState.IsRestartingWithGold)
            {
                // 두 번 재시작 불가능
                relics = relics.Where(r => r.Effect is not RestartWithGoldEffect).ToList();
            }
            
            // type이 증가할수록 모든 등급의 가중치가 1:1:1:1로 수렴
            // type에 따른 보간 계수 (0.0 ~ 1.0)
            weight += PersistentGameState.Instance.HouseState.GetUpgradeValue(UpgradeType.RelicRewardRarity);
            weight = Mathf.Clamp(weight, 0, 10);
            float t = Mathf.Clamp01(weight * 0.1f); // weight 10이면 완전히 1:1:1:1
            
            // 원래 가중치
            float baseCommon = 120f;
            float baseUncommon = 60f;
            float baseRare = 20f;
            float baseEpic = 5f;
            
            // 목표 가중치 (모두 동일)
            float targetWeight = 1.0f;
            
            var pickedRelics = Utils.PickRelics(
                relics: relics,
                excludeRelics:GameState.OwnedRelics.Value,
                alreadyRolledRelics: alreadyRolledRelics,
                rarityWeights: new Dictionary<RelicGrade, float>
                {
                    { RelicGrade.Common, Mathf.Lerp(baseCommon, targetWeight, t) },
                    { RelicGrade.Uncommon, Mathf.Lerp(baseUncommon, targetWeight, t) },
                    { RelicGrade.Rare, Mathf.Lerp(baseRare, targetWeight, t) },
                    { RelicGrade.Epic, Mathf.Lerp(baseEpic, targetWeight, t) },
                },
                count: relicCount);
            _relicRewardController.Show(weight, pickedRelics, alreadyRolledRelics, onComplete);
        }

        public void UnlockRelic(string effectName, int level)
        {
            PersistentGameState.Instance.CodexState.UnlockRelicWithLevel(effectName, level);
            Save();
        }

        public void AcquireRelic(string effectName)
        {
            var relic = PersistentGameState.Instance.CodexState.GetRelic(effectName);
            if (relic == null)
            {
                Debug.LogError($"[GameManager] AcquireRelic: Relic not found. (effectName: {effectName})");
                return;
            }

            if (GameState.OwnedRelics.Value.Count >= PersistentGameState.Instance.GetMaxRelicCount())
            {
                Debug.LogError($"[GameManager] AcquireRelic: Max relic count reached. (maxRelicCount: {PersistentGameState.Instance.GetMaxRelicCount()})");
                return;
            }

            var newRelics = GameState.OwnedRelics.Value.ToList();
            newRelics.Add(relic);
            GameState.OwnedRelics.Value = newRelics;
            // 얻는 순간 적용되는 효과들을 적용
            if (relic.Effect is BattlePointBoostEffect bpbe)
            {
                GameState.PlayerStats.AddBattlePoint(bpbe.GetBattlePointIncrease());
            }
            if (relic.Effect is RestartWithGoldEffect gre)
            {
                // 테스트 필요
                MoveToMap("Start", new Vector3(0f, 0f));
                int gold = PersistentGameState.Instance.Gold.Value;
                GameState.InitializeDefaultState();
                StartNewLoop();
                PersistentGameState.Instance.Gold.Value = gold;
            }
            Save();
        }

        public void DiscardRelic(string relicEffectName)
        {
            var removedRelic = GameState.OwnedRelics.Value.FirstOrDefault(r => r.EffectName == relicEffectName);
            if (removedRelic == null)
            {
                return;
            }

            var newRelics = GameState.OwnedRelics.Value.ToList();
            newRelics.Remove(removedRelic);
            GameState.OwnedRelics.Value = newRelics;
            // 얻는 순간 적용된 효과들을 제거
            if (removedRelic.Effect is BattlePointBoostEffect bpbe)
            {
                GameState.PlayerStats.AddBattlePoint(-bpbe.GetBattlePointIncrease());
            }
            Save();
        }

        public bool TryGetRelic<T>(out Relic relic) where T : RelicEffect
        {
            if (!GameState.OwnedRelics.Value.Any(r => r.Effect is T))
            {
                relic = null;
                return false;
            }
            else
            {
                relic = GameState.OwnedRelics.Value.First(r => r.Effect is T);
                return true;
            }
        }
#endregion
    }
} 