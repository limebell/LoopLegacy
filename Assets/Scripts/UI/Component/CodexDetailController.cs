using System.Collections;
using System.Linq;
using LoopLegacy.Loader;
using LoopLegacy.State;
using LoopLegacy.UI.Component.CodexDetail;
using UnityEngine;
using UnityEngine.UI;

namespace LoopLegacy.UI.Component
{
    public class CodexDetailController : MonoBehaviour
    {
        [SerializeField] private Image _codexImage;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _content;
        [Header("Codex Details")]
        [SerializeField] private MonsterCodexDetail _monsterCodexDetail;
        [SerializeField] private EquipmentCodexDetail _equipmentCodexDetail;
        [SerializeField] private RelicCodexDetail _relicCodexDetail;

        private Coroutine _resetScrollCoroutine;

        void Start()
        {
            SetImage(null);
            _monsterCodexDetail.gameObject.SetActive(false);
            _equipmentCodexDetail.gameObject.SetActive(false);
            _relicCodexDetail.gameObject.SetActive(false);
        }

        void OnDisable()
        {
            // GameObject가 비활성화될 때 실행 중인 코루틴 정리
            if (_resetScrollCoroutine != null)
            {
                StopCoroutine(_resetScrollCoroutine);
                _resetScrollCoroutine = null;
            }
        }

        public void UpdateCodexDetail()
        {
            SetImage(null);
            _monsterCodexDetail.gameObject.SetActive(false);
            _equipmentCodexDetail.gameObject.SetActive(false);
            _relicCodexDetail.gameObject.SetActive(false);
        }

        public void UpdateCodexDetail(MonsterData monsterData)
        {
            _monsterCodexDetail.gameObject.SetActive(true);
            _equipmentCodexDetail.gameObject.SetActive(false);
            _relicCodexDetail.gameObject.SetActive(false);
            var killCount = PersistentGameState.Instance.CodexState.GetMobKillCount(monsterData.code);
            if (killCount == 0)
            {
                SetImage(null);
                _monsterCodexDetail.UpdateCodexDetail(null);
            }
            else
            {
                SetImage(monsterData.sprite);
                _monsterCodexDetail.UpdateCodexDetail(monsterData);
            }

            ResetScrollPosition();
        }

        public void UpdateCodexDetail(EquipmentData equipmentData)
        {
            _monsterCodexDetail.gameObject.SetActive(false);
            _equipmentCodexDetail.gameObject.SetActive(true);
            _relicCodexDetail.gameObject.SetActive(false);
            var count = PersistentGameState.Instance.InventoryState.GetOwnedEquipments(equipmentData.type)[equipmentData.id];
            if (count == 0)
            {
                SetImage(null);
                _equipmentCodexDetail.UpdateCodexDetail("???", "");
            }
            else
            {
                SetImage(equipmentData.sprite);
                string descriptionText = $"{equipmentData.baseValue} (+{equipmentData.multiplier}%)\n" +
                    $"{Utils.GetUIString("owned-count")}: {count}\n" +
                    $"{Utils.GetEquipmentDescription(equipmentData.type, equipmentData.id)}";
                _equipmentCodexDetail.UpdateCodexDetail(
                    Utils.GetEquipmentName(equipmentData.type, equipmentData.id),
                    descriptionText);
            }

            ResetScrollPosition();
        }

