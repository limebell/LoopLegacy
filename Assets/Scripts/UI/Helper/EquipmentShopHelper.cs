using System;
using System.Collections.Generic;
using System.Linq;
using LoopLegacy.Loader;
using LoopLegacy.Manager;
using LoopLegacy.State;
using LoopLegacy.UI.Component;
using LoopLegacy.UI.Controller;
using TMPro;

namespace LoopLegacy.UI.Helper
{
    public class EquipmentShopHelper : ShopHelper
    {
        public EquipmentShopHelper(ShopController shopController) : base(shopController)
        {
        }

        public override void Buy(int category, object data, Action onBuy)
        {
            var equipment = data as EquipmentData;
            if (PersistentGameState.Instance.InventoryState.GetOwnedEquipments(equipment.type)[equipment.id] >=
                InventoryState.MAX_EQUIPMENT_DUPLICATE_COUNT)
            {
                // Click not allowed when max duplicate count is reached
                ConfirmationController.Instance.ShowWarning(Utils.GetUIString("max-duplicate-count"));
                return;
            }
            
            int price = GetEquipmentPrice(equipment);
            if (PersistentGameState.Instance.Gold.Value < price)
            {
                string warningMessage = Utils.GetUIString("not-enough-gold", new object[] { price.ToString("n0") });
                ConfirmationController.Instance.ShowWarning(warningMessage);
                return;
            }

            string name = Utils.GetEquipmentName(equipment.type, equipment.id);
            string message = Utils.GetUIString("buy-confirmation", new object[] { name, price.ToString("n0") });
            ConfirmationController.Instance.ShowConfirmation(message, () =>
            {
                PersistentGameState.Instance.SpendGold(price);
                PersistentGameState.Instance.InventoryState.AddEquipment(equipment.type, equipment.id);
                PersistentGameState.Instance.SaveState();
                onBuy?.Invoke();
            });
        }

        public override void Initialize()
        {
            foreach (var button in _shopController.CategoryButtons)
            {
                button.gameObject.SetActive(false);
            }

            _shopController.CategoryButtons[0].gameObject.SetActive(true);
            _shopController.CategoryButtons[1].gameObject.SetActive(true);
            _shopController.CategoryButtons[0].GetComponentInChildren<TMP_Text>().text = Utils.GetUIString("weapon");
            _shopController.CategoryButtons[1].GetComponentInChildren<TMP_Text>().text = Utils.GetUIString("armor");
        }

        public override object[] Populate(int category)
        {
            if (category == 0)
            {
                return PopulateWeaponList();
            }
            else if (category == 1)
            {
                return PopulateArmorList();
            }
            else
            {
                throw new InvalidOperationException("Invalid category: " + category);
            }
        }

        private object[] PopulateWeaponList()
        {
            var shopList = new List<object>();
            var weapons = TableManager.GetEquipments(EquipmentType.Weapon)
                .Where(e => e.price != -1)
                .ToList();
            foreach (var weapon in weapons)
            {
                shopList.Add(weapon);
            }
            return shopList.ToArray();
        }

        private object[] PopulateArmorList()
        {
            var shopList = new List<object>();
            var armors = TableManager.GetEquipments(EquipmentType.Armor)
                .Where(e => e.price != -1)
                .ToList();
            foreach (var armor in armors)
            {
                shopList.Add(armor);
            }
            return shopList.ToArray();
        }

        public override void Select(int category, object data)
        {
            var equipment = data as EquipmentData;
            int ownedCount = PersistentGameState.Instance.InventoryState.GetOwnedEquipments(equipment.type)[equipment.id];
            string dialogueText = $"<size=120%>[{Utils.GetEquipmentName(equipment.type, equipment.id)}]</size>";
            dialogueText += $"\n{Utils.GetUIString("owned-count")}: {ownedCount}";
            dialogueText += $"\n{equipment.baseValue} (+{equipment.multiplier}%)";
            dialogueText += $"\n{Utils.GetEquipmentDescription(equipment.type, equipment.id)}";
            string priceText = GetEquipmentPriceText(equipment);
            _shopController.UpdateDialogueText(dialogueText, priceText);
        }

        public override void SetShopElement(ShopElement element, int category, object data)
        {
            var equipment = data as EquipmentData;
            element.UpdateElement(
                equipment.sprite,
                Utils.GetEquipmentName(equipment.type, equipment.id),
                $"{equipment.baseValue} (+{equipment.multiplier}%)",
                GetEquipmentPriceText(equipment),
                $"{Utils.GetUIString("owned-count")}: {PersistentGameState.Instance.InventoryState.GetOwnedEquipments(equipment.type)[equipment.id]}");
        }

        private string GetEquipmentPriceText(EquipmentData equipment)
        {
            int ownedCount = PersistentGameState.Instance.InventoryState.GetOwnedEquipments(equipment.type)[equipment.id];
            return ownedCount >= InventoryState.MAX_EQUIPMENT_DUPLICATE_COUNT ?
                "(MAX)" :
                $"{GetEquipmentPrice(equipment):n0}G";
        }

        private int GetEquipmentPrice(EquipmentData equipment)
        {
            int ownedCount = PersistentGameState.Instance.InventoryState.GetOwnedEquipments(equipment.type)[equipment.id];
            return (int)(equipment.price * (1.0f + ownedCount * 0.1f));
        }
    }
}