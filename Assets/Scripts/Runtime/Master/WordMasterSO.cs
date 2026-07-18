using System;
using System.Collections.Generic;
using Mojipet.Models;
using UnityEngine;

namespace Mojipet.Master
{
    [Serializable]
    public sealed class WordMasterEntry
    {
        public int WordId;
        public string Word;
        public string Reading;
        public int Length;
        public int Difficulty;
        public CategoryId Category;
        public int RequiredLevel;
        public int ResearchTimeSeconds;
        public string[] Characters;
    }

    [CreateAssetMenu(fileName = "WordMaster", menuName = "Mojipet/Master/WordMaster")]
    public sealed class WordMasterSO : ScriptableObject
    {
        [SerializeField] private WordMasterEntry[] _entries = Array.Empty<WordMasterEntry>();

        public IReadOnlyList<WordMasterEntry> Entries => _entries;

        public void SetEntries(WordMasterEntry[] entries)
        {
            _entries = entries ?? Array.Empty<WordMasterEntry>();
        }
    }
}
