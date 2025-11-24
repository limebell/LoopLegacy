using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LoopLegacy.UI.Component
{
    public class VolumeComponent : MonoBehaviour
    {
        [SerializeField]
        private Slider _slider;
        [SerializeField]
        private Button _muteButton;
        [SerializeField]
        private Image _iconImage;
        [SerializeField]
        private TextMeshProUGUI _maxText;
        [SerializeField]
        private Sprite[] _iconSprites;

        public UnityEvent<int> onValueChanged;
        public int value
        {
            get => (int)_slider.value;
            set => _slider.value = value;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _muteButton.onClick.AddListener(OnMuteButtonClicked);
            _slider.onValueChanged.AddListener(OnValueChanged);
            UpdateComponent(value);
        }

        void OnMuteButtonClicked()
        {
            _slider.value = 0;
        }

        void OnValueChanged(float value)
        {
            UpdateComponent((int)value);
            onValueChanged?.Invoke((int)value);
        }

        private void UpdateComponent(int value)
        {
            _maxText.color = value == _slider.maxValue ?
                new Color(0.2823f, 0.2470f, 0.2274f, 1.0f) :
                new Color(0.5960f, 0.5490f, 0.5294f, 0.2f);

            if (value == 0)
            {
                _iconImage.sprite = _iconSprites[0];
            }
            else if (value < 4)
            {
                _iconImage.sprite = _iconSprites[1];
            }
            else if (value < 7)
            {
                _iconImage.sprite = _iconSprites[2];
            }
            else
            {
                _iconImage.sprite = _iconSprites[3];
            }
        }
    }
}
