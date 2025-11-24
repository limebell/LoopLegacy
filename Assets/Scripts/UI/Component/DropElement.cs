using System;
using TMPro;
using UnityEngine;

namespace LoopLegacy.UI.Component
{
    public class DropElement : MonoBehaviour
    {
        [SerializeField] private ItemContainer _imageContainer;
        [SerializeField] private TMP_Text _typeLabel;
        [SerializeField] private GameObject _newIcon;
        [SerializeField] private TMP_Text _countLabel;

        public void UpdateElement(Sprite image, string type, bool isNew, int count, Action onClick)
        {
            _imageContainer.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                _imageContainer.onClick.AddListener(() => onClick?.Invoke());
            }
            _imageContainer.SetImage(image);
            _typeLabel.text = type;
            _newIcon.SetActive(isNew);
            _countLabel.gameObject.SetActive(count > 1);
            _countLabel.text = $"x{count}";
        }
    }
}