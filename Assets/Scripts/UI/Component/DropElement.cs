using System;
using LoopLegacy.Battle;
using LoopLegacy.Loader;
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

        public void UpdateElement(Relic relic, Action onClick)
        {
            _imageContainer.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                _imageContainer.onClick.AddListener(() => onClick?.Invoke());
            }
            _imageContainer.SetRelic(relic);
            _typeLabel.text = "UNLOCK";
            _newIcon.SetActive(true);
            _countLabel.gameObject.SetActive(false);
        }

        public void UpdateElement(EquipmentData equipment, bool isNew, int count, Action onClick)
        {
            _imageContainer.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                _imageContainer.onClick.AddListener(() => onClick?.Invoke());
            }
            _imageContainer.SetImage(equipment.sprite);
            _typeLabel.text = "DROP";
            _newIcon.SetActive(isNew);
            _countLabel.gameObject.SetActive(count > 1);
            _countLabel.text = $"x{count}";
        }
    }
}