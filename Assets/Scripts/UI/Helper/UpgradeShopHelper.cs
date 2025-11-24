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
    public class UpgradeShopHelper : ShopHelper
    {
        public UpgradeShopHelper(ShopController shopController) : base(shopController)
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

            _shopController.CategoryButtons[0].gameObject.SetActive(true);
            _shopController.CategoryButtons[1].gameObject.SetActive(true);
            _shopController.CategoryButtons[0].GetComponentInChildren<TMP_Text>().text = Utils.GetUIString("stats");
            _shopController.CategoryButtons[1].GetComponentInChildren<TMP_Text>().text = Utils.GetUIString("special");
        }

        public override object[] Populate(int category)
        {
            if (category == 0)
            {
                return PopulateUpgradeStatList();
            }
            else if (category == 1)
            {
                return PopulateUpgradeEtcList();
            }
            else
            {
                throw new InvalidOperationException("Invalid category: " + category);
            }
        }

        private object[] PopulateUpgradeStatList()
        {
            var shopList = new List<object>
            {
                TableManager.GetUpgrade(UpgradeType.BaseHP),
                TableManager.GetUpgrade(UpgradeType.BaseATK),
                TableManager.GetUpgrade(UpgradeType.BaseDEF),
                TableManager.GetUpgrade(UpgradeType.BaseLUC)
            };
            return shopList.ToArray();
        }

        private object[] PopulateUpgradeEtcList()
        {
            var shopList = new List<object>
            {
                TableManager.GetUpgrade(UpgradeType.BoostExp),
                TableManager.GetUpgrade(UpgradeType.BoostGold),
                TableManager.GetUpgrade(UpgradeType.MovementSpeed),
                TableManager.GetUpgrade(UpgradeType.EncounterRate),
                TableManager.GetUpgrade(UpgradeType.InstantEncounter)
            };
            return shopList.ToArray();
        }

        public override void Select(int category, object data)
        {
            var upgrade = data as UpgradeData;
            string dialogueText = $"<size=120%>[{Utils.GetUpgradeName(upgrade.type)}]</size>";
            string priceText = "";
            var level = PersistentGameState.Instance.HouseState.GetUpgradeLevel(upgrade.type);
            string values;
            if (level < upgrade.maxLevel - 1)
            {
                values = $"{upgrade.values[level]} -> {upgrade.values[level + 1]}";
                priceText = $"{upgrade.prices[level + 1]:n0}G";
            }
            else
            {
                values = $"{upgrade.values[level]}";
                priceText = "MAX";
            }
            dialogueText += $"\n\n{Utils.GetUpgradeDescription(upgrade.type, args: new object[] { values })}";
            _shopController.UpdateDialogueText(dialogueText, priceText);
        }

        public override void SetShopElement(ShopElement element, int category, object data)
        {
            var upgrade = data as UpgradeData;
            int myLevel = PersistentGameState.Instance.HouseState.GetUpgradeLevel(upgrade.type);
            string detailText = myLevel < upgrade.maxLevel - 1 ?
                $"{upgrade.values[myLevel]} -> {upgrade.values[myLevel + 1]}" :
                $"{upgrade.values[myLevel]} (MAX)";
            string priceText = myLevel < upgrade.maxLevel - 1 ? $"{upgrade.prices[myLevel + 1]:n0}G" : "-";
            string levelText = "Lv. " + (myLevel + 1);
            if (upgrade.type == UpgradeType.InstantEncounter)
            {
                detailText = "";
                levelText = myLevel == 0 ? "LOCKED" : "UNLOCKED";
            }
            element.UpdateElement(
                null,
                Utils.GetUpgradeName(upgrade.type),
                detailText,
                priceText,
                levelText);
        }
    }
}