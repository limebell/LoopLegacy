using System;
using LoopLegacy.UI.Component;
using LoopLegacy.UI.Controller;

namespace LoopLegacy.UI.Helper
{
    public abstract class ShopHelper
    {
        protected ShopController _shopController;

        public ShopHelper(ShopController shopController)
        {
            _shopController = shopController;
        }

        public abstract void Initialize();

        public abstract object[] Populate(int category);

        public abstract void SetShopElement(ShopElement element, int category, object data);

        public abstract void Buy(int category, object data, Action onBuy);

        public abstract void Select(int category, object data);
    }
}