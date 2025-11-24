using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoopLegacy.UI.Component
{
    public class ListElement : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Image[] _icons;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _detailText;
        [SerializeField] private TMP_Text _countText;

        private Action _callBack;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(() => _callBack?.Invoke());
        }

        public void UpdateElement(Sprite sprite, string name, string detail, string count, Action callBack, bool[] icons = null)
        {
            _image.sprite = sprite;
            _image.color = new Color(1, 1, 1, sprite == null ? 0 : 1);
            if (icons == null)
            {
                for (int i = 0; i < _icons.Length; i++)
                {
                    _icons[i].gameObject.SetActive(false);
                }
            }
            else
            {
                for (int i = 0; i < _icons.Length; i++)
                {
                    _icons[i].gameObject.SetActive(icons[i]);
                }
            }

            if (sprite != null)
            {
                // sprite 비율에 맞게 조정
                int preferredWidth = Mathf.Max((int)sprite.rect.width, 32);
                int preferredHeight = Mathf.Max((int)sprite.rect.height, 32);
                if (sprite.rect.width > sprite.rect.height)
                {
                    _image.rectTransform.localScale = new Vector3(1, sprite.rect.height / sprite.rect.width, 1) * (sprite.rect.width / preferredWidth);
                }
                else if (sprite.rect.width < sprite.rect.height)
                {
                    _image.rectTransform.localScale = new Vector3(sprite.rect.width / sprite.rect.height, 1, 1) * (sprite.rect.height / preferredHeight);
                }
                else
                {
                    _image.rectTransform.localScale = Vector3.one * (sprite.rect.width / preferredWidth);
                }
            }
            _nameText.text = name;
            _detailText.text = detail;
            _countText.text = count;
            _callBack = callBack;
        }

        public void ToggleSelect(bool isSelected)
        {
            GetComponent<Button>().interactable = !isSelected;
        }
    }
}