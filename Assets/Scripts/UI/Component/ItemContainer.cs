using LoopLegacy.Battle;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LoopLegacy.UI.Component
{
    public class ItemContainer : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Image _outline;
        [SerializeField] private Color _outlineColor = new(1, 1, 1, 0f);
        [SerializeField] private TMP_Text _stackCountText;
        [SerializeField] private TMP_Text _levelText;

        private bool _isContentOutline = false;
        private float _contentOutlineThickness = 0.7f;
        private Color _contentOutlineColor = new(1, 1, 1, 0f);
        private Material _runtimeMat;
        public UnityEvent onClick;

        public Color OutlineColor
        {
            get => _outlineColor;
            set
            {
                _outlineColor = value;
                UpdateVisuals();
            }
        }

        void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnClickElement);
            SetImageInternal(_image.sprite);

            _runtimeMat = Instantiate(_image.material);
            _image.material = _runtimeMat;
            OverrideMaterial();
        }

        void OnEnable()
        {
#if UNITY_EDITOR
            UpdateVisuals();
#endif
        }

        void OnValidate()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall += UpdateVisuals;
#endif
        }

        private void OnClickElement()
        {
            onClick?.Invoke();
        }

        public void SetImage(Sprite sprite)
        {
            SetImageInternal(sprite);
            _stackCountText.text = "";
            _levelText.text = "";
            SetContentOutline(false);
        }

        private void SetImageInternal(Sprite sprite)
        {
            _image.sprite = sprite;
            if (sprite == null)
            {
                _image.color = new Color(1, 1, 1, 0);
            }
            else
            {
                _image.color = new Color(1, 1, 1, 1);
            }
        }

        public void SetRelic(Relic relic)
        {
            if (relic == null)
            {
                SetImageInternal(null);
                _stackCountText.text = "";
                _levelText.text = "";
                SetContentOutline(false);
                return;
            }

            SetImageInternal(relic.Sprite);
            _levelText.text = Utils.GetRomanNumber(relic.Level + 1);
            if (relic.Stack > 0)
            {
                _stackCountText.text = relic.Stack.ToString();
            }
            else
            {
                _stackCountText.text = "";
            }

            SetContentOutline(true, color: Utils.GetRelicGradeColor(relic.Grade));
        }

        private void UpdateVisuals()
        {
            if (_outline is not null)
            {
                _outline.color = _outlineColor;
            }
        }

        private void SetContentOutline(bool on, float thickness = 0.7f, Color color = default)
        {
            // 속성 덮어쓰기
            _isContentOutline = on;
            _contentOutlineThickness = thickness;
            _contentOutlineColor = color != default ? color : new Color(1f, 1f, 1f, 0f);
            if (_runtimeMat != null)
            {
                OverrideMaterial();
            }
        }

        private void OverrideMaterial()
        {
            _runtimeMat.SetFloat("_OutlineThickness", _isContentOutline ? _contentOutlineThickness : 0f);
            _runtimeMat.SetColor("_OutlineColor", _contentOutlineColor);
        }
    }
}