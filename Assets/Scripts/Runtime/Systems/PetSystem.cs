using System;
using System.Collections.Generic;
using Mojipet.Events;
using Mojipet.Managers;
using Mojipet.Master;
using Mojipet.Models;

namespace Mojipet.Systems
{
    public sealed class PetSystem
    {
        private readonly SaveSystem _saveSystem;
        private readonly MasterManager _masterManager;
        private readonly FacilitySystem _facilitySystem;
        private readonly EventBus _eventBus;

        private readonly Dictionary<int, PetData> _petsByCharacterId = new Dictionary<int, PetData>();
        private readonly Dictionary<int, PetMasterEntry> _petMasterByCharacterId = new Dictionary<int, PetMasterEntry>();
        private readonly Dictionary<int, ExpMasterEntry> _expMasterByLevel = new Dictionary<int, ExpMasterEntry>();
        private readonly Dictionary<ItemType, ItemMasterEntry> _itemMasterByType = new Dictionary<ItemType, ItemMasterEntry>();

        public PetSystem(SaveSystem saveSystem, MasterManager masterManager, FacilitySystem facilitySystem, EventBus eventBus)
        {
            _saveSystem = saveSystem ?? throw new ArgumentNullException(nameof(saveSystem));
            _masterManager = masterManager ?? throw new ArgumentNullException(nameof(masterManager));
            _facilitySystem = facilitySystem ?? throw new ArgumentNullException(nameof(facilitySystem));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            BuildCaches();
        }

        private void BuildCaches()
        {
            _petsByCharacterId.Clear();
            foreach (var pet in _saveSystem.Data.Pets)
            {
                _petsByCharacterId[pet.CharacterId] = pet;
            }

            _petMasterByCharacterId.Clear();
            foreach (var entry in _masterManager.PetMaster.Entries)
            {
                _petMasterByCharacterId[entry.CharacterId] = entry;
            }

            _expMasterByLevel.Clear();
            foreach (var entry in _masterManager.ExpMaster.Entries)
            {
                _expMasterByLevel[entry.Level] = entry;
            }

            _itemMasterByType.Clear();
            foreach (var entry in _masterManager.ItemMaster.Entries)
            {
                if (!_itemMasterByType.ContainsKey(entry.ItemType))
                {
                    _itemMasterByType[entry.ItemType] = entry;
                }
            }
        }

        public PetData GetPet(int characterId)
        {
            if (!_petsByCharacterId.TryGetValue(characterId, out var pet))
            {
                throw new ArgumentException($"Pet not found or not unlocked: {characterId}", nameof(characterId));
            }

            return pet;
        }

        public IReadOnlyList<PetData> GetAllPets()
        {
            return _saveSystem.Data.Pets;
        }

        public bool IsUnlocked(int characterId)
        {
            return _petsByCharacterId.ContainsKey(characterId);
        }

        public void UnlockPet(int characterId)
        {
            if (_petsByCharacterId.ContainsKey(characterId))
            {
                throw new InvalidOperationException($"Pet already unlocked: {characterId}");
            }

            if (!_petMasterByCharacterId.TryGetValue(characterId, out var masterEntry))
            {
                throw new ArgumentException($"PetMaster entry not found: {characterId}", nameof(characterId));
            }

            var pet = new PetData
            {
                CharacterId = characterId,
                Level = Math.Max(1, masterEntry.InitialLevel),
                Exp = 0,
                Hunger = 100f,
                Unlocked = true
            };

            _saveSystem.Data.Pets.Add(pet);
            _petsByCharacterId[characterId] = pet;

            _saveSystem.Save();
            _eventBus.Publish(new OnPetUnlocked(characterId));
        }

        public void AddExperience(int characterId, int amount)
        {
            if (amount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "amount must be at least 1.");
            }

            var pet = GetPet(characterId);
            var maxLevel = _masterManager.GameBalanceMaster.MaxPetLevel;

            if (pet.Level >= maxLevel)
            {
                return;
            }

            var oldLevel = pet.Level;
            pet.Exp += amount;

            while (pet.Level < maxLevel
                   && _expMasterByLevel.TryGetValue(pet.Level + 1, out var nextLevelEntry)
                   && pet.Exp >= nextLevelEntry.RequiredExp)
            {
                pet.Level++;
            }

