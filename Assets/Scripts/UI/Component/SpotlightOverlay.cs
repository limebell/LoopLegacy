using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace LoopLegacy.UI.Component
{
    public class SpotlightOverlay : MonoBehaviour
    {
        private Image _topDimmed;
        private Image _bottomDimmed;
        private Image _leftDimmed;
        private Image _rightDimmed;
        [SerializeField] private Color _dimmedColor = new Color(0, 0, 0, 0.7f);
        
        private Image _topOutline;
        private Image _bottomOutline;
        private Image _leftOutline;
        private Image _rightOutline;
        [SerializeField] private Color _outlineColor = new Color(1, 1, 1, 1f);
        [SerializeField] private float _outlineThickness = 3f;
        [SerializeField] private float _outlineThicknessVariation = 2f;
        
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
            
            Hide();
        }

        private void CreateDimmedOverlays()
        {
            _topDimmed = CreateDimmedImage("TopDimmed");
            _bottomDimmed = CreateDimmedImage("BottomDimmed");
            _leftDimmed = CreateDimmedImage("LeftDimmed");
            _rightDimmed = CreateDimmedImage("RightDimmed");
        }
        
        private void CreateOutlines()
        {
            _topOutline = CreateOutlineImage("TopOutline");
            _bottomOutline = CreateOutlineImage("BottomOutline");
            _leftOutline = CreateOutlineImage("LeftOutline");
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
            image.raycastTarget = true;
            
            return image;
        }

        public void SetHighlightArea(RectTransform targetRect, float margin = 0f)
        {
            if (targetRect == null || _rectTransform == null)
                return;

            Canvas targetCanvas = targetRect.GetComponentInParent<Canvas>();
            Camera targetCamera = targetCanvas != null ? targetCanvas.worldCamera : null;
            
            Vector3[] worldCorners = new Vector3[4];
            targetRect.GetWorldCorners(worldCorners);
            
            Vector2[] screenCorners = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                if (targetCamera != null)
                {
                    screenCorners[i] = RectTransformUtility.WorldToScreenPoint(targetCamera, worldCorners[i]);
                }
                else
                {
                    screenCorners[i] = RectTransformUtility.WorldToScreenPoint(null, worldCorners[i]);
                }
            }

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
            
            Vector2 marginVector = new Vector2(margin, margin);
            Vector2 min = localCorners[0] - marginVector;
            Vector2 max = localCorners[2] + marginVector;
            
            UpdateOverlay(min, max);
            _currentMin = min;
            _currentMax = max;
            UpdateOutlines(min, max);
        }

        private void UpdateOverlay(Vector2 min, Vector2 max)
        {
            if (_rectTransform == null)
                return;

            float canvasWidth = _rectTransform.rect.width;
            float canvasHeight = _rectTransform.rect.height;
            
            float canvasLeft = -canvasWidth * 0.5f;
            float canvasRight = canvasWidth * 0.5f;
            float canvasBottom = -canvasHeight * 0.5f;
            float canvasTop = canvasHeight * 0.5f;
            
            if (_topDimmed != null)
            {
                _topDimmed.rectTransform.anchorMin = new Vector2(0, (max.y - canvasBottom) / canvasHeight);
                _topDimmed.rectTransform.anchorMax = new Vector2(1, 1);
                _topDimmed.rectTransform.offsetMin = Vector2.zero;
                _topDimmed.rectTransform.offsetMax = Vector2.zero;
            }
            
            if (_bottomDimmed != null)
            {
                _bottomDimmed.rectTransform.anchorMin = new Vector2(0, 0);
                _bottomDimmed.rectTransform.anchorMax = new Vector2(1, (min.y - canvasBottom) / canvasHeight);
                _bottomDimmed.rectTransform.offsetMin = Vector2.zero;
                _bottomDimmed.rectTransform.offsetMax = Vector2.zero;
            }
            
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
        
        private void UpdateOutlines(Vector2 min, Vector2 max)
        {
            if (_rectTransform == null)
                return;

            _currentMin = min;
            _currentMax = max;
            _currentOutlineThickness = _outlineThickness;
            
            UpdateOutlineThickness(_outlineThickness);
            
            StartOutlineAnimation();
        }
        
        private void UpdateOutlineThickness(float thickness)
        {
            if (_rectTransform == null)
                return;

            float canvasWidth = _rectTransform.rect.width;
            float canvasHeight = _rectTransform.rect.height;
            
            float canvasLeft = -canvasWidth * 0.5f;
            float canvasRight = canvasWidth * 0.5f;
            float canvasBottom = -canvasHeight * 0.5f;
            float canvasTop = canvasHeight * 0.5f;
            
            Vector2 min = _currentMin;
            Vector2 max = _currentMax;
            
            if (_topOutline != null)
            {
                _topOutline.rectTransform.anchorMin = new Vector2((min.x - canvasLeft) / canvasWidth, (max.y - canvasBottom) / canvasHeight);
                _topOutline.rectTransform.anchorMax = new Vector2((max.x - canvasLeft) / canvasWidth, (max.y - canvasBottom) / canvasHeight);
                _topOutline.rectTransform.offsetMin = new Vector2(0, 0);
                _topOutline.rectTransform.offsetMax = new Vector2(0, thickness);
            }
            
            if (_bottomOutline != null)
            {
                _bottomOutline.rectTransform.anchorMin = new Vector2((min.x - canvasLeft) / canvasWidth, (min.y - canvasBottom) / canvasHeight);
                _bottomOutline.rectTransform.anchorMax = new Vector2((max.x - canvasLeft) / canvasWidth, (min.y - canvasBottom) / canvasHeight);
                _bottomOutline.rectTransform.offsetMin = new Vector2(0, -thickness);
                _bottomOutline.rectTransform.offsetMax = new Vector2(0, 0);
            }
            
            if (_leftOutline != null)
            {
                _leftOutline.rectTransform.anchorMin = new Vector2((min.x - canvasLeft) / canvasWidth, (min.y - canvasBottom) / canvasHeight);
                _leftOutline.rectTransform.anchorMax = new Vector2((min.x - canvasLeft) / canvasWidth, (max.y - canvasBottom) / canvasHeight);
                _leftOutline.rectTransform.offsetMin = new Vector2(-thickness, 0);
                _leftOutline.rectTransform.offsetMax = new Vector2(0, 0);
            }
            
            if (_rightOutline != null)
            {
                _rightOutline.rectTransform.anchorMin = new Vector2((max.x - canvasLeft) / canvasWidth, (min.y - canvasBottom) / canvasHeight);
                _rightOutline.rectTransform.anchorMax = new Vector2((max.x - canvasLeft) / canvasWidth, (max.y - canvasBottom) / canvasHeight);
                _rightOutline.rectTransform.offsetMin = new Vector2(0, 0);
                _rightOutline.rectTransform.offsetMax = new Vector2(thickness, 0);
            }
        }
        
        private void StartOutlineAnimation()
        {
            StopOutlineAnimation();
            
            if (_topOutline == null || _bottomOutline == null || _leftOutline == null || _rightOutline == null)
                return;
            
            _outlineAnimation = DOTween.Sequence();
            
            Image[] outlines = { _topOutline, _bottomOutline, _leftOutline, _rightOutline };
            
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
        
        private void StopOutlineAnimation()
        {
            if (_outlineAnimation != null && _outlineAnimation.IsActive())
            {
                _outlineAnimation.Kill();
                _outlineAnimation = null;
            }
            
            if (_topOutline != null) _topOutline.color = _outlineColor;
            if (_bottomOutline != null) _bottomOutline.color = _outlineColor;
            if (_leftOutline != null) _leftOutline.color = _outlineColor;
            if (_rightOutline != null) _rightOutline.color = _outlineColor;
            
            UpdateOutlineThickness(_outlineThickness);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (_topDimmed != null) _topDimmed.gameObject.SetActive(true);
            if (_bottomDimmed != null) _bottomDimmed.gameObject.SetActive(true);
            if (_leftDimmed != null) _leftDimmed.gameObject.SetActive(true);
            if (_rightDimmed != null) _rightDimmed.gameObject.SetActive(true);
            if (_topOutline != null) _topOutline.gameObject.SetActive(true);
            if (_bottomOutline != null) _bottomOutline.gameObject.SetActive(true);
            if (_leftOutline != null) _leftOutline.gameObject.SetActive(true);
            if (_rightOutline != null) _rightOutline.gameObject.SetActive(true);
        }

        public void Hide()
        {
            StopOutlineAnimation();
            if (_topDimmed != null) _topDimmed.gameObject.SetActive(false);
            if (_bottomDimmed != null) _bottomDimmed.gameObject.SetActive(false);
            if (_leftDimmed != null) _leftDimmed.gameObject.SetActive(false);
            if (_rightDimmed != null) _rightDimmed.gameObject.SetActive(false);
            if (_topOutline != null) _topOutline.gameObject.SetActive(false);
            if (_bottomOutline != null) _bottomOutline.gameObject.SetActive(false);
            if (_leftOutline != null) _leftOutline.gameObject.SetActive(false);
            if (_rightOutline != null) _rightOutline.gameObject.SetActive(false);
        }


        public void SetDimmedColor(Color color)
        {
            _dimmedColor = color;
            if (_topDimmed != null) _topDimmed.color = color;
            if (_bottomDimmed != null) _bottomDimmed.color = color;
            if (_leftDimmed != null) _leftDimmed.color = color;
            if (_rightDimmed != null) _rightDimmed.color = color;
        }

        public void SetOutlineColor(Color color)
        {
            _outlineColor = color;
            if (_topOutline != null) _topOutline.color = color;
            if (_bottomOutline != null) _bottomOutline.color = color;
            if (_leftOutline != null) _leftOutline.color = color;
            if (_rightOutline != null) _rightOutline.color = color;
        }

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
