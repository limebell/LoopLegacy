using LoopLegacy.Battle;
using LoopLegacy.Loader;
using LoopLegacy.Manager;
using LoopLegacy.State;
using LoopLegacy.UI.Component;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoopLegacy.UI.Controller
{
    public class BattleResultController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _resultLabel;
        [SerializeField] private TMP_Text _battlePointLabel;
        [SerializeField] private TMP_Text _levelLabel;
        [SerializeField] private TMP_Text _goldLabel;
        [SerializeField] private GameObject _dropContainer;
        [SerializeField] private DropElement[] _drops;
        [SerializeField] private GameObject _dropDialogue;
        [SerializeField] private TMP_Text _dropDialogueText;
        [SerializeField] private AudioClip _victoryBGM;
        [SerializeField] private AudioClip _displaySFX;
        [SerializeField] private GameObject _infoText;
        
        private const float RESULT_ANIMATION_DELAY = 0.5f; // 각 요소 표시 간격 (초)
        private bool _skipResultAnimation = false; // 애니메이션 스킵 플래그
        private bool _isAnimatingResult = false; // 결과 애니메이션 진행 중 여부

        void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnClickBattleResult);
        }

        private void OnClickBattleResult()
        {
            // 애니메이션 중이면 스킵
            if (_isAnimatingResult)
            {
                _skipResultAnimation = true;
            }
            else
            {
                // 애니메이션이 끝났으면 다음 단계로
                BattleManager.Instance.OnClickBattleUI();
            }
        }

        public void ShowBattleResult(BattleContext result, LevelUpResult? levelUpResult = null)
        {
            gameObject.SetActive(true);
            AudioManager.Instance.StopBGM();
            
            // 순차적으로 표시하기 위해 코루틴 시작
            StartCoroutine(ShowBattleResultAnimationCoroutine(result, levelUpResult));
        }

        /// <summary>
        /// Drop 요소 설정 (중복 코드 제거용 헬퍼)
        /// </summary>
        private void SetupDropElement(int index, DropEntry drop, bool playSound = true)
        {
            if (drop.itemType == DropType.Relic)
            {
                var relicData = TableManager.GetRelic(drop.relicEffectName);
                _drops[index].UpdateElement(
                    image: relicData.sprite,
                    type: "UNLOCK",
                    isNew: true,
                    count: 1,
                    onClick: () => ConfirmationController.Instance.ShowRelic(new Relic(relicData, drop.relicLevel)));
            }
            else
            {
                var equipmentData = TableManager.GetEquipment(drop.itemType switch
                {
                    DropType.Weapon => EquipmentType.Weapon,
                    DropType.Armor => EquipmentType.Armor,
                    _ => EquipmentType.Weapon,
                }, drop.itemId);
                var isNew = PersistentGameState.Instance.InventoryState.GetOwnedEquipments(equipmentData.type)[equipmentData.id] == 0;
                _drops[index].UpdateElement(
                    image: equipmentData.sprite,
                    type: "DROP",
                    isNew: isNew,
                    count: 1,
                    onClick: () => ConfirmationController.Instance.ShowEquipment(equipmentData));
            }
            
            _drops[index].gameObject.SetActive(true);
            
            if (playSound)
            {
                PlayDisplaySFX();
            }
        }
        
        /// <summary>
        /// 요소 표시 효과음 재생
        /// </summary>
        private void PlayDisplaySFX()
        {
            if (_displaySFX != null)
            {
                AudioManager.Instance.PlaySFXSound(_displaySFX);
            }
        }
        
        private string GetDropDialogueText(DropEntry drop)
        {
            if (drop.itemType == DropType.Relic)
            {
                var relicName = $"{Utils.GetRelicName(drop.relicEffectName)} Lv. {drop.relicLevel + 1}";
                return Utils.GetUIString("drop-dialogue_relic", new object[] { relicName });
            }
            else
            {
                var equipmentName = Utils.GetEquipmentName(TableManager.GetEquipment(drop.itemType switch
                {
                    DropType.Weapon => EquipmentType.Weapon,
                    DropType.Armor => EquipmentType.Armor,
                    _ => EquipmentType.Weapon,
                }, drop.itemId).type, drop.itemId);
                return Utils.GetUIString(
                    drop.itemType == DropType.Weapon ? "drop-dialogue_weapon" : "drop-dialogue_armor",
                    new object[] { equipmentName });
            }
        }
        
        /// <summary>
        /// 스킵 가능한 대기 (매 프레임마다 스킵 체크)
        /// </summary>
        private IEnumerator WaitForSecondsSkippable(float seconds)
        {
            float elapsed = 0f;
            while (elapsed < seconds && !_skipResultAnimation)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        
        /// <summary>
        /// 전투 결과를 순차적으로 표시하는 애니메이션
        /// </summary>
        private IEnumerator ShowBattleResultAnimationCoroutine(BattleContext result, LevelUpResult? levelUpResult)
        {
            _isAnimatingResult = true;
            _skipResultAnimation = false;
            bool hasNewDrop = false; // 새 드랍이 있는지 체크
            
            var currentBattlePoint = GameManager.Instance.GameState.PlayerStats.BattlePoint.Value;
            _dropContainer.SetActive(result.IsVictory);
            
            // 모든 요소 초기에 숨김
            _battlePointLabel.gameObject.SetActive(false);
            _goldLabel.gameObject.SetActive(false);
            _levelLabel.gameObject.SetActive(false);
            _dropDialogue.SetActive(false);
            _infoText.SetActive(false);
            foreach (var drop in _drops)
            {
                drop.gameObject.SetActive(false);
            }
            
            if (result.IsVictory)
            {
                _resultLabel.text = Utils.GetUIString("victory");

                // 승리 BGM 재생 (루프 없음)
                AudioManager.Instance.PlayBGM(_victoryBGM, false);
                
                // 1. BattlePoint 표시
                var battlePointAfter = Mathf.Max(0, currentBattlePoint + result.EarnedBattlePoint);
                var color = battlePointAfter < currentBattlePoint ? "red" : "green";
                _battlePointLabel.text = $"{currentBattlePoint}  >  <color={color}>{battlePointAfter}</color>";
                _battlePointLabel.gameObject.SetActive(true);
                PlayDisplaySFX();
                
                yield return WaitForSecondsSkippable(RESULT_ANIMATION_DELAY);
                
                // 2. Gold 표시
                _goldLabel.text = $"{PersistentGameState.Instance.Gold.Value:n0}G  <color=green>+{result.EarnedGold:n0}G</color>";
                _goldLabel.gameObject.SetActive(true);
                PlayDisplaySFX();
                
                yield return WaitForSecondsSkippable(RESULT_ANIMATION_DELAY);
                
                // 3. Level 표시
                var currentLevel = GameManager.Instance.GameState.PlayerStats.Level.Value;
                int expectedLevel;
                
                if (levelUpResult.HasValue)
                {
                    expectedLevel = levelUpResult.Value.FinalLevel;
                }
                else
                {
                    expectedLevel = GameManager.Instance.GameState.PlayerStats.ExpectedLevel(result.EarnedEXP);
                }
                
                _levelLabel.text = $"{currentLevel}  >  <color=green>{expectedLevel}</color>";
                _levelLabel.gameObject.SetActive(true);
                PlayDisplaySFX();
                
                // 4. Drop이 있을 때만 대기 후 표시
                if (result.DroppedItems.Count > 0)
                {
                    yield return WaitForSecondsSkippable(RESULT_ANIMATION_DELAY);
                    
                    // Drop 다이얼로그 표시 (빈 상태로 먼저 표시)
                    _dropDialogueText.text = "";
                    _dropDialogue.SetActive(true);
                    
                    // 5. Drop 아이템 하나씩 표시 (텍스트도 함께 추가)
                    for (int i = 0; i < result.DroppedItems.Count && i < _drops.Length; i++)
                    {
                        var drop = result.DroppedItems[i];
                        
                        // 새 드랍인지 체크
                        if (drop.itemType == DropType.Relic)
                        {
                            hasNewDrop = true; // 유물은 항상 새 드랍
                        }
                        else
                        {
                            var equipmentData = TableManager.GetEquipment(drop.itemType switch
                            {
                                DropType.Weapon => EquipmentType.Weapon,
                                DropType.Armor => EquipmentType.Armor,
                                _ => EquipmentType.Weapon,
                            }, drop.itemId);
                            var isNew = PersistentGameState.Instance.InventoryState.GetOwnedEquipments(equipmentData.type)[equipmentData.id] == 0;
                            if (isNew)
                            {
                                hasNewDrop = true;
                            }
                        }
                        
                        // 스킵되었다면 모든 아이템 즉시 표시 (소리 없이)
                        if (_skipResultAnimation)
                        {
                            // 남은 모든 아이템과 텍스트를 한꺼번에 추가
                            System.Text.StringBuilder allText = new System.Text.StringBuilder(_dropDialogueText.text);
                            for (int j = i; j < result.DroppedItems.Count && j < _drops.Length; j++)
                            {
                                if (allText.Length > 0)
                                    allText.Append("\n");
                                allText.Append(GetDropDialogueText(result.DroppedItems[j]));
                                SetupDropElement(j, result.DroppedItems[j], playSound: false);
                            }
                            _dropDialogueText.text = allText.ToString();
                            break;
                        }
                        
                        // 텍스트 추가
                        if (_dropDialogueText.text.Length > 0)
                            _dropDialogueText.text += "\n";
                        _dropDialogueText.text += GetDropDialogueText(result.DroppedItems[i]);
                        
                        // 아이템 표시
                        SetupDropElement(i, result.DroppedItems[i], playSound: true);
                        
                        if (i < result.DroppedItems.Count - 1)
                            yield return WaitForSecondsSkippable(RESULT_ANIMATION_DELAY);
                    }
                }
                
                // 승리 시 마지막에 infoText 표시 (소리 없이)
                if (!_skipResultAnimation)
                {
                    // Drop이 있었다면 대기, 없었다면 대기 없음
                    yield return WaitForSecondsSkippable(RESULT_ANIMATION_DELAY);
                    
                    // 새 드랍이 있었다면 추가로 0.6초 대기
                    if (hasNewDrop)
                    {
                        yield return new WaitForSeconds(0.6f);
                    }
                    
                    _infoText.SetActive(true);
                }
            }
            else
            {
                _resultLabel.text = Utils.GetUIString("defeat");
                
                // 패배 시에도 순차적으로 표시
                // 1. BattlePoint 표시
                var battlePointAfter = Mathf.Max(0, currentBattlePoint + result.EarnedBattlePoint);
                var color = battlePointAfter < currentBattlePoint ? "red" : "green";
                _battlePointLabel.text = $"{currentBattlePoint}  >  <color={color}>{battlePointAfter}</color>";
                _battlePointLabel.gameObject.SetActive(true);
                PlayDisplaySFX();
                
                yield return WaitForSecondsSkippable(RESULT_ANIMATION_DELAY);
                
                // 2. Gold 표시 (패배 시 "-")
                _goldLabel.text = "-";
                _goldLabel.gameObject.SetActive(true);
                PlayDisplaySFX();
                
                yield return WaitForSecondsSkippable(RESULT_ANIMATION_DELAY);
                
                // 3. Level 표시 (패배 시 "-")
                _levelLabel.text = "-";
                _levelLabel.gameObject.SetActive(true);
                PlayDisplaySFX();
                
                // 패배 시 마지막에 infoText 표시 (소리 없이)
                if (!_skipResultAnimation)
                {
                    yield return WaitForSecondsSkippable(RESULT_ANIMATION_DELAY);
                    _infoText.SetActive(true);
                }
            }
            
            // 스킵되었을 경우 남은 요소들 즉시 표시 (소리 없이)
            if (_skipResultAnimation)
            {
                _battlePointLabel.gameObject.SetActive(true);
                _goldLabel.gameObject.SetActive(true);
                _levelLabel.gameObject.SetActive(true);
                
                if (result.IsVictory && result.DroppedItems.Count > 0)
                {
                    _dropDialogue.SetActive(true);
                    
                    // 아직 추가되지 않은 텍스트 완성
                    System.Text.StringBuilder completeText = new System.Text.StringBuilder(_dropDialogueText.text);
                    for (int i = 0; i < result.DroppedItems.Count && i < _drops.Length; i++)
                    {
                        if (!_drops[i].gameObject.activeSelf)
                        {
                            if (completeText.Length > 0)
                                completeText.Append("\n");
                            completeText.Append(GetDropDialogueText(result.DroppedItems[i]));
                            SetupDropElement(i, result.DroppedItems[i], playSound: false);
                        }
                    }
                    _dropDialogueText.text = completeText.ToString();
                }
                
                // 새 드랍이 있으면 스킵했어도 0.6초 대기 (스킵 불가)
                if (hasNewDrop)
                {
                    yield return new WaitForSeconds(0.6f);
                }
                
                // 스킵 시 infoText도 소리 없이 표시
                _infoText.SetActive(true);
            }
            
            _isAnimatingResult = false;
        }

        public void HideBattleResult()
        {
            // 애니메이션 중단
            _isAnimatingResult = false;
            _skipResultAnimation = false;
            StopAllCoroutines();
            
            // 모든 UI 요소 다시 표시 (다음 전투를 위해)
            _battlePointLabel.gameObject.SetActive(true);
            _goldLabel.gameObject.SetActive(true);
            _levelLabel.gameObject.SetActive(true);
            _infoText.SetActive(false); // 다음 전투를 위해 숨김
            
            gameObject.SetActive(false);
            AudioManager.Instance.StopBGM();
            AudioManager.Instance.RestoreBGMState(); // 맵 배경음악 복원
        }
    }
}