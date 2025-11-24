using System;
using System.Collections.Generic;
using LoopLegacy.Loader;
using LoopLegacy.Manager;
using LoopLegacy.State;
using LoopLegacy.UI.Component;
using LoopLegacy.UI.Controller;

namespace LoopLegacy.UI.Helper
{
    public class MaidShopHelper : ShopHelper
    {
        public MaidShopHelper(ShopController shopController) : base(shopController)
        {
        }

        public override void Buy(int category, object data, Action onBuy)
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

        public override void Initialize()
        {
            foreach (var button in _shopController.CategoryButtons)
            {
                button.gameObject.SetActive(false);
            }
        }

        public override object[] Populate(int category)
        {
            var shopList = new List<object>
            {
                TableManager.GetUpgrade(UpgradeType.TerritoryLevel)
            };
            return shopList.ToArray();
        }

        public override void Select(int category, object data)
        {
            var upgrade = data as UpgradeData;
            string dialogueText = $"<size=120%>[{Utils.GetUpgradeName(upgrade.type)}]</size>";
            string priceText = "";
            var level = PersistentGameState.Instance.HouseState.GetUpgradeLevel(upgrade.type);
            if (level < upgrade.maxLevel - 1)
            {
                dialogueText += $"\n\n{Utils.GetUpgradeDescription(upgrade.type, level)}";
                priceText = $"{upgrade.prices[level + 1]:n0}G";
            }
            else
            {
                dialogueText += $"\n\n{Utils.GetUIString("max-territory-level")}";
                priceText = "MAX";
            }
            _shopController.UpdateDialogueText(dialogueText, priceText);
        }

        public override void SetShopElement(ShopElement element, int category, object data)
        {
            var upgrade = data as UpgradeData;
            int myLevel = PersistentGameState.Instance.HouseState.GetUpgradeLevel(upgrade.type);
            string priceText = myLevel < upgrade.maxLevel - 1 ? $"{upgrade.prices[myLevel + 1]:n0}G" : "-";
            element.UpdateElement(
                null,
                Utils.GetUpgradeName(upgrade.type),
                "",
                priceText,
                "Lv. " + (myLevel + 1));
        }
    }
}