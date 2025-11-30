using LoopLegacy.Loader;
using LoopLegacy.Manager;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoopLegacy.UI.Component
{
    public class DropUGUIElement : MonoBehaviour
    {
        [SerializeField] public Image _image;
        [SerializeField] public TextMeshProUGUI _nameText;

        public void SetDrop(DropEntry drop)
        {
            if (drop.itemType == DropType.Relic)
            {
                var relicLevel = drop.relicLevel;
                RelicData relic = TableManager.GetRelic(drop.relicEffectName);
                _image.sprite = relic.sprite;
                string name = Utils.GetRelicName(relic.effectName);
                _nameText.text = $"{name} Lv. {relicLevel + 1}";
                return;
            }
            else
            {
                EquipmentData equipment = TableManager.GetEquipment(drop.itemType switch
                {
                    DropType.Weapon => EquipmentType.Weapon,
                    DropType.Armor => EquipmentType.Armor,
                    _ => throw new NotImplementedException()
                }, drop.itemId);
                _image.sprite = equipment.sprite;

                string name = Utils.GetEquipmentName(equipment.type, equipment.id);
                _nameText.text = name;
            }
        }
    }
}