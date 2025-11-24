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
    public class RelicShopHelper : ShopHelper
    {
        public RelicShopHelper(ShopController shopController) : base(shopController)
        {
        }

        public override void Buy(int category, object data, Action onBuy)
        {
            if (category == 0)
            {
                var relic = data as RelicData;
                var myRelicLevel = PersistentGameState.Instance.CodexState.GetRelic(relic.id)?.Level ?? -1;

                if (myRelicLevel >= relic.values.Length - 1)
                {
                    ConfirmationController.Instance.ShowWarning(Utils.GetUIString("max-level"));
                    return;
                }
                else if (myRelicLevel >= relic.prices.Length - 1)
                {
                    ConfirmationController.Instance.ShowWarning(Utils.GetUIString("not-for-sale_description"));
                    return;
                }

                var price = relic.prices[myRelicLevel + 1];
                if (PersistentGameState.Instance.Gold.Value < price)
                {
                    string warningMessage = Utils.GetUIString("not-enough-gold", new object[] { price.ToString("n0") });
                    ConfirmationController.Instance.ShowWarning(warningMessage);
                    return;
                }

                string message = Utils.GetUIString(
                    "buy-confirmation",
                    new object[] { Utils.GetRelicName(relic.effectName),
                    price.ToString("n0") });
                ConfirmationController.Instance.ShowConfirmation(message, () =>
                {
                    PersistentGameState.Instance.SpendGold(price);
                    PersistentGameState.Instance.CodexState.UnlockRelicWithLevel(relic.id, myRelicLevel + 1);
                    PersistentGameState.Instance.SaveState();
                    onBuy?.Invoke();
                });
            }
            else if (category == 1)
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
                int myRelicCount = PersistentGameState.Instance.CodexState.GetAvailableRelics().Count;
                int requiredUnlockedRelicCount = GetRequiredUnlockedRelicCount(upgrade.type, currentLevel + 1);
                if (myRelicCount < requiredUnlockedRelicCount)
                {
                    string message = Utils.GetUIString("relic-upgrade-condition_warning", new object[] { requiredUnlockedRelicCount, myRelicCount });
                    ConfirmationController.Instance.ShowWarning(message);
                    return;
                }
                else if (PersistentGameState.Instance.Gold.Value < price)
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
                        if (upgrade.type == UpgradeType.InitialRelicSelect)
                        {
                            Initialize();
                        }
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
            else if (category == 2)
            {
                Debug.LogError("Cannot buy InitialRelicSelect");
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
            _shopController.CategoryButtons[0].GetComponentInChildren<TMP_Text>().text = Utils.GetUIString("relic");
            _shopController.CategoryButtons[1].GetComponentInChildren<TMP_Text>().text = Utils.GetUIString("upgrade");
            if (PersistentGameState.Instance.HouseState.GetUpgradeLevel(UpgradeType.InitialRelicSelect) > 0)
            {
                _shopController.CategoryButtons[2].gameObject.SetActive(true);
                _shopController.CategoryButtons[2].GetComponentInChildren<TMP_Text>().text = Utils.GetUIString("initial-relic");
            }
        }

        public override object[] Populate(int category)
        {
            if (category == 0)
            {
                return PopulateRelicList();
            }
            else if (category == 1)
            {
                return PopulateRelicUpgradeList();
            }
            else if (category == 2)
            {
                return PopulateInitialRelicSelectList();
            }
            else
            {
                throw new InvalidOperationException("Invalid category: " + category);
            }
        }

        private object[] PopulateRelicList()
        {
            _shopController.HideInitialRelicSelect();
            var shopList = new List<object>();
            var relics = TableManager.GetAllRelics().Where(r => r.prices.Length > 0).ToList();
            foreach (var relic in relics)
            {
                shopList.Add(relic);
            }
            return shopList.ToArray();
        }

        private object[] PopulateRelicUpgradeList()
        {
            _shopController.HideInitialRelicSelect();
            var shopList = new List<object>
            {
                TableManager.GetUpgrade(UpgradeType.MaxRelicCount),
                TableManager.GetUpgrade(UpgradeType.RelicRewardChoiceCount),
                TableManager.GetUpgrade(UpgradeType.RelicRewardRerollCount),
                TableManager.GetUpgrade(UpgradeType.RelicRewardRarity),
                TableManager.GetUpgrade(UpgradeType.InitialRelicSelect)
            };
            return shopList.ToArray();
        }

        private object[] PopulateInitialRelicSelectList()
        {
            _shopController.ShowInitialRelicSelect();
            return new object[0];
        }

        public override void Select(int category, object data)
        {
            string dialogueText = "";
            string priceText = "";
            if (category == 0)
            {
                var relic = data as RelicData;
                var relicName = Utils.GetRelicName(relic.effectName);
                var ownedRelic = PersistentGameState.Instance.CodexState.GetRelic(relic.id);
                string gradeText = $"<size=90%><color={Utils.GetRelicGradeColorHex(relic.grade)}>{Utils.GetUIString("grade_" + relic.grade.ToString().ToLowerInvariant())}</color></size>";
                if (ownedRelic == null)
                {
                    dialogueText = $"<size=120%>[{relicName} Lv. 1]</size>\n";
                    dialogueText += $"{gradeText}\n{Utils.GetRelicDescription(relic.effectName, relic.values[0].Split(':'))} (NEW)";
                    priceText = $"{relic.prices[0]:n0}G";
                }
                else if (ownedRelic.Level < relic.values.Length - 1)
                {
                    dialogueText = $"<size=120%>[{relicName} Lv. {ownedRelic.Level + 1} > {ownedRelic.Level + 2}]</size>\n";
                    var values = relic.values[ownedRelic.Level].Split(':');
                    var nextValues = relic.values[ownedRelic.Level + 1].Split(':');
                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] = $"{values[i]} -> {nextValues[i]}";
                    }
                    dialogueText += $"{gradeText}\n{Utils.GetRelicDescription(relic.effectName, values.ToArray())}";
                    priceText = ownedRelic.Level < relic.prices.Length - 1 ? $"{relic.prices[ownedRelic.Level + 1]:n0}G" : $"({Utils.GetUIString("not-for-sale")})";
                }
                else
                {
                    var values = relic.values[ownedRelic.Level].Split(':');
                    dialogueText = $"<size=120%>[{relicName} Lv. {ownedRelic.Level + 1}]</size>\n";
                    dialogueText += $"{gradeText}\n{Utils.GetRelicDescription(relic.effectName, values.ToArray())} (MAX)";
                    priceText = "MAX";
                }
            }
            else
            {
                var upgrade = data as UpgradeData;
                dialogueText = $"<size=120%>[{Utils.GetUpgradeName(upgrade.type)}]</size>";
                priceText = "";
                var level = PersistentGameState.Instance.HouseState.GetUpgradeLevel(upgrade.type);
                string valueText;
                if (level < upgrade.maxLevel - 1)
                {
                    valueText = $"{GetValueText(upgrade.type, upgrade.values[level])} -> {GetValueText(upgrade.type, upgrade.values[level + 1])}";
                    dialogueText += $"\n{GetUnlockConditionText(upgrade.type, level + 1)}";
                    priceText = $"{upgrade.prices[level + 1]:n0}G";
                }
                else
                {
                    valueText = GetValueText(upgrade.type, upgrade.values[level]);
                    priceText = "MAX";
                }
                dialogueText += $"\n{Utils.GetUpgradeDescription(upgrade.type, new object[] { valueText })}";
                _shopController.UpdateDialogueText(dialogueText, priceText);
            }

            _shopController.UpdateDialogueText(dialogueText, priceText);
        }

        private string GetValueText(UpgradeType upgradeType, int value)
        {
            if (upgradeType == UpgradeType.InitialRelicSelect)
            {
                if (value < 0)
                {
                    return Utils.GetUIString("locked");
                }
                else
                {
                    return Utils.GetUIString("grade_" + Enum.GetName(typeof(RelicGrade), value).ToLowerInvariant());
                }
            }
            
            return value.ToString();
        }

        private int GetRequiredUnlockedRelicCount(UpgradeType upgradeType, int level)
        {
            switch (upgradeType)
            {
                case UpgradeType.MaxRelicCount:
                    return 5 * level;
                case UpgradeType.RelicRewardChoiceCount:
                    return 10 + 10 * level;
                case UpgradeType.RelicRewardRerollCount:
                    return 5 * level;
                case UpgradeType.RelicRewardRarity:
                    return 15 + 5 * level;
                case UpgradeType.InitialRelicSelect:
                    return 25 + 5 * level;
                default:
                    return 0;
            }
        }

        private string GetUnlockConditionText(UpgradeType upgradeType, int level)
        {
            int count = GetRequiredUnlockedRelicCount(upgradeType, level);
            int myRelicCount = PersistentGameState.Instance.CodexState.GetAvailableRelics().Count;
            if (count <= 0) return "";
            return $"<size=90%><color={(myRelicCount < count ? "red" : "white")}>{Utils.GetUIString("relic-upgrade-condition", new object[] { count })}</color></size>";
        }

        public override void SetShopElement(ShopElement element, int category, object data)
        {
            if (category == 0)
            {
                // Relics
                var relicData = data as RelicData;
                if (relicData.prices.Length > 0)
                {
                    var myRelicLevel = PersistentGameState.Instance.CodexState.GetRelic(relicData.id)?.Level ?? -1;
                    string detailText = myRelicLevel < 0 ?
                        $"{relicData.values[0]} (NEW)" :
                        myRelicLevel < relicData.values.Length - 1 ?
                            $"{relicData.values[myRelicLevel]} -> {relicData.values[myRelicLevel + 1]}" :
                            $"{relicData.values[myRelicLevel]} (MAX)";
                    string priceText = myRelicLevel < relicData.prices.Length - 1 ?
                        $"{relicData.prices[myRelicLevel + 1]:n0}G" :
                        myRelicLevel < relicData.values.Length ?
                            $"({Utils.GetUIString("not-for-sale")})" :
                            "-";
                    element.UpdateElement(
                        relicData.sprite,
                        Utils.GetRelicName(relicData.effectName),
                        detailText,
                        priceText,
                        myRelicLevel < 0 ? "NEW" : "Lv. " + (myRelicLevel + 1));
                }
            }
            else
            {
                var upgrade = data as UpgradeData;
                var upgradeLevel = PersistentGameState.Instance.HouseState.GetUpgradeLevel(upgrade.type);
                string detailText = upgradeLevel < upgrade.maxLevel - 1 ?
                    $"{GetValueText(upgrade.type, upgrade.values[upgradeLevel])} -> {GetValueText(upgrade.type, upgrade.values[upgradeLevel + 1])}" :
                    $"{GetValueText(upgrade.type, upgrade.values[upgradeLevel])} (MAX)";
                string priceText = upgradeLevel < upgrade.maxLevel - 1 ? $"{upgrade.prices[upgradeLevel + 1]:n0}G" : "-";
                element.UpdateElement(
                    null,
                    Utils.GetUpgradeName(upgrade.type),
                    detailText,
                    priceText,
                    "Lv. " + (upgradeLevel + 1));
            }
        }
    }
}