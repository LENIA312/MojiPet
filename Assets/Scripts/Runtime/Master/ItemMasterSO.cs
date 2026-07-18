using System;
using System.Collections.Generic;
using Mojipet.Models;
using UnityEngine;

namespace Mojipet.Master
{
    [Serializable]
    public sealed class ItemMasterEntry
    {
        public int Id;
        public string Name;
        public string Description;
        public ItemType ItemType;
        public int Price;
        public float Value;
    }

    [CreateAssetMenu(fileName = "ItemMaster", menuName = "Mojipet/Master/ItemMaster")]
    public sealed class ItemMasterSO : ScriptableObject
    {
        [SerializeField] private ItemMasterEntry[] _entries = Array.Empty<ItemMasterEntry>();

        public IReadOnlyList<ItemMasterEntry> Entries => _entries;

        public void SetEntries(ItemMasterEntry[] entries)
        {
            _entries = entries ?? Array.Empty<ItemMasterEntry>();
        }
    }
}
