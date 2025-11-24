using System;
using System.Collections.Generic;
using LoopLegacy.Loader;
using LoopLegacy.Manager;
using LoopLegacy.State;
using LoopLegacy.UI.Component;
using LoopLegacy.UI.Controller;
using TMPro;

namespace LoopLegacy.UI.Helper
{
    public class LibraryHelper : ShopHelper
    {
        public LibraryHelper(ShopController shopController) : base(shopController)
        {
        }

        public override void Buy(int category, object data, Action onBuy)
        {
            if (category == 1)
            {
                var upgrade = data as UpgradeData;
                int currentLevel = PersistentGameState.Instance.HouseState.GetUpgradeLevel(upgrade.type);
                if (currentLevel >= upgrade.maxLevel - 1)
                {
                    string message = Utils.GetUIString("max-level");
                    ConfirmationController.Instance.ShowWarning(message);
                    return;
                }

                var price = upgrade.prices[currentLevel + 1];
                if (PersistentGameState.Instance.Gold.Value < price)
                {
                    string message = Utils.GetUIString("not-enough-gold", new object[] { price.ToString("n0") });
                    ConfirmationController.Instance.ShowWarning(message);
                    return;
                }
                else if (currentLevel < upgrade.maxLevel - 1)
                {
                    string message = Utils.GetUIString(
                        "upgrade-confirmation",
                        new object[] { Utils.GetUpgradeName(upgrade.type), price.ToString("n0"), currentLevel + 1 });
                    ConfirmationController.Instance.ShowConfirmation(message, () =>
                    {
                        PersistentGameState.Instance.SpendGold(price);
                        PersistentGameState.Instance.HouseState.Upgrade(upgrade.type);
                        PersistentGameState.Instance.SaveState();
                        onBuy?.Invoke();
                    });
                }
                else
                {
                    string message = Utils.GetUIString("max-level");
                    ConfirmationController.Instance.ShowWarning(message);
                    return;
                }
            }
        }

        public override void Initialize()
        {
            foreach (var button in _shopController.CategoryButtons)
            {
                button.gameObject.SetActive(false);
            }

            _shopController.CategoryButtons[0].gameObject.SetActive(true);
            _shopController.CategoryButtons[1].gameObject.SetActive(true);
            _shopController.CategoryButtons[0].GetComponentInChildren<TMP_Text>().text = Utils.GetUIString("library");
            _shopController.CategoryButtons[1].GetComponentInChildren<TMP_Text>().text = Utils.GetUIString("library_management");
        }

        public override object[] Populate(int category)
        {
            if (category == 0)
            {
                _shopController.ShowLibrary();
                return new object[0];
            }
            else if (category == 1)
            {
                return PopulateLibraryManagementList();
            }
            else
            {
                throw new InvalidOperationException("Invalid category: " + category);
            }
        }

        private object[] PopulateLibraryManagementList()
        {
            _shopController.HideLibrary();
            var shopList = new List<object>
            {
                TableManager.GetUpgrade(UpgradeType.LibraryManagement),
            };
            return shopList.ToArray();
        }

        public override void Select(int category, object data)
        {
            string dialogueText = "";
            string priceText = "";
            if (category == 1)
            {
                var upgrade = data as UpgradeData;
                dialogueText = $"<size=120%>[{Utils.GetUpgradeName(upgrade.type)}]</size>";
                priceText = "";
                var level = PersistentGameState.Instance.HouseState.GetUpgradeLevel(upgrade.type);
                if (level < upgrade.maxLevel - 1)
                {
                    dialogueText += $"\n{upgrade.values[level]} -> {upgrade.values[level + 1]}";
                    priceText = $"{upgrade.prices[level + 1]:n0}G";
                }
                else
                {
                    dialogueText += $"\n{upgrade.values[level]} (MAX)";
                    priceText = "MAX";
                }
                dialogueText += $"\n{Utils.GetUpgradeDescription(upgrade.type)}";
                _shopController.UpdateDialogueText(dialogueText, priceText);
            }

            _shopController.UpdateDialogueText(dialogueText, priceText);
        }

        public override void SetShopElement(ShopElement element, int category, object data)
        {
            if (category == 1)
            {
                var upgrade = data as UpgradeData;
                var upgradeLevel = PersistentGameState.Instance.HouseState.GetUpgradeLevel(upgrade.type);
                string detailText = upgradeLevel < upgrade.maxLevel - 1 ?
                    $"{upgrade.values[upgradeLevel]} -> {upgrade.values[upgradeLevel + 1]}" :
                    $"{upgrade.values[upgradeLevel]} (MAX)";
                string priceText = upgradeLevel < upgrade.maxLevel - 1 ? $"{upgrade.prices[upgradeLevel + 1]:n0}G" : "-";
                element.UpdateElement(
                    null,
                    Utils.GetUpgradeName(upgrade.type),
                    detailText,
                    priceText,
                    "Lv. " + upgradeLevel);
            }
        }
    }
}