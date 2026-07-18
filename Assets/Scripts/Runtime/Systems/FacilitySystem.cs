using System;
using System.Collections.Generic;
using Mojipet.Events;
using Mojipet.Managers;
using Mojipet.Master;
using Mojipet.Models;

namespace Mojipet.Systems
{
    public sealed class FacilitySystem
    {
        private readonly SaveSystem _saveSystem;
        private readonly CurrencySystem _currencySystem;
        private readonly EventBus _eventBus;

        private readonly Dictionary<FacilityId, FacilityData> _facilitiesById = new Dictionary<FacilityId, FacilityData>();
        private readonly Dictionary<FacilityId, Dictionary<int, FacilityMasterEntry>> _masterByFacilityAndLevel =
            new Dictionary<FacilityId, Dictionary<int, FacilityMasterEntry>>();

        public FacilitySystem(SaveSystem saveSystem, MasterManager masterManager, CurrencySystem currencySystem, EventBus eventBus)
        {
            _saveSystem = saveSystem ?? throw new ArgumentNullException(nameof(saveSystem));
            _currencySystem = currencySystem ?? throw new ArgumentNullException(nameof(currencySystem));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            if (masterManager == null)
            {
                throw new ArgumentNullException(nameof(masterManager));
            }

            BuildCaches(masterManager);
        }

        private void BuildCaches(MasterManager masterManager)
        {
            _facilitiesById.Clear();
            foreach (var facility in _saveSystem.Data.Facilities)
            {
                _facilitiesById[facility.FacilityId] = facility;
            }

            _masterByFacilityAndLevel.Clear();
            foreach (var entry in masterManager.FacilityMaster.Entries)
            {
                if (!_masterByFacilityAndLevel.TryGetValue(entry.FacilityType, out var levelMap))
                {
                    levelMap = new Dictionary<int, FacilityMasterEntry>();
                    _masterByFacilityAndLevel[entry.FacilityType] = levelMap;
                }

                levelMap[entry.Level] = entry;
            }
        }

        public int GetLevel(FacilityId facilityId)
        {
            return GetOrCreateFacilityData(facilityId).Level;
        }

        public long GetUpgradeCost(FacilityId facilityId)
        {
            var nextLevel = GetLevel(facilityId) + 1;
            return TryGetMasterEntry(facilityId, nextLevel, out var entry) ? entry.UpgradeCost : -1L;
        }

        public float GetEffectValue(FacilityId facilityId)
        {
            var level = GetLevel(facilityId);
            return TryGetMasterEntry(facilityId, level, out var entry) ? entry.EffectValue : 1f;
        }

        public bool CanUpgrade(FacilityId facilityId)
        {
            var nextLevel = GetLevel(facilityId) + 1;
            if (!TryGetMasterEntry(facilityId, nextLevel, out var entry))
            {
                return false;
            }

            return _currencySystem.CanConsume(entry.UpgradeCost);
        }

        public bool UpgradeFacility(FacilityId facilityId)
        {
            var facility = GetOrCreateFacilityData(facilityId);
            var nextLevel = facility.Level + 1;

            if (!TryGetMasterEntry(facilityId, nextLevel, out var entry))
            {
                return false;
            }

            if (!_currencySystem.ConsumeMoney(entry.UpgradeCost))
            {
                return false;
            }

            facility.Level = nextLevel;
            _saveSystem.Save();
            _eventBus.Publish(new OnFacilityUpgraded(facilityId, nextLevel));

            if (!TryGetMasterEntry(facilityId, nextLevel + 1, out _))
            {
                _eventBus.Publish(new OnFacilityMaxLevel(facilityId));
            }

            return true;
        }

        public IReadOnlyList<FacilityData> GetAllFacilities()
        {
            foreach (FacilityId facilityId in Enum.GetValues(typeof(FacilityId)))
            {
                GetOrCreateFacilityData(facilityId);
            }

            return _saveSystem.Data.Facilities;
        }

        private FacilityData GetOrCreateFacilityData(FacilityId facilityId)
        {
            if (_facilitiesById.TryGetValue(facilityId, out var data))
            {
                return data;
            }

            data = new FacilityData { FacilityId = facilityId, Level = 1 };
            _saveSystem.Data.Facilities.Add(data);
            _facilitiesById[facilityId] = data;
            return data;
        }

        private bool TryGetMasterEntry(FacilityId facilityId, int level, out FacilityMasterEntry entry)
        {
            entry = null;
            return _masterByFacilityAndLevel.TryGetValue(facilityId, out var levelMap) && levelMap.TryGetValue(level, out entry);
        }
    }
}