        public void UpdateCodexDetail(RelicData relicData)
        {
            _monsterCodexDetail.gameObject.SetActive(false);
            _equipmentCodexDetail.gameObject.SetActive(false);
            _relicCodexDetail.gameObject.SetActive(true);
            var relic = PersistentGameState.Instance.CodexState.GetRelic(relicData.id);
            if (relic == null)
            {
                SetImage(null);
                _relicCodexDetail.UpdateCodexDetail("???", "");
            }
            else
            {
                SetImage(relicData.sprite);
                string descriptionText = $"Lv. {relic.Level + 1}\n";
                descriptionText += $"<color={Utils.GetRelicGradeColorHex(relic.Grade)}>{Utils.GetUIString("grade_" + relic.Grade.ToString().ToLowerInvariant())}</color>";
                string[] args = new string[0];
                if (relicData.values.Length > 0)
                {
                    var value0 = relicData.values[0];
                    args = new string[value0.Split(':').Length];
                    for (int i = 0; i < args.Length; i++)
                    {
                        args[i] = string.Empty;
                    }
                    foreach (var (i, value) in relicData.values.Select((value, index) => (index, value)))
                    {
                        foreach (var (j, f) in value.Split(':').Select((value, index) => (index, float.Parse(value))))
                        {
                            if (i == relic.Level)
                            {
                                args[j] += $"{f}/";
                            }
                            else
                            {
                                args[j] += $"<color=#808080>{f}</color>/";
                            }
                        }
                    }
                    for (int i = 0; i < args.Length; i++)
                    {
                        args[i] = args[i].TrimEnd('/');
                    }
                    descriptionText += $"\n\n{Utils.GetRelicDescription(relicData.effectName, args)}";
                }
                _relicCodexDetail.UpdateCodexDetail(
                    Utils.GetRelicName(relicData.effectName),
                    descriptionText);
            }

            ResetScrollPosition();
        }

        private void SetImage(Sprite sprite)
        {
            _codexImage.sprite = sprite;
            _codexImage.color = new Color(1, 1, 1, sprite == null ? 0 : 1);
            if (sprite != null)
            {
                int preferredWidth = Mathf.Max((int)sprite.rect.width, 32);
                int preferredHeight = Mathf.Max((int)sprite.rect.height, 32);
                // sprite 비율에 맞게 조정
                if (sprite.rect.width > sprite.rect.height)
                {
                    _codexImage.rectTransform.localScale = new Vector3(1, sprite.rect.height / sprite.rect.width, 1) * (sprite.rect.width / preferredWidth);
                }
                else if (sprite.rect.width < sprite.rect.height)
                {
                    _codexImage.rectTransform.localScale = new Vector3(sprite.rect.width / sprite.rect.height, 1, 1) * (sprite.rect.height / preferredHeight);
                }
                else
                {
                    _codexImage.rectTransform.localScale = Vector3.one * (sprite.rect.width / preferredWidth);
                }
            }
        }

        private void ResetScrollPosition()
        {
            // GameObject가 활성화되어 있고 컴포넌트가 활성화되어 있을 때만 코루틴 시작
            if (!gameObject.activeInHierarchy || !enabled)
            {
                return;
            }

            if (_resetScrollCoroutine != null)
            {
                StopCoroutine(_resetScrollCoroutine);
            }
            _resetScrollCoroutine = StartCoroutine(ResetScrollPositionCoroutine());
        }

        private IEnumerator ResetScrollPositionCoroutine()
        {
            // 현재 프레임의 레이아웃 업데이트를 완료하기 위해 프레임 끝까지 대기
            yield return new WaitForEndOfFrame();
            
            // GameObject가 여전히 활성화되어 있는지 확인
            if (!gameObject.activeInHierarchy || _content == null || _scrollRect == null)
            {
                yield break;
            }
            
            // 레이아웃 강제 재계산 (null 체크)
            Canvas.ForceUpdateCanvases();
            
            if (_content != null && _content.gameObject.activeInHierarchy)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
            }
            
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null && rectTransform.gameObject.activeInHierarchy)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
            
            Canvas.ForceUpdateCanvases();
            
            // 한 프레임 더 대기하여 레이아웃이 완전히 적용되도록 함
            yield return null;
            
            // 다시 한번 null 체크
            if (!gameObject.activeInHierarchy || _scrollRect == null)
            {
                yield break;
            }
            
            // 스크롤 위치를 맨 위로 리셋
            _scrollRect.verticalNormalizedPosition = 1;
            
            // 최종 레이아웃 업데이트
            Canvas.ForceUpdateCanvases();
        }
    }
}
