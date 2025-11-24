using LoopLegacy.Battle;
using LoopLegacy.Battle.RelicEffects;
using LoopLegacy.Loader;
using LoopLegacy.Region;
using LoopLegacy.State;
using R3;
using System;
using System.Linq;
using UnityEngine;

namespace LoopLegacy.Manager
{
    public class EncounterManager : IDisposable
    {
        public ReactiveProperty<float> EncounterGauge { get; private set; }
        private readonly RegionDetector _playerRegionDetector;

        // 엔카운터 관련 상수
        private const float MAX_GAUGE = 100f;
        private const float MIN_GAUGE = 0f;
        private const float ENCOUNTER_GAUGE_INCREASE = 15.0f; // 이동 시도마다 증가할 게이지 양
        private const float MIN_ENCOUNTER_THRESHOLD = 20f;  // 최소 엔카운터 게이지 (이 이하는 절대 만나지 않음)
        private const float MAX_ENCOUNTER_THRESHOLD = 100f; // 최대 엔카운터 게이지
        private const float ENCOUNTER_CHECK_INTERVAL = 0.1f;  // 엔카운터 체크 간격 (초)
        private float _lastEncounterCheckTime = 0;
        private bool _enabled = true;

        public EncounterManager(RegionDetector playerRegionDetector)
        {
            _playerRegionDetector = playerRegionDetector;
            EncounterGauge = new ReactiveProperty<float>(0f);
        }

        public void AddGauge(float fixedDeltaTime)
        {
            if (!_enabled) return;
            if (_playerRegionDetector.LastRegion.Value == null) return;
            float increase = ENCOUNTER_GAUGE_INCREASE + 2.5f *
                _playerRegionDetector.LastRegion.Value.GetRelativeLevel(
                    GameManager.Instance.GameState.PlayerStats.Level.Value);
            // 영지 강화 효과 적용
            increase *= 1.0f - PersistentGameState.Instance.HouseState.GetUpgradeValue(UpgradeType.EncounterRate) / 100f;
            // 유물 효과 적용
            if (GameManager.Instance?.TryGetRelic<DecreaseEncounterIncrementEffect>(out Relic relic) ?? false)
            {
                increase *= 1.0f - (relic.Effect as DecreaseEncounterIncrementEffect).GetEncounterIncrementReductionPercentage() / 100f;
            }
            float newValue = EncounterGauge.Value + fixedDeltaTime * increase;
            EncounterGauge.Value = Mathf.Clamp(newValue, MIN_GAUGE, MAX_GAUGE);

            if (GameManager.Instance.IsMovingMap.Value) return;

            if (EncounterGauge.Value >= MIN_ENCOUNTER_THRESHOLD)
            {
                // ENCOUNTER_CHECK_INTERVAL 시간 간격이 지났는지 확인
                _lastEncounterCheckTime += fixedDeltaTime;
                if (_lastEncounterCheckTime >= ENCOUNTER_CHECK_INTERVAL)
                {
                    CheckForEncounter(EncounterGauge.Value);
                    _lastEncounterCheckTime = 0f;
                }
            }
        }

        public void ResetGauge()
        {
            EncounterGauge.Value = MIN_GAUGE;
        }

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        private void CheckForEncounter(float gauge)
        {
            // 게이지에 따른 엔카운터 확률 계산 (지수 함수 사용)
            float encounterChance = CalculateEncounterChance(gauge);
            
            // 랜덤 체크
            if (UnityEngine.Random.value < encounterChance)
            {
                Debug.Log($"[Encounter] Encountered with chance {encounterChance} (Gauge: {gauge})");
                TriggerEncounter();
            }
        }

        private float CalculateEncounterChance(float gauge)
        {
            // 게이지가 최소 임계값보다 낮으면 0% 확률
            if (gauge < MIN_ENCOUNTER_THRESHOLD)
                return 0f;

            // 게이지가 최대 임계값이면 100% 확률
            if (gauge >= MAX_ENCOUNTER_THRESHOLD)
                return 1f;

            // 지수 함수를 사용하여 확률 계산
            // MIN_ENCOUNTER_THRESHOLD에서 0%, MAX_ENCOUNTER_THRESHOLD에서 100%가 되도록 조정
            float normalizedGauge = (gauge - MIN_ENCOUNTER_THRESHOLD) / (MAX_ENCOUNTER_THRESHOLD - MIN_ENCOUNTER_THRESHOLD);
            return Mathf.Pow(normalizedGauge, 3f); // 세제곱을 사용하여 더 급격하게 증가
        }

        public void TriggerEncounter()
        {
            if (GameManager.Instance.IsMovingMap.Value) return;
            
            // 마지막으로 있던 Region 정보 가져오기
            var lastRegion = _playerRegionDetector.LastRegion.Value;
            RegionEntry entry = lastRegion?.GetRegionEntry();

            if (entry == null)
            {
                Debug.LogError($"[Encounter] Region entry not found");
                return;
            }

            // 몬스터 선택
            var selectedMonster = SelectMonsterFromRegion(entry);
            if (selectedMonster == null)
            {
                Debug.LogError("[Encounter] Monster selection failed");
                return;
            }
            
            ResetGauge();
            
            // 전투 시작
            BattleManager.Instance.StartBattle(selectedMonster, _ => { });
        }

        private MonsterData SelectMonsterFromRegion(RegionEntry region)
        {
            if (region.monsters == null || region.monsters.Count == 0)
            {
                Debug.LogError($"[Encounter] No monsters found in region: Lv. {region.label}");
                return null;
            }

            // 확률에 따른 몬스터 선택
            float random = UnityEngine.Random.value;
            float total = region.monsters.Sum(monster => monster.spawnRate);
            float cumulativeProbability = 0f;

            foreach (var monsterInfo in region.monsters)
            {
                cumulativeProbability += monsterInfo.spawnRate / total;
                if (random <= cumulativeProbability)
                {
                    return TableManager.GetMonster(monsterInfo.monsterCode);
                }
            }

            // 기본값으로 첫 번째 몬스터 반환
            return TableManager.GetMonster(region.monsters[0].monsterCode);
        }

        public void Dispose()
        {
            EncounterGauge.Dispose();
        }
    }
}