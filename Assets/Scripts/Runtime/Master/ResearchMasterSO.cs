using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mojipet.Master
{
    [Serializable]
    public sealed class ResearchMasterEntry
    {
        public int Difficulty;
        public int RequiredSeconds;
    }

    [CreateAssetMenu(fileName = "ResearchMaster", menuName = "Mojipet/Master/ResearchMaster")]
    public sealed class ResearchMasterSO : ScriptableObject
    {
        [SerializeField] private ResearchMasterEntry[] _entries = Array.Empty<ResearchMasterEntry>();

        public IReadOnlyList<ResearchMasterEntry> Entries => _entries;

        public void SetEntries(ResearchMasterEntry[] entries)
        {
            _entries = entries ?? Array.Empty<ResearchMasterEntry>();
        }
    }
}
