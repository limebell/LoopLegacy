using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace LoopLegacy.UI.Component
{
    /// <summary>
    /// 특정 영역을 강조하기 위해 주변 영역을 어둡게 처리하는 오버레이 컴포넌트
    /// 4개의 Image(위, 아래, 왼쪽, 오른쪽)를 사용하여 특정 영역 주변만 어둡게 만듭니다.
    /// </summary>
    public class SpotlightOverlay : MonoBehaviour
    {
        private Image _topDimmed;
        private Image _bottomDimmed;
        private Image _leftDimmed;
        private Image _rightDimmed;
        [SerializeField] private Color _dimmedColor = new Color(0, 0, 0, 0.7f);
        
        // 외곽선
        private Image _topOutline;
        private Image _bottomOutline;
        private Image _leftOutline;
        private Image _rightOutline;
        [SerializeField] private Color _outlineColor = new Color(1, 1, 1, 1f); // 밝은 흰색
        [SerializeField] private float _outlineThickness = 3f;
        [SerializeField] private float _outlineThicknessVariation = 2f; // 두께 변화량
        
        private Sequence _outlineAnimation;
        private Vector2 _currentMin;
        private Vector2 _currentMax;
        private float _currentOutlineThickness;

        private RectTransform _rectTransform;
        private Canvas _canvas;
        private RectTransform _canvasRect;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            
            if (_canvas != null)
            {
                _canvasRect = _canvas.GetComponent<RectTransform>();
            }
            
            CreateDimmedOverlays();
            CreateOutlines();
            
            // 초기 상태는 숨김
            Hide();
        }

        /// <summary>
        /// 어두운 오버레이들을 자동으로 생성
        /// </summary>
        private void CreateDimmedOverlays()
        {
            // 위쪽 어두운 영역
            _topDimmed = CreateDimmedImage("TopDimmed");
            
            // 아래쪽 어두운 영역
            _bottomDimmed = CreateDimmedImage("BottomDimmed");
            
            // 왼쪽 어두운 영역
            _leftDimmed = CreateDimmedImage("LeftDimmed");
            
            // 오른쪽 어두운 영역
            _rightDimmed = CreateDimmedImage("RightDimmed");
        }
        
        /// <summary>
        /// 외곽선들을 자동으로 생성
        /// </summary>
        private void CreateOutlines()
        {
            // 위쪽 외곽선
            _topOutline = CreateOutlineImage("TopOutline");
            
            // 아래쪽 외곽선
            _bottomOutline = CreateOutlineImage("BottomOutline");
            
            // 왼쪽 외곽선
            _leftOutline = CreateOutlineImage("LeftOutline");
            
            // 오른쪽 외곽선
            _rightOutline = CreateOutlineImage("RightOutline");
        }
        
        private Image CreateOutlineImage(string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(transform, false);
            
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            
            Image image = obj.AddComponent<Image>();
            image.color = _outlineColor;
            image.raycastTarget = false;
            
            return image;
        }

        private Image CreateDimmedImage(string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(transform, false);
            
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            
            Image image = obj.AddComponent<Image>();
            image.color = _dimmedColor;
            image.raycastTarget = true; // 가림막이 클릭을 막도록 설정
            
            return image;
        }

        /// <summary>
        /// 강조할 영역을 설정합니다.
        /// 다른 Canvas에 있는 RectTransform도 강조할 수 있습니다.
        /// </summary>
        /// <param name="targetRect">강조할 RectTransform 영역</param>
        /// <param name="margin">강조 영역 주변에 추가할 여백 (모든 방향에 동일하게 적용)</param>
        public void SetHighlightArea(RectTransform targetRect, float margin = 0f)
        {
            if (targetRect == null || _rectTransform == null)
                return;

            // targetRect가 속한 Canvas 찾기
            Canvas targetCanvas = targetRect.GetComponentInParent<Canvas>();
            Camera targetCamera = targetCanvas != null ? targetCanvas.worldCamera : null;
            
            // RectTransform의 월드 좌표를 스크린 좌표로 변환
            Vector3[] worldCorners = new Vector3[4];
            targetRect.GetWorldCorners(worldCorners);
            
            // 스크린 좌표로 변환 (targetRect의 Canvas Camera 사용)
            Vector2[] screenCorners = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                if (targetCamera != null)
                {
                    screenCorners[i] = RectTransformUtility.WorldToScreenPoint(targetCamera, worldCorners[i]);
                }
                else
                {
                    // Screen Space - Overlay인 경우
                    screenCorners[i] = RectTransformUtility.WorldToScreenPoint(null, worldCorners[i]);
                }
            }
            
            // 스크린 좌표를 SpotlightOverlay 자신의 RectTransform 로컬 좌표로 변환
            // (배너 광고 등으로 Canvas 자식들의 offset이 변경되어도 정확한 위치 계산 가능)
            Vector2[] localCorners = new Vector2[4];
            Camera overlayCamera = _canvas != null ? _canvas.worldCamera : null;
            for (int i = 0; i < 4; i++)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform,
                    screenCorners[i],
                    overlayCamera,
                    out localCorners[i]);
            }
            
            // margin 적용 (모든 방향에 동일하게)
            Vector2 marginVector = new Vector2(margin, margin);
            Vector2 min = localCorners[0] - marginVector;
            Vector2 max = localCorners[2] + marginVector;
            
            UpdateOverlay(min, max);
            _currentMin = min;
            _currentMax = max;
            UpdateOutlines(min, max);
        }

        /// <summary>
        /// 강조할 영역을 스크린 좌표로 설정합니다.
        /// </summary>
        /// <param name="screenPosition">강조할 영역의 중심 스크린 좌표</param>
        /// <param name="size">강조할 영역의 크기</param>
        public void SetHighlightAreaScreen(Vector2 screenPosition, Vector2 size)
        {
            if (_rectTransform == null)
                return;

            // 스크린 좌표를 SpotlightOverlay 자신의 RectTransform 로컬 좌표로 변환
            Vector2 localPosition;
            Camera overlayCamera = _canvas != null ? _canvas.worldCamera : Camera.main;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform,
                screenPosition,
                overlayCamera,
                out localPosition);

            Vector2 min = localPosition - size * 0.5f;
            Vector2 max = localPosition + size * 0.5f;
            
            UpdateOverlay(min, max);
            _currentMin = min;
            _currentMax = max;
            UpdateOutlines(min, max);
        }

        /// <summary>
        /// 오버레이를 업데이트합니다.
        /// </summary>
        private void UpdateOverlay(Vector2 min, Vector2 max)
        {
            if (_rectTransform == null)
                return;

            float canvasWidth = _rectTransform.rect.width;
            float canvasHeight = _rectTransform.rect.height;
            
            // Canvas의 중심이 (0, 0)이므로 좌표 변환
            float canvasLeft = -canvasWidth * 0.5f;
            float canvasRight = canvasWidth * 0.5f;
            float canvasBottom = -canvasHeight * 0.5f;
            float canvasTop = canvasHeight * 0.5f;
            
            // 위쪽 어두운 영역 (전체 너비, max.y부터 canvasTop까지)
            if (_topDimmed != null)
            {
                _topDimmed.rectTransform.anchorMin = new Vector2(0, (max.y - canvasBottom) / canvasHeight);
                _topDimmed.rectTransform.anchorMax = new Vector2(1, 1);
                _topDimmed.rectTransform.offsetMin = Vector2.zero;
                _topDimmed.rectTransform.offsetMax = Vector2.zero;
            }
            
            // 아래쪽 어두운 영역 (전체 너비, canvasBottom부터 min.y까지)
            if (_bottomDimmed != null)
            {
                _bottomDimmed.rectTransform.anchorMin = new Vector2(0, 0);
                _bottomDimmed.rectTransform.anchorMax = new Vector2(1, (min.y - canvasBottom) / canvasHeight);
                _bottomDimmed.rectTransform.offsetMin = Vector2.zero;
                _bottomDimmed.rectTransform.offsetMax = Vector2.zero;
            }
            
            // 왼쪽 어두운 영역 (canvasLeft부터 min.x까지, min.y부터 max.y까지)
            if (_leftDimmed != null)
            {
                float leftAnchorMin = (min.x - canvasLeft) / canvasWidth;
                float bottomAnchorMin = (min.y - canvasBottom) / canvasHeight;
                float topAnchorMax = (max.y - canvasBottom) / canvasHeight;
                
                _leftDimmed.rectTransform.anchorMin = new Vector2(0, bottomAnchorMin);
                _leftDimmed.rectTransform.anchorMax = new Vector2(leftAnchorMin, topAnchorMax);
                _leftDimmed.rectTransform.offsetMin = Vector2.zero;
                _leftDimmed.rectTransform.offsetMax = Vector2.zero;
            }
            
            // 오른쪽 어두운 영역 (max.x부터 canvasRight까지, min.y부터 max.y까지)
            if (_rightDimmed != null)
            {
                float rightAnchorMin = (max.x - canvasLeft) / canvasWidth;
                float bottomAnchorMin = (min.y - canvasBottom) / canvasHeight;
                float topAnchorMax = (max.y - canvasBottom) / canvasHeight;
                
                _rightDimmed.rectTransform.anchorMin = new Vector2(rightAnchorMin, bottomAnchorMin);
                _rightDimmed.rectTransform.anchorMax = new Vector2(1, topAnchorMax);
                _rightDimmed.rectTransform.offsetMin = Vector2.zero;
                _rightDimmed.rectTransform.offsetMax = Vector2.zero;
            }
        }
        
        /// <summary>
        /// 외곽선을 업데이트합니다.
        /// </summary>
        private void UpdateOutlines(Vector2 min, Vector2 max)
        {
            if (_rectTransform == null)
                return;

            _currentMin = min;
            _currentMax = max;
            _currentOutlineThickness = _outlineThickness;
            
            UpdateOutlineThickness(_outlineThickness);
            
            // 외곽선 애니메이션 시작
            StartOutlineAnimation();
        }
        
        /// <summary>
        /// 외곽선 두께를 업데이트합니다.
        /// </summary>
        private void UpdateOutlineThickness(float thickness)
        {
            if (_rectTransform == null)
                return;

            float canvasWidth = _rectTransform.rect.width;
            float canvasHeight = _rectTransform.rect.height;
            
            // Canvas의 중심이 (0, 0)이므로 좌표 변환
            float canvasLeft = -canvasWidth * 0.5f;
            float canvasRight = canvasWidth * 0.5f;
            float canvasBottom = -canvasHeight * 0.5f;
            float canvasTop = canvasHeight * 0.5f;
            
            Vector2 min = _currentMin;
            Vector2 max = _currentMax;
            
            // 위쪽 외곽선
            if (_topOutline != null)
            {
                _topOutline.rectTransform.anchorMin = new Vector2((min.x - canvasLeft) / canvasWidth, (max.y - canvasBottom) / canvasHeight);
                _topOutline.rectTransform.anchorMax = new Vector2((max.x - canvasLeft) / canvasWidth, (max.y - canvasBottom) / canvasHeight);
                _topOutline.rectTransform.offsetMin = new Vector2(0, 0);
                _topOutline.rectTransform.offsetMax = new Vector2(0, thickness);
            }
            
            // 아래쪽 외곽선
            if (_bottomOutline != null)
            {
                _bottomOutline.rectTransform.anchorMin = new Vector2((min.x - canvasLeft) / canvasWidth, (min.y - canvasBottom) / canvasHeight);
                _bottomOutline.rectTransform.anchorMax = new Vector2((max.x - canvasLeft) / canvasWidth, (min.y - canvasBottom) / canvasHeight);
                _bottomOutline.rectTransform.offsetMin = new Vector2(0, -thickness);
                _bottomOutline.rectTransform.offsetMax = new Vector2(0, 0);
            }
            
            // 왼쪽 외곽선
            if (_leftOutline != null)
            {
                _leftOutline.rectTransform.anchorMin = new Vector2((min.x - canvasLeft) / canvasWidth, (min.y - canvasBottom) / canvasHeight);
                _leftOutline.rectTransform.anchorMax = new Vector2((min.x - canvasLeft) / canvasWidth, (max.y - canvasBottom) / canvasHeight);
                _leftOutline.rectTransform.offsetMin = new Vector2(-thickness, 0);
                _leftOutline.rectTransform.offsetMax = new Vector2(0, 0);
            }
            
            // 오른쪽 외곽선
            if (_rightOutline != null)
            {
                _rightOutline.rectTransform.anchorMin = new Vector2((max.x - canvasLeft) / canvasWidth, (min.y - canvasBottom) / canvasHeight);
                _rightOutline.rectTransform.anchorMax = new Vector2((max.x - canvasLeft) / canvasWidth, (max.y - canvasBottom) / canvasHeight);
                _rightOutline.rectTransform.offsetMin = new Vector2(0, 0);
                _rightOutline.rectTransform.offsetMax = new Vector2(thickness, 0);
            }
        }
        
        /// <summary>
        /// 외곽선 애니메이션을 시작합니다.
        /// </summary>
        private void StartOutlineAnimation()
        {
            // 기존 애니메이션 중지
            StopOutlineAnimation();
            
            if (_topOutline == null || _bottomOutline == null || _leftOutline == null || _rightOutline == null)
                return;
            
            // 펄스 애니메이션 (색상 알파값 변화 + 선 굵기 변화)
            _outlineAnimation = DOTween.Sequence();
            
            // 모든 외곽선에 동시에 애니메이션 적용
            Image[] outlines = { _topOutline, _bottomOutline, _leftOutline, _rightOutline };
            
            // 색상 알파값 애니메이션
            foreach (var outline in outlines)
            {
                if (outline != null)
                {
                    Color originalColor = _outlineColor;
                    _outlineAnimation.Join(
                        outline.DOColor(new Color(originalColor.r, originalColor.g, originalColor.b, 0.7f), 0.8f)
                            .SetEase(Ease.InOutSine)
                    );
                }
            }
            
            // 선 굵기 애니메이션
            float minThickness = _outlineThickness;
            float maxThickness = _outlineThickness + _outlineThicknessVariation;
            
            _outlineAnimation.Join(
                DOTween.To(
                    () => _currentOutlineThickness,
                    thickness => {
                        _currentOutlineThickness = thickness;
                        UpdateOutlineThickness(thickness);
                    },
                    maxThickness,
                    0.8f
                ).SetEase(Ease.InOutSine)
            );
            
            _outlineAnimation.SetLoops(-1, LoopType.Yoyo);
        }
        
        /// <summary>
        /// 외곽선 애니메이션을 중지합니다.
        /// </summary>
        private void StopOutlineAnimation()
        {
            if (_outlineAnimation != null && _outlineAnimation.IsActive())
            {
                _outlineAnimation.Kill();
                _outlineAnimation = null;
            }
            
            // 외곽선 색상과 두께를 원래대로 복원
            if (_topOutline != null) _topOutline.color = _outlineColor;
            if (_bottomOutline != null) _bottomOutline.color = _outlineColor;
            if (_leftOutline != null) _leftOutline.color = _outlineColor;
            if (_rightOutline != null) _rightOutline.color = _outlineColor;
            
            UpdateOutlineThickness(_outlineThickness);
        }

        /// <summary>
        /// 오버레이를 표시합니다.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            // 모든 Image 활성화
            if (_topDimmed != null) _topDimmed.gameObject.SetActive(true);
            if (_bottomDimmed != null) _bottomDimmed.gameObject.SetActive(true);
            if (_leftDimmed != null) _leftDimmed.gameObject.SetActive(true);
            if (_rightDimmed != null) _rightDimmed.gameObject.SetActive(true);
            if (_topOutline != null) _topOutline.gameObject.SetActive(true);
            if (_bottomOutline != null) _bottomOutline.gameObject.SetActive(true);
            if (_leftOutline != null) _leftOutline.gameObject.SetActive(true);
            if (_rightOutline != null) _rightOutline.gameObject.SetActive(true);
        }

        /// <summary>
        /// 오버레이를 숨깁니다.
        /// GameObject는 활성화 상태를 유지하여 GameObject.Find로 찾을 수 있도록 합니다.
        /// </summary>
        public void Hide()
        {
            StopOutlineAnimation();
            // 자식 Image들만 비활성화 (GameObject는 활성화 상태 유지)
            if (_topDimmed != null) _topDimmed.gameObject.SetActive(false);
            if (_bottomDimmed != null) _bottomDimmed.gameObject.SetActive(false);
            if (_leftDimmed != null) _leftDimmed.gameObject.SetActive(false);
            if (_rightDimmed != null) _rightDimmed.gameObject.SetActive(false);
            if (_topOutline != null) _topOutline.gameObject.SetActive(false);
            if (_bottomOutline != null) _bottomOutline.gameObject.SetActive(false);
            if (_leftOutline != null) _leftOutline.gameObject.SetActive(false);
            if (_rightOutline != null) _rightOutline.gameObject.SetActive(false);
        }

        /// <summary>
        /// 어두운 색상을 설정합니다.
        /// </summary>
        public void SetDimmedColor(Color color)
        {
            _dimmedColor = color;
            if (_topDimmed != null) _topDimmed.color = color;
            if (_bottomDimmed != null) _bottomDimmed.color = color;
            if (_leftDimmed != null) _leftDimmed.color = color;
            if (_rightDimmed != null) _rightDimmed.color = color;
        }
        
        /// <summary>
        /// 외곽선 색상을 설정합니다.
        /// </summary>
        public void SetOutlineColor(Color color)
        {
            _outlineColor = color;
            if (_topOutline != null) _topOutline.color = color;
            if (_bottomOutline != null) _bottomOutline.color = color;
            if (_leftOutline != null) _leftOutline.color = color;
            if (_rightOutline != null) _rightOutline.color = color;
        }
        
        /// <summary>
        /// 외곽선 두께를 설정합니다.
        /// </summary>
        public void SetOutlineThickness(float thickness)
        {
            _outlineThickness = thickness;
        }
        
        private void OnDestroy()
        {
            StopOutlineAnimation();
        }
    }
}
