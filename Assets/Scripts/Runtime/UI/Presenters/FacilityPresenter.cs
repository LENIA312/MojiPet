using System;
using System.Collections.Generic;
using Mojipet.Models;
using Mojipet.Systems;

namespace Mojipet.UI.Presenters
{
    public readonly struct FacilityRowData
    {
        public readonly FacilityId FacilityId;
        public readonly string DisplayName;
        public readonly int Level;
        public readonly float EffectValue;
        public readonly long UpgradeCost;
        public readonly bool CanUpgrade;
        public readonly bool IsMaxLevel;

        public FacilityRowData(
            FacilityId facilityId,
            string displayName,
            int level,
            float effectValue,
            long upgradeCost,
            bool canUpgrade,
            bool isMaxLevel)
        {
            FacilityId = facilityId;
            DisplayName = displayName;
            Level = level;
            EffectValue = effectValue;
            UpgradeCost = upgradeCost;
            CanUpgrade = canUpgrade;
            IsMaxLevel = isMaxLevel;
        }
    }

    public sealed class FacilityPresenter
    {
        private readonly FacilitySystem _facilitySystem;

        public FacilityPresenter(FacilitySystem facilitySystem)
        {
            _facilitySystem = facilitySystem;
        }

        public IReadOnlyList<FacilityRowData> GetRows()
        {
            var result = new List<FacilityRowData>();

            foreach (FacilityId facilityId in Enum.GetValues(typeof(FacilityId)))
            {
                var level = _facilitySystem.GetLevel(facilityId);
                var effect = _facilitySystem.GetEffectValue(facilityId);
                var cost = _facilitySystem.GetUpgradeCost(facilityId);
                var isMaxLevel = cost < 0;
                var canUpgrade = !isMaxLevel && _facilitySystem.CanUpgrade(facilityId);

                result.Add(new FacilityRowData(
                    facilityId,
                    GetDisplayName(facilityId),
                    level,
                    effect,
                    cost,
                    canUpgrade,
                    isMaxLevel));
            }

            return result;
        }

        public bool Upgrade(FacilityId facilityId)
        {
            return _facilitySystem.UpgradeFacility(facilityId);
        }

        private static string GetDisplayName(FacilityId facilityId)
        {
            switch (facilityId)
            {
                case FacilityId.ResearchLab:
                    return "研究所";
                case FacilityId.Library:
                    return "図書館";
                case FacilityId.Garden:
                    return "もじの庭";
                default:
                    return facilityId.ToString();
            }
        }
    }
}
