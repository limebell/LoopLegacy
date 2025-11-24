using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using LoopLegacy.Manager;
using TMPro;

namespace LoopLegacy.UI.Controller
{
    public class CutsceneController : MonoBehaviour
    {
        private const float _delayBetweenCharacters = 0.05f;

        [Header("Cutscene Manager")]
        [SerializeField] private CutsceneManager _cutsceneManager;

        [Header("UI Elements")]
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _dialogueText;
        [SerializeField] private Button _skipButton;
        
        private Sequence _currentTextSequence;
        private bool _isTextPlaying = false;
        private string _currentFullText = "";
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnCutsceneClicked);
            _skipButton.onClick.AddListener(OnSkipButtonClicked);
        }

        private void OnCutsceneClicked()
        {
            _cutsceneManager.HandleClick();
        }

        public void SetImage(Sprite image)
        {
            _image.sprite = image;

            // imageElement의 크기를 조절하여 이미지 비율을 유지하도록 함
            /*var imageRatio = (float)image.texture.width / image.texture.height;
            var screenRatio = (float)Screen.width / Screen.height;
            
            imageElement.style.width = new Length(100, LengthUnit.Percent);
            
            if (imageRatio > screenRatio)
            {
                // 이미지가 더 넓은 경우, height를 화면에 맞춤
                imageElement.style.height = new Length(100f / imageRatio, LengthUnit.Percent);
            }
            else
            {
                // 이미지가 더 높은 경우, height를 화면 높이에 맞춤
                imageElement.style.height = new Length(100, LengthUnit.Percent);
            }*/
        }
        
        /// <summary>
        /// 텍스트를 타이핑 효과로 재생합니다.
        /// </summary>
        /// <param name="text">재생할 텍스트</param>
        public void PlayText(string text)
        {            
            // 이전 애니메이션 중지
            StopCurrentTextAnimation();
            
            // 텍스트 초기화 및 상태 설정
            _dialogueText.text = "";
            _currentFullText = text;
            _isTextPlaying = true;
            
            // DoTween을 사용한 타이핑 애니메이션
            _currentTextSequence = DOTween.Sequence();
            
            int i = 0;
            while (i < text.Length)
            {
                int index = i; // 클로저 문제 해결
                if (text[index] == '<')
                {
                    int tagEnd = text.IndexOf('>', index);
                    if (tagEnd != -1)
                    {
                        string tag = text.Substring(index, tagEnd - index + 1);
                        _currentTextSequence.AppendCallback(() => {
                            _dialogueText.text += tag;
                        }).AppendInterval(_delayBetweenCharacters);
                        i = tagEnd + 1;
                        continue;
                    }
                }
                // 일반 문자 처리
                _currentTextSequence.AppendCallback(() => {
                    _dialogueText.text += text[index];
                }).AppendInterval(_delayBetweenCharacters);
                i++;
            }
            
            // 애니메이션 완료 시 상태 업데이트
            _currentTextSequence.OnComplete(() => {
                _isTextPlaying = false;
                _currentTextSequence = null;
            });
        }
        
        /// <summary>
        /// 현재 텍스트를 즉시 완성합니다.
        /// </summary>
        public void CompleteCurrentText()
        {
            if (_isTextPlaying && _currentTextSequence != null && _currentTextSequence.IsActive())
            {
                _currentTextSequence.Complete();
                _dialogueText.text = _currentFullText;
                _isTextPlaying = false;
            }
        }
        
        /// <summary>
        /// 텍스트가 재생 중인지 확인합니다.
        /// </summary>
        /// <returns>텍스트가 재생 중이면 true</returns>
        public bool IsTextPlaying()
        {
            return _isTextPlaying;
        }
        
        /// <summary>
        /// 텍스트를 즉시 완성합니다.
        /// </summary>
        /// <param name="text">표시할 텍스트</param>
        public void SetTextImmediate(string text)
        {            
            StopCurrentTextAnimation();
            _dialogueText.text = text;
            _currentFullText = text;
            _isTextPlaying = false;
        }
        
        /// <summary>
        /// 현재 재생 중인 텍스트 애니메이션을 중지합니다.
        /// </summary>
        public void StopCurrentTextAnimation()
        {
            if (_currentTextSequence != null && _currentTextSequence.IsActive())
            {
                _currentTextSequence.Kill();
                _currentTextSequence = null;
            }
            _isTextPlaying = false;
        }
        
        private void OnSkipButtonClicked()
        {
            // 현재 텍스트 애니메이션을 즉시 완료
            if (_currentTextSequence != null && _currentTextSequence.IsActive())
            {
                _currentTextSequence.Complete();
            }
            
            // CutsceneManager에 스킵 알림
            if (_cutsceneManager != null)
            {
                _cutsceneManager.SkipCutscene();
            }
        }
        
        private void OnDestroy()
        {
            StopCurrentTextAnimation();
        }
    }
}
