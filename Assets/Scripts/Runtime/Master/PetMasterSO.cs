using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mojipet.Master
{
    [Serializable]
    public sealed class PetMasterEntry
    {
        public int CharacterId;
        public string Character;
        public string DisplayName;
        public int InitialLevel;
        public int BaseProduction;
        public float BaseResearchSpeed;
    }

    [CreateAssetMenu(fileName = "PetMaster", menuName = "Mojipet/Master/PetMaster")]
    public sealed class PetMasterSO : ScriptableObject
    {
        [SerializeField] private PetMasterEntry[] _entries = Array.Empty<PetMasterEntry>();

        public IReadOnlyList<PetMasterEntry> Entries => _entries;

        public void SetEntries(PetMasterEntry[] entries)
        {
            _entries = entries ?? Array.Empty<PetMasterEntry>();
        }
    }
}