            _saveSystem.Save();

            if (pet.Level != oldLevel)
            {
                _eventBus.Publish(new OnPetLevelUp(characterId, oldLevel, pet.Level));
            }

            _eventBus.Publish(new OnPetUpdated(characterId));
        }

        public bool Feed(int characterId, ItemType itemType)
        {
            var pet = GetPet(characterId);

            if (!_itemMasterByType.TryGetValue(itemType, out var itemEntry))
            {
                throw new InvalidOperationException($"ItemMaster entry not found for type: {itemType}");
            }

            var oldHunger = pet.Hunger;
            pet.Hunger = ClampHunger(pet.Hunger + itemEntry.Value);

            _saveSystem.Save();
            _eventBus.Publish(new OnPetFed(characterId, itemType, oldHunger, pet.Hunger));
            return true;
        }

        public void UpdateHunger(TimeSpan elapsed)
        {
            if (elapsed <= TimeSpan.Zero)
            {
                return;
            }

            var decayPerHour = _masterManager.GameBalanceMaster.HungerDecayPerHour;
            var decay = decayPerHour * (float)elapsed.TotalHours;

            foreach (var pet in _saveSystem.Data.Pets)
            {
                pet.Hunger = ClampHunger(pet.Hunger - decay);
            }
        }

        public long GetProductionRate(int characterId)
        {
            var pet = GetPet(characterId);
            var petMaster = _petMasterByCharacterId[characterId];
            var expEntry = GetExpEntry(pet.Level);
            var hungerMultiplier = GetHungerMultiplier(pet.Hunger);
            var facilityMultiplier = _facilitySystem.GetEffectValue(FacilityId.Garden);

            var rate = petMaster.BaseProduction * expEntry.ProductionMultiplier * hungerMultiplier * facilityMultiplier;
            return (long)rate;
        }

        public float GetResearchSpeed(int characterId)
        {
            var pet = GetPet(characterId);
            var petMaster = _petMasterByCharacterId[characterId];
            var expEntry = GetExpEntry(pet.Level);
            var hungerMultiplier = GetHungerMultiplier(pet.Hunger);
            var facilityMultiplier = _facilitySystem.GetEffectValue(FacilityId.ResearchLab);

            return petMaster.BaseResearchSpeed * expEntry.ResearchSpeedMultiplier * hungerMultiplier * facilityMultiplier;
        }

        public long CalculateProduction(TimeSpan elapsed)
        {
            if (elapsed <= TimeSpan.Zero)
            {
                return 0;
            }

            var seconds = elapsed.TotalSeconds;
            long total = 0;
            foreach (var pet in _saveSystem.Data.Pets)
            {
                total += (long)(GetProductionRate(pet.CharacterId) * seconds);
            }

            return total;
        }

        public bool CanLevelUp(int characterId)
        {
            var pet = GetPet(characterId);
            var maxLevel = _masterManager.GameBalanceMaster.MaxPetLevel;

            if (pet.Level >= maxLevel)
            {
                return false;
            }

            return _expMasterByLevel.TryGetValue(pet.Level + 1, out var nextLevelEntry)
                   && pet.Exp >= nextLevelEntry.RequiredExp;
        }

        private ExpMasterEntry GetExpEntry(int level)
        {
            if (_expMasterByLevel.TryGetValue(level, out var entry))
            {
                return entry;
            }

            throw new InvalidOperationException($"ExpMaster entry not found for level: {level}");
        }

        private float GetHungerMultiplier(float hunger)
        {
            var balance = _masterManager.GameBalanceMaster;

            if (hunger <= 0f)
            {
                return balance.HungerStarvingMultiplier;
            }

            if (hunger < balance.HungerLowThreshold)
            {
                return balance.HungerLowMultiplier;
            }

            return 1f;
        }

        private float ClampHunger(float hunger)
        {
            var max = _masterManager.GameBalanceMaster.MaxFood;
            if (hunger < 0f)
            {
                return 0f;
            }

            if (hunger > max)
            {
                return max;
            }

            return hunger;
        }
    }
}
