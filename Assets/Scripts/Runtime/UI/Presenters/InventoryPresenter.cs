using System.Collections.Generic;
using Mojipet.Managers;
using Mojipet.Models;
using Mojipet.Systems;

namespace Mojipet.UI.Presenters
{
    public readonly struct InventoryRowData
    {
        public readonly int ItemId;
        public readonly string Name;
        public readonly int Count;
        public readonly bool CanUseDirectly;

        public InventoryRowData(int itemId, string name, int count, bool canUseDirectly)
        {
            ItemId = itemId;
            Name = name;
            Count = count;
            CanUseDirectly = canUseDirectly;
        }
    }

    public sealed class InventoryPresenter
    {
        private readonly ItemSystem _itemSystem;
        private readonly MasterManager _masterManager;

        public InventoryPresenter(ItemSystem itemSystem, MasterManager masterManager)
        {
            _itemSystem = itemSystem;
            _masterManager = masterManager;
        }

        public IReadOnlyList<InventoryRowData> GetRows()
        {
            var result = new List<InventoryRowData>();

            foreach (var pair in _itemSystem.GetAllItems())
            {
                if (pair.Value <= 0)
                {
                    continue;
                }

                var name = $"Item{pair.Key}";
                var canUseDirectly = false;

                foreach (var itemMaster in _masterManager.ItemMaster.Entries)
                {
                    if (itemMaster.Id == pair.Key)
                    {
                        name = itemMaster.Name;
                        canUseDirectly = itemMaster.ItemType == ItemType.Seed;
                        break;
                    }
                }

                result.Add(new InventoryRowData(pair.Key, name, pair.Value, canUseDirectly));
            }

            return result;
        }

        public bool UseDirectly(int itemId)
        {
            return _itemSystem.Use(itemId, 0);
        }
    }
}
