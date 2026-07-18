using System;
using System.Collections.Generic;
using Mojipet.Events;
using Mojipet.Managers;
using Mojipet.Master;

namespace Mojipet.Systems
{
    public sealed class ShopSystem
    {
        private readonly MasterManager _masterManager;
        private readonly CurrencySystem _currencySystem;
        private readonly ItemSystem _itemSystem;
        private readonly EventBus _eventBus;

        public ShopSystem(MasterManager masterManager, CurrencySystem currencySystem, ItemSystem itemSystem, EventBus eventBus)
        {
            _masterManager = masterManager ?? throw new ArgumentNullException(nameof(masterManager));
            _currencySystem = currencySystem ?? throw new ArgumentNullException(nameof(currencySystem));
            _itemSystem = itemSystem ?? throw new ArgumentNullException(nameof(itemSystem));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public IReadOnlyList<ShopMasterEntry> GetItems()
        {
            return _masterManager.ShopMaster.Entries;
        }

        public bool CanPurchase(int shopEntryId)
        {
            var entry = FindEntry(shopEntryId);
            return entry != null && _currencySystem.CanConsume(entry.Price);
        }

        public bool Purchase(int shopEntryId)
        {
            var entry = FindEntry(shopEntryId);
            if (entry == null)
            {
                throw new ArgumentException($"ShopMaster entry not found: {shopEntryId}", nameof(shopEntryId));
            }

            if (!_currencySystem.ConsumeMoney(entry.Price))
            {
                _eventBus.Publish(new OnPurchaseFailed(entry.Id, "insufficient_money"));
                return false;
            }

            _itemSystem.AddItem(entry.ItemId, 1);
            _eventBus.Publish(new OnItemPurchased(entry.Id, entry.ItemId));
            return true;
        }

        private ShopMasterEntry FindEntry(int shopEntryId)
        {
            foreach (var entry in _masterManager.ShopMaster.Entries)
            {
                if (entry.Id == shopEntryId)
                {
                    return entry;
                }
            }

            return null;
        }
    }
}
