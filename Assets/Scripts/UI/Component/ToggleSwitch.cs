using System;
using System.Collections;
using LoopLegacy.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LoopLegacy.UI.Component
{
    public class ToggleSwitch : MonoBehaviour, IPointerClickHandler
    {
        [Header("Slider setup")] 
        [SerializeField, Range(0, 1f)]
        protected float sliderValue;
        private bool _currentValue;
        public bool CurrentValue
        {
            get => _currentValue;
            private set
            {
                _currentValue = value;
                _text.text = _currentValue ? "ON" : "OFF";
            }
        }
        
        private bool _previousValue;
        private Slider _slider;

        [Header("Animation")] 
        [SerializeField, Range(0, 1f)] private float animationDuration = 0.5f;
        [SerializeField] private AnimationCurve slideEase =
            AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Coroutine _animateSliderCoroutine;

        [Header("Audio")]
        [SerializeField] private AudioClip _audioClip;

        [Header("Events")] 
        public UnityEvent onToggleOn;
        public UnityEvent onToggleOff; 

        private TMP_Text _text;
        
        protected Action transitionEffect;
        
        protected virtual void OnValidate()
        {
            SetupToggleComponents();

            _slider.value = sliderValue;
        }

        private void SetupToggleComponents()
        {
            if (_slider != null)
                return;

            SetupSliderComponent();
        }

        private void SetupSliderComponent()
        {
            _slider = GetComponent<Slider>();

            if (_slider == null)
            {
                Debug.Log("No slider found!", this);
                return;
            }

            _slider.interactable = false;
            var sliderColors = _slider.colors;
            sliderColors.disabledColor = Color.white;
            _slider.colors = sliderColors;
            _slider.transition = Selectable.Transition.None;
        }


        protected virtual void Awake()
        {
            SetupSliderComponent();
            _text = GetComponentInChildren<TMP_Text>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Toggle();
            AudioManager.Instance.PlaySFXSound(_audioClip);
        }

        public void SetValue(bool value)
        {
            CurrentValue = value;
            _slider.value = value ? 1 : 0;
        }
        
        private void Toggle()
        {
            SetStateAndStartAnimation(!CurrentValue);
        }
        
        private void SetStateAndStartAnimation(bool state)
        {
            _previousValue = CurrentValue;
            CurrentValue = state;

            if (_previousValue != CurrentValue)
            {
                if (CurrentValue)
                    onToggleOn?.Invoke();
                else
                    onToggleOff?.Invoke();
            }

            if (_animateSliderCoroutine != null)
                StopCoroutine(_animateSliderCoroutine);

            _animateSliderCoroutine = StartCoroutine(AnimateSlider());
        }


        private IEnumerator AnimateSlider()
        {
            float startValue = _slider.value;
            float endValue = CurrentValue ? 1 : 0;

            float time = 0;
            if (animationDuration > 0)
            {
                while (time < animationDuration)
                {
                    time += Time.deltaTime;

                    float lerpFactor = slideEase.Evaluate(time / animationDuration);
                    _slider.value = sliderValue = Mathf.Lerp(startValue, endValue, lerpFactor);

                    transitionEffect?.Invoke();
                        
                    yield return null;
                }
            }

            _slider.value = endValue;
        }
    }
}