using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mojipet.Master
{
    [Serializable]
    public sealed class ShopMasterEntry
    {
        public int Id;
        public int ItemId;
        public int Price;
        public string UnlockCondition;
    }

    [CreateAssetMenu(fileName = "ShopMaster", menuName = "Mojipet/Master/ShopMaster")]
    public sealed class ShopMasterSO : ScriptableObject
    {
        [SerializeField] private ShopMasterEntry[] _entries = Array.Empty<ShopMasterEntry>();

        public IReadOnlyList<ShopMasterEntry> Entries => _entries;

        public void SetEntries(ShopMasterEntry[] entries)
        {
            _entries = entries ?? Array.Empty<ShopMasterEntry>();
        }
    }
}
