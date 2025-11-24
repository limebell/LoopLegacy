using LoopLegacy.Battle;
using LoopLegacy.Loader;
using LoopLegacy.Manager;
using LoopLegacy.State;
using LoopLegacy.UI.Component;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoopLegacy.UI.Controller
{
    public class EquipmentController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _backButton;
        [SerializeField] private VirtualizedScrollRect _virtualizedScrollRect;

        private Action _onClose;
        private EquipmentType _category;
        private List<object> _currentDataList = new List<object>();

        void Awake()
        {
            SetupVirtualizedScrollRect();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _backButton.onClick.AddListener(OnBackButtonClicked);
        }

        public void Show(EquipmentType equipmentType, Action onClose)
        {
            _category = equipmentType;
            _title.text = Utils.GetUIString($"equipment-title_{(_category == EquipmentType.Weapon ? "weapon" : "armor")}");
            PopulateEquipmentList();
            _onClose = onClose;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _onClose?.Invoke();
            _onClose = null;
        }

        private void OnBackButtonClicked()
        {
            Hide();
        }

        private void SetupVirtualizedScrollRect()
        {
            // VirtualizedScrollRect의 OnItemUpdate 이벤트에 데이터 바인딩 함수 연결
            _virtualizedScrollRect.OnItemUpdate += OnItemUpdate;
        }

        private void OnItemUpdate(GameObject element, object data, int index)
        {
            var listElement = element.GetComponent<ListElement>();
            var equipmentData = data as EquipmentData;
            
            if (equipmentData == null) return;

            var equipments = PersistentGameState.Instance.InventoryState.GetOwnedEquipments(_category);
            var count = equipments[equipmentData.id];
            if (count == 0 || equipmentData.type != _category) 
            {
                element.SetActive(false);
                return;
            }

            SetListElement(listElement, equipmentData, count);
        }

        private void PopulateEquipmentList()
        {
            _currentDataList.Clear();

            var equipments = PersistentGameState.Instance.InventoryState.GetOwnedEquipments(_category);
            for (int id = 0; id < equipments.Length; id++)
            {
                var count = equipments[id];
                var equipment = TableManager.GetEquipment(_category, id);
                if (count == 0 || equipment.type != _category) continue;

                _currentDataList.Add(equipment);
            }

            // VirtualizedScrollRect에 데이터 설정
            _virtualizedScrollRect.SetData(_currentDataList);
        }

        private void SetListElement(ListElement listElement, EquipmentData data, int count)
        {
            string detailText = "";
            switch (data.type)
            {
                case EquipmentType.Weapon:
                    var weapon = new Weapon(data, count);
                    detailText = $"{(int)weapon.GetBaseDamage():n0} " +
                        (weapon.EnchantmentLevel == 1
                            ? ""
                            : $"<color=#4FC1FF>+{(int)weapon.GetEnchantedBaseDamage():n0}</color> ") +
                        $"(+{(int)weapon.GetDamageMultiplier()}%" +
                        (weapon.EnchantmentLevel == 1
                            ? ")"
                            : $" <color=#4FC1FF>+{(int)weapon.GetEnchantedDamageMultiplier()}%</color>)");
                    break;
                case EquipmentType.Armor:
                    var armor = new Armor(data, count);
                    detailText = $"{armor.GetBaseDefense():n0} " +
                        (armor.EnchantmentLevel == 1
                            ? ""
                            : $"<color=#4FC1FF>+{armor.GetEnchantedBaseDefense():n0}</color> ") +
                        $"(+{(int)armor.GetDefenseMultiplier()}%" +
                        (armor.EnchantmentLevel == 1
                            ? ")"
                            : $" <color=#4FC1FF>+{(int)armor.GetEnchantedDefenseMultiplier()}%</color>)");
                    break;
            }
            string nameText = Utils.GetEquipmentName(data.type, data.id);
            if (IsEquipped(data))
            {
                nameText += $" <color=#41D229>({Utils.GetUIString("equipped")})</color>";
            }
            string countText = $"{Utils.GetUIString("owned-count")}: {count}";
            listElement.UpdateElement(data.sprite, nameText, detailText, countText, () => OnEquipmentClick(data));
        }

        private void OnEquipmentClick(EquipmentData equipment)
        {
            string equipmentName = Utils.GetEquipmentName(equipment.type, equipment.id);
            bool isEquipped = IsEquipped(equipment);

            if (isEquipped)
            {
                string message = Utils.GetUIString("already-equipped", new object[] { equipmentName });
                ConfirmationController.Instance.ShowWarning(message);
            }
            else
            {
                string message = Utils.GetUIString("equip-confirmation", new object[] { equipmentName });
                int currentId = _category == EquipmentType.Weapon ?
                    PersistentGameState.Instance.GetCurrentWeapon().Id :
                    PersistentGameState.Instance.GetCurrentArmor().Id;
                    
                string statTypeText = Utils.GetUIString(_category == EquipmentType.Weapon ? "damage" : "protection");
                if (currentId != -1)
                {
                    var currentEquipment = TableManager.GetEquipment(_category, currentId);
                    message += $"\n\n{statTypeText}\n{CalculateStat(currentEquipment):n0} > {CalculateStat(equipment):n0}";
                }
                else
                {
                    message += $"\n\n{statTypeText}\n{CalculateStat(equipment):n0}";
                }
                ConfirmationController.Instance.ShowConfirmation(message, () => ConfirmEquip(equipment));
            }
        }

        private int CalculateStat(EquipmentData equipment)
        {
            if (_category == EquipmentType.Weapon)
            {
                Weapon weapon = new Weapon(equipment, PersistentGameState.Instance.InventoryState.GetOwnedEquipments(_category)[equipment.id]);
                return (int)weapon.GetFinalDamage(GameManager.GetStat(StatType.ATK));
            }
            else if (_category == EquipmentType.Armor)
            {
                Armor armor = new Armor(equipment, PersistentGameState.Instance.InventoryState.GetOwnedEquipments(_category)[equipment.id]);
                return (int)armor.GetFinalDefense(GameManager.GetStat(StatType.DEF));
            }

            return -1;
        }

        private bool IsEquipped(EquipmentData equipment)
        {
            switch (equipment.type)
            {
                case EquipmentType.Weapon:
                    return PersistentGameState.Instance.GetCurrentWeapon().Id == equipment.id;
                case EquipmentType.Armor:
                    return PersistentGameState.Instance.GetCurrentArmor().Id == equipment.id;
                default:
                    return false;
            }
        }

        private void ConfirmEquip(EquipmentData equipment)
        {
            PersistentGameState.Instance.Equip(equipment.type, equipment.id);

            Hide();
        }
    }
}
