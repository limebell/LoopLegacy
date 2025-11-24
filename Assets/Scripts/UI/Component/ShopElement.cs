using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoopLegacy.UI.Component
{
    public class ShopElement : MonoBehaviour
    {
        [SerializeField] private Image _itemImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _specText;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private TMP_Text _countText;

        private Action _callBack;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            _callBack?.Invoke();
        }

        public void UpdateElement(Sprite sprite, string name, string spec, string price, string count)
        {
            _itemImage.sprite = sprite;
            _nameText.text = name;
            _specText.text = spec;
            _priceText.text = price;
            _countText.text = count;
        }

        public void SetCallback(Action callBack)
        {
            _callBack = callBack;
        }

        public void ToggleSelect(bool isSelected)
        {
            GetComponent<Button>().interactable = !isSelected;
        }
    }
}