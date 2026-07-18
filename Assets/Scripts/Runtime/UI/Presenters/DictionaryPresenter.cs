using System.Collections.Generic;
using Mojipet.Models;
using Mojipet.Systems;

namespace Mojipet.UI.Presenters
{
    public readonly struct DictionaryRowData
    {
        public readonly int WordId;
        public readonly string DisplayWord;
        public readonly string Reading;
        public readonly CategoryId Category;
        public readonly int Difficulty;
        public readonly bool Unlocked;

        public DictionaryRowData(
            int wordId,
            string displayWord,
            string reading,
            CategoryId category,
            int difficulty,
            bool unlocked)
        {
            WordId = wordId;
            DisplayWord = displayWord;
            Reading = reading;
            Category = category;
            Difficulty = difficulty;
            Unlocked = unlocked;
        }
    }

    public sealed class DictionaryPresenter
    {
        private readonly DictionarySystem _dictionarySystem;
        private readonly WordSystem _wordSystem;

        public DictionaryPresenter(DictionarySystem dictionarySystem, WordSystem wordSystem)
        {
            _dictionarySystem = dictionarySystem;
            _wordSystem = wordSystem;
        }

        public float GetCompletionRate()
        {
            return _dictionarySystem.GetCompletionRate();
        }

        public int GetUnlockedCount()
        {
            return _dictionarySystem.GetUnlockedCount();
        }

        public int GetTotalCount()
        {
            return _dictionarySystem.GetTotalWordCount();
        }

        public IReadOnlyList<DictionaryRowData> GetRows()
        {
            var words = _wordSystem.GetWords();
            var rows = new List<DictionaryRowData>(words.Count);

            foreach (var word in words)
            {
                var unlocked = _dictionarySystem.IsUnlocked(word.WordId);
                rows.Add(new DictionaryRowData(
                    word.WordId,
                    unlocked ? word.Word : "？？？",
                    unlocked ? word.Reading : string.Empty,
                    word.Category,
                    word.Difficulty,
                    unlocked));
            }

            return rows;
        }
    }
}
