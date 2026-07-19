using System;
using System.Collections.Generic;
using Mojipet.Events;
using Mojipet.Managers;
using Mojipet.Master;
using Mojipet.Models;

namespace Mojipet.Systems
{
    public sealed class ItemSystem
    {
        private readonly SaveSystem _saveSystem;
        private readonly PetSystem _petSystem;
        private readonly MasterManager _masterManager;
        private readonly EventBus _eventBus;
        private readonly Random _random = new Random();

        public ItemSystem(SaveSystem saveSystem, PetSystem petSystem, MasterManager masterManager, EventBus eventBus)
        {
            _saveSystem = saveSystem ?? throw new ArgumentNullException(nameof(saveSystem));
            _petSystem = petSystem ?? throw new ArgumentNullException(nameof(petSystem));
            _masterManager = masterManager ?? throw new ArgumentNullException(nameof(masterManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public int GetItemCount(int itemId)
        {
            return _saveSystem.Data.Inventory.ItemCounts.TryGetValue(itemId, out var count) ? count : 0;
        }

        public bool HasItem(int itemId)
        {
            return GetItemCount(itemId) > 0;
        }

        public IReadOnlyDictionary<int, int> GetAllItems()
        {
            return _saveSystem.Data.Inventory.ItemCounts;
        }

        public void AddItem(int itemId, int count)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "count must be at least 1.");
            }

            var itemCounts = _saveSystem.Data.Inventory.ItemCounts;
            itemCounts.TryGetValue(itemId, out var current);
            var newCount = current + count;
            itemCounts[itemId] = newCount;

            _saveSystem.Save();
            _eventBus.Publish(new OnItemAdded(itemId, newCount));
        }

        public bool RemoveItem(int itemId, int count)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "count must be at least 1.");
            }

            var itemCounts = _saveSystem.Data.Inventory.ItemCounts;
            if (!itemCounts.TryGetValue(itemId, out var current) || current < count)
            {
                return false;
            }

            var newCount = current - count;
            if (newCount <= 0)
            {
                itemCounts.Remove(itemId);
            }
            else
            {
                itemCounts[itemId] = newCount;
            }

            _saveSystem.Save();
            _eventBus.Publish(new OnItemRemoved(itemId, newCount));
            return true;
        }

        public bool Use(int itemId, int characterId)
        {
            if (!HasItem(itemId))
            {
                return false;
            }

            var itemEntry = FindItemMaster(itemId);

            switch (itemEntry.ItemType)
            {
                case ItemType.Food:
                    _petSystem.Feed(characterId, ItemType.Food);
                    break;

                case ItemType.Seed:
                    UseSeed();
                    break;

                case ItemType.ResearchBoost:
                    _petSystem.ApplyResearchBoost(itemEntry.Value, TimeSpan.FromSeconds(itemEntry.DurationSeconds));
                    break;

                default:
                    throw new NotSupportedException(
                        $"ItemSystem.Use does not yet support ItemType: {itemEntry.ItemType}");
            }

            RemoveItem(itemId, 1);
            _eventBus.Publish(new OnItemUsed(itemId, characterId));
            return true;
        }

        private void UseSeed()
        {
            var candidates = new List<int>();
            foreach (var petMasterEntry in _masterManager.PetMaster.Entries)
            {
                if (!_petSystem.IsUnlocked(petMasterEntry.CharacterId))
                {
                    candidates.Add(petMasterEntry.CharacterId);
                }
            }

            if (candidates.Count == 0)
            {
                throw new InvalidOperationException("All characters are already unlocked.");
            }

            var chosenCharacterId = candidates[_random.Next(candidates.Count)];
            _petSystem.UnlockPet(chosenCharacterId);
        }

        private ItemMasterEntry FindItemMaster(int itemId)
        {
            foreach (var entry in _masterManager.ItemMaster.Entries)
            {
                if (entry.Id == itemId)
                {
                    return entry;
                }
            }

            throw new ArgumentException($"ItemMaster entry not found: {itemId}", nameof(itemId));
        }
    }
}
