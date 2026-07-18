using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mojipet.Master
{
    [Serializable]
    public sealed class ExpMasterEntry
    {
        public int Level;
        public int RequiredExp;
        public float ProductionMultiplier;
        public float ResearchSpeedMultiplier;
    }

    [CreateAssetMenu(fileName = "ExpMaster", menuName = "Mojipet/Master/ExpMaster")]
    public sealed class ExpMasterSO : ScriptableObject
    {
        [SerializeField] private ExpMasterEntry[] _entries = Array.Empty<ExpMasterEntry>();

        public IReadOnlyList<ExpMasterEntry> Entries => _entries;

        public void SetEntries(ExpMasterEntry[] entries)
        {
            _entries = entries ?? Array.Empty<ExpMasterEntry>();
        }
    }
}
