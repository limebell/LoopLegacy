using System;
using System.Collections.Generic;
using LoopLegacy.State;
using LoopLegacy.UI.Component;
using LoopLegacy.UI.Controller;

namespace LoopLegacy.UI.Helper
{
    public class WorkerShopHelper : ShopHelper
    {
        public WorkerShopHelper(ShopController shopController) : base(shopController)
        {
        }

        public override void Buy(int category, object data, Action onBuy)
        {
            Shortcut shortcut = (Shortcut)(byte)data;
            if (PersistentGameState.Instance.HouseState.GetShortcut(shortcut))
            {
                string message = Utils.GetUIString("shortcut_already-opened");
                ConfirmationController.Instance.ShowWarning(message);
                return;
            }
            else if (PersistentGameState.Instance.Gold.Value < GetShortcutPrice(shortcut))
            {
                string message = Utils.GetUIString("not-enough-gold", new object[] { GetShortcutPrice(shortcut).ToString("n0") });
                ConfirmationController.Instance.ShowWarning(message);
                return;
            }
            else
            {
                ConfirmationController.Instance.ShowConfirmation(
                    Utils.GetUIString("shortcut_purchase-confirmation",
                    new object[] { Utils.GetShortcutName(shortcut), GetShortcutPrice(shortcut).ToString("n0") }), () =>
                {
                    PersistentGameState.Instance.SpendGold(GetShortcutPrice(shortcut));
                    PersistentGameState.Instance.HouseState.OpenShortcut(shortcut);
                    PersistentGameState.Instance.SaveState();
                    onBuy?.Invoke();
                });
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
            var shopList = new List<object>();
            var codexState = PersistentGameState.Instance.CodexState;
            if (codexState.IsRegionVisited("desert-0"))
            {
                shopList.Add((byte)Shortcut.Desert);
            }
            if (codexState.IsRegionVisited("deep_forest-0") || codexState.IsRegionVisited("deep_forest-1"))
            {
                shopList.Add((byte)Shortcut.DeepForest);
            }
            if (codexState.IsRegionVisited("castle-0"))
            {
                shopList.Add((byte)Shortcut.Castle);
            }
            return shopList.ToArray();
        }

        public override void Select(int category, object data)
        {
            Shortcut shortcut = (Shortcut)(byte)data;
            string dialogueText = $"<size=120%>[{Utils.GetShortcutName(shortcut)}]</size>";
            string priceText = "";
            if (PersistentGameState.Instance.HouseState.GetShortcut(shortcut))
            {
                dialogueText += $"\n{Utils.GetShortcutDescription(shortcut)}";
                priceText = "-";
            }
            else
            {
                dialogueText += $"\n{Utils.GetShortcutDescription(shortcut)}";
                priceText = $"{GetShortcutPrice(shortcut):n0}G";
            }
            _shopController.UpdateDialogueText(dialogueText, priceText);
        }

        public override void SetShopElement(ShopElement element, int category, object data)
        {
            Shortcut shortcut = (Shortcut)(byte)data;
            string countText = PersistentGameState.Instance.HouseState.GetShortcut(shortcut) ?
                $"{Utils.GetUIString("shortcut_opened")}" :
                $"{Utils.GetUIString("shortcut_not-opened")}";
            string priceText = PersistentGameState.Instance.HouseState.GetShortcut(shortcut) ? "-" : $"{GetShortcutPrice(shortcut):n0}G";
            element.UpdateElement(
                null,
                Utils.GetShortcutName(shortcut),
                "",
                priceText,
                countText);
        }

        private int GetShortcutPrice(Shortcut shortcut)
        {
            switch (shortcut)
            {
                case Shortcut.Desert:
                    return 100000;
                case Shortcut.DeepForest:
                    return 400000;
                case Shortcut.Castle:
                    return 3000000;
                default:
                    Debug.LogError($"[WorkerShopHelper] Unknown shortcut: {shortcut}");
                    return 0;
            }
        }
    }
}