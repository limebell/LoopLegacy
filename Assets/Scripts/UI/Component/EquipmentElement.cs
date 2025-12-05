using LoopLegacy.Battle;
using LoopLegacy.Manager;
using LoopLegacy.State;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LoopLegacy.UI.Component
{
    public class EquipmentElement : MonoBehaviour
    {
        [SerializeField] public EquipmentType _type;
        [SerializeField] public TMP_Text _typeText;
        [SerializeField] public ItemContainer _itemContainer;
        [SerializeField] public TMP_Text _nameText;
        [SerializeField] public TMP_Text _baseStatText;
        [SerializeField] public TMP_Text _multiplierText;
        [SerializeField] public Button _changeButton;

        public UnityEvent<EquipmentType> onChangeButtonClicked;

        void Start()
        {
            _changeButton.onClick.AddListener(OnChangeButtonClicked);
            _itemContainer.onClick.AddListener(OnChangeButtonClicked);
            _typeText.text = _type switch
            {
                EquipmentType.Weapon => Utils.GetUIString("weapon"),
                EquipmentType.Armor => Utils.GetUIString("armor"),
                _ => throw new NotImplementedException()
            };
        }

        void OnEnable()
        {
            UpdateVisuals();
        }

        private void OnChangeButtonClicked()
        {
            onChangeButtonClicked?.Invoke(_type);
        }

        private void UpdateVisuals()
        {
            if (_type == EquipmentType.Weapon)
            {
                UpdateWeaponVisual();
            }
            else if (_type == EquipmentType.Armor)
            {
                UpdateArmorVisual();
            }
        }

        private void UpdateWeaponVisual()
        {
            // 스탯 표기 업데이트
            var weapon = PersistentGameState.Instance.GetCurrentWeapon();

            if (weapon != null)
            {
                string weaponNameString = Utils.GetEquipmentName(EquipmentType.Weapon, weapon.Id);
                _nameText.text = weaponNameString + (weapon.EnchantmentLevel > 0 ? $" (+{weapon.EnchantmentLevel})" : "");
                _baseStatText.text = $"{(int)weapon.GetCalculatedBaseDamage():n0}";
                /*_baseStatText.text =
                    $"{(int)weapon.GetBaseDamage():n0} " +
                    (weapon.EnchantmentLevel == 1
                        ? ""
                        : $"<color=#4FC1FF>+{(int)weapon.GetEnchantedBaseDamage():n0}</color>");*/
                _multiplierText.text = $"{(int)weapon.GetCalculatedDamageMultiplier()}%";
                /*_multiplierText.text =
                    $"{(int)weapon.GetDamageMultiplier()}%" +
                    (weapon.EnchantmentLevel == 1
                        ? ""
                        : $" <color=#4FC1FF>+{(int)weapon.GetEnchantedDamageMultiplier()}%</color>");*/
                _itemContainer.SetImage(weapon.Sprite);
            }
            else
            {
                _nameText.text = Utils.GetUIString("no-equipment");
                _baseStatText.text = "";
                _multiplierText.text = "";
                _itemContainer.SetImage(null);
            }
        }

        private void UpdateArmorVisual()
        {
            var armor = PersistentGameState.Instance.GetCurrentArmor();
            if (armor != null)
            {
                string armorNameString = Utils.GetEquipmentName(EquipmentType.Armor, armor.Id);
                _nameText.text = armorNameString + (armor.EnchantmentLevel > 0 ? $" (+{armor.EnchantmentLevel})" : "");
                _baseStatText.text = $"{(int)armor.GetCalculatedBaseDefense():n0}";
                /*_baseStatText.text =
                    $"{(int)armor.GetBaseDefense():n0} " +
                    (armor.EnchantmentLevel == 1
                        ? ""
                        : $"<color=#4FC1FF>+{(int)armor.GetEnchantedBaseDefense():n0}</color>");*/
                _multiplierText.text = $"{(int)armor.GetCalculatedDefenseMultiplier()}%";
                /*_multiplierText.text =
                    $"{(int)armor.GetDefenseMultiplier()}%" +
                    (armor.EnchantmentLevel == 1
                        ? ""
                        : $" <color=#4FC1FF>+{(int)armor.GetEnchantedDefenseMultiplier()}%</color>");*/
                
                _itemContainer.SetImage(armor.Sprite);
            }
            else
            {
                _nameText.text = Utils.GetUIString("no-equipment");
                _baseStatText.text = "";
                _multiplierText.text = "";
                _itemContainer.SetImage(null);
            }
        }
    }
}