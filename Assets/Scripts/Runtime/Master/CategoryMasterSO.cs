using System;
using System.Collections.Generic;
using Mojipet.Models;
using UnityEngine;

namespace Mojipet.Master
{
    [Serializable]
    public sealed class CategoryMasterEntry
    {
        public CategoryId Category;
        public int RequiredLibraryLevel;
    }

    [CreateAssetMenu(fileName = "CategoryMaster", menuName = "Mojipet/Master/CategoryMaster")]
    public sealed class CategoryMasterSO : ScriptableObject
    {
        [SerializeField] private CategoryMasterEntry[] _entries = Array.Empty<CategoryMasterEntry>();

        public IReadOnlyList<CategoryMasterEntry> Entries => _entries;

        public void SetEntries(CategoryMasterEntry[] entries)
        {
            _entries = entries ?? Array.Empty<CategoryMasterEntry>();
        }
    }
}
