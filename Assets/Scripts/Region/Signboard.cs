using DG.Tweening;
using TMPro;
using UnityEngine;

namespace LoopLegacy.Region
{
    public class Signboard : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _sprite; // NPC 스프라이트
        [SerializeField] private TextMeshPro _signboardText; // 표지판 텍스트
        [SerializeField] private SpriteRenderer _signboardSprite; // 표지판 스프라이트
        public string DisplayText { get; set; }
        private MaterialPropertyBlock _mpb;
        private Sequence _currentTextSequence;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _mpb = new MaterialPropertyBlock();
            HideSignboard();
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.CompareTag("Player")) return;

            SetOutline(true, 1f);
            ShowSignboard();
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.gameObject.CompareTag("Player")) return;

            SetOutline(false, 1f);
            HideSignboard();
        }

        void OnDestroy()
        {
            _currentTextSequence?.Kill();
            _currentTextSequence = null;
        }

        public void SetOutline(bool on, float thickness = 1f)
        {
            // 현재 값 읽어오기
            _sprite.GetPropertyBlock(_mpb);

            // 속성 덮어쓰기
            _mpb.SetFloat("_OutlineThickness", on ? thickness : 0f);
            
            _mpb.SetColor("_OutlineColor", new Color(0.9960938f, 0.9042969f, 0.6806641f, 1f));

            // Renderer에 적용
            _sprite.SetPropertyBlock(_mpb);
        }

        private void ShowSignboard()
        {
            _signboardText.gameObject.SetActive(true);
            _signboardSprite.gameObject.SetActive(true);
            const float delayBetweenCharacters = 0.05f;

            // 이전 애니메이션 중지
            if (_currentTextSequence != null && _currentTextSequence.IsActive())
            {
                _currentTextSequence.Kill();
                _currentTextSequence = null;
            }
            
            // 텍스트 초기화 및 상태 설정
            SetSignboardText("");
            var currentFullText = DisplayText;
            
            // DoTween을 사용한 타이핑 애니메이션
            _currentTextSequence = DOTween.Sequence();
            
            int i = 0;
            while (i < currentFullText.Length)
            {
                int index = i; // 클로저 문제 해결
                if (currentFullText[index] == '<')
                {
                    int tagEnd = currentFullText.IndexOf('>', index);
                    if (tagEnd != -1)
                    {
                        string tag = currentFullText.Substring(index, tagEnd - index + 1);
                        _currentTextSequence.AppendCallback(() => {
                            SetSignboardText(_signboardText.text + tag);
                        }).AppendInterval(delayBetweenCharacters);
                        i = tagEnd + 1;
                        continue;
                    }
                }
                // 일반 문자 처리
                _currentTextSequence.AppendCallback(() => {
                    SetSignboardText(_signboardText.text + currentFullText[index]);
                }).AppendInterval(delayBetweenCharacters);
                i++;
            }
            
            // 애니메이션 완료 시 상태 업데이트
            _currentTextSequence.OnComplete(() => {
                _currentTextSequence = null;
            });
        }

        private void SetSignboardText(string text)
        {
            _signboardText.text = text;
            _signboardSprite.size = new Vector2(_signboardText.preferredWidth * 0.018f, _signboardSprite.size.y);
        }

        private void HideSignboard()
        {
            _signboardText.gameObject.SetActive(false);
            _signboardSprite.gameObject.SetActive(false);
        }
    }
}
