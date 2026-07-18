using System.Collections.Generic;
using Mojipet.Managers;
using Mojipet.Systems;

namespace Mojipet.UI.Presenters
{
    public readonly struct ShopRowData
    {
        public readonly int ShopEntryId;
        public readonly string ItemName;
        public readonly int Price;
        public readonly int OwnedCount;
        public readonly bool CanPurchase;

        public ShopRowData(int shopEntryId, string itemName, int price, int ownedCount, bool canPurchase)
        {
            ShopEntryId = shopEntryId;
            ItemName = itemName;
            Price = price;
            OwnedCount = ownedCount;
            CanPurchase = canPurchase;
        }
    }

    public sealed class ShopPresenter
    {
        private readonly ShopSystem _shopSystem;
        private readonly ItemSystem _itemSystem;
        private readonly MasterManager _masterManager;

        public ShopPresenter(ShopSystem shopSystem, ItemSystem itemSystem, MasterManager masterManager)
        {
            _shopSystem = shopSystem;
            _itemSystem = itemSystem;
            _masterManager = masterManager;
        }

        public IReadOnlyList<ShopRowData> GetRows()
        {
            var result = new List<ShopRowData>();

            foreach (var entry in _shopSystem.GetItems())
            {
                var name = $"Item{entry.ItemId}";
                foreach (var itemMaster in _masterManager.ItemMaster.Entries)
                {
                    if (itemMaster.Id == entry.ItemId)
                    {
                        name = itemMaster.Name;
                        break;
                    }
                }

                result.Add(new ShopRowData(
                    entry.Id,
                    name,
                    entry.Price,
                    _itemSystem.GetItemCount(entry.ItemId),
                    _shopSystem.CanPurchase(entry.Id)));
            }

            return result;
        }

        public bool Purchase(int shopEntryId)
        {
            return _shopSystem.Purchase(shopEntryId);
        }
    }
}
