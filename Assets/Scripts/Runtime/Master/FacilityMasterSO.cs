using System;
using System.Collections.Generic;
using Mojipet.Models;
using UnityEngine;

namespace Mojipet.Master
{
    [Serializable]
    public sealed class FacilityMasterEntry
    {
        public int Id;
        public FacilityId FacilityType;
        public int Level;
        public long UpgradeCost;
        public float EffectValue;
    }

    [CreateAssetMenu(fileName = "FacilityMaster", menuName = "Mojipet/Master/FacilityMaster")]
    public sealed class FacilityMasterSO : ScriptableObject
    {
        [SerializeField] private FacilityMasterEntry[] _entries = Array.Empty<FacilityMasterEntry>();

        public IReadOnlyList<FacilityMasterEntry> Entries => _entries;

        public void SetEntries(FacilityMasterEntry[] entries)
        {
            _entries = entries ?? Array.Empty<FacilityMasterEntry>();
        }
    }
}
