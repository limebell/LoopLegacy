using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LoopLegacy.UI.Component
{
    public class Gauge : MonoBehaviour
    {
        [SerializeField]
        private Image _fillImage;

        [SerializeField]
        private Image _bufferImage;

        [SerializeField]
        [Range(0, 1)]
        private float _value;

        [SerializeField]
        [Range(0, 1)]
        private float _minValue = 0f;

        private void OnEnable()
        {
#if UNITY_EDITOR
            UpdateVisuals();
#endif
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall += UpdateVisuals;
#endif
        }

        public void SetValue(float value, bool ignoreBuffer = false)
        {
            _value = Mathf.Clamp01(value);
            if (_fillImage != null && _fillImage.rectTransform != null)
            {
                _fillImage.rectTransform.anchorMax = 
                    new Vector2(Mathf.Clamp01(_minValue + _value * (1 - _minValue)), _fillImage.rectTransform.anchorMax.y);

                if (_bufferImage != null && _bufferImage.rectTransform != null)
                {
                    if (ignoreBuffer)
                    {
                        _bufferImage.rectTransform.anchorMax = _fillImage.rectTransform.anchorMax;
                        return;
                    }
                    else
                    {
                        DOTween
                            .To(
                                () => _bufferImage.rectTransform.anchorMax.x,
                                x => _bufferImage.rectTransform.anchorMax = new Vector2(x, _bufferImage.rectTransform.anchorMax.y),
                                _fillImage.rectTransform.anchorMax.x, 0.5f)
                            .SetEase(Ease.OutQuad);
                    }
                }
            }
        }

        private void UpdateVisuals()
        {
            if (_fillImage != null && _fillImage.rectTransform != null)
            {
                _fillImage.rectTransform.anchorMax = 
                    new Vector2(Mathf.Clamp01(_minValue +_value * (1 - _minValue)), _fillImage.rectTransform.anchorMax.y);

                if (_bufferImage != null && _bufferImage.rectTransform != null)
                {
                    _bufferImage.rectTransform.anchorMax = 
                        new Vector2(_fillImage.rectTransform.anchorMax.x, _bufferImage.rectTransform.anchorMax.y);
                }
            }
        }
    }
}
