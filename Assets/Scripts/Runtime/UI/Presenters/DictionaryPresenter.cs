using System.Collections.Generic;
using Mojipet.Master;
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
        // The dictionary has ~20,000 words (JMDict import); instantiating a UI
        // row per word at once was catastrophically slow to open, so the list is
        // paged and only the current page's rows are ever materialized.
        public const int PageSize = 50;

        // Row index 0 is the "すべて" (all) tab; 1-10 are the gojuon (五十音) rows.
        public static readonly string[] RowLabels =
        {
            "すべて", "あ", "か", "さ", "た", "な", "は", "ま", "や", "ら", "わ"
        };

        // Voiced/semi-voiced and small-kana variants are grouped under their base
        // consonant row, matching how gojuon-order (五十音順) dictionaries sort them.
        private static readonly string[] RowMembers =
        {
            "あいうえおぁぃぅぇぉ",
            "かきくけこがぎぐげご",
            "さしすせそざじずぜぞ",
            "たちつてとだぢづでどっ",
            "なにぬねの",
            "はひふへほばびぶべぼぱぴぷぺぽ",
            "まみむめも",
            "やゆよゃゅょ",
            "らりるれろ",
            "わをんゎ"
        };

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

        public int GetPageCount(int rowIndex)
        {
            var count = GetFilteredWords(rowIndex).Count;
            var pageCount = (count + PageSize - 1) / PageSize;
            return pageCount < 1 ? 1 : pageCount;
        }

        public IReadOnlyList<DictionaryRowData> GetRows(int rowIndex, int page)
        {
            var words = GetFilteredWords(rowIndex);
            var start = page * PageSize;
            if (start < 0 || start >= words.Count)
            {
                return new List<DictionaryRowData>();
            }

            var end = start + PageSize;
            if (end > words.Count)
            {
                end = words.Count;
            }

            var rows = new List<DictionaryRowData>(end - start);
            for (var i = start; i < end; i++)
            {
                var word = words[i];
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

        private IReadOnlyList<WordMasterEntry> GetFilteredWords(int rowIndex)
        {
            var allWords = _wordSystem.GetWords();
            if (rowIndex <= 0)
            {
                return allWords;
            }

            var members = RowMembers[rowIndex - 1];
            var filtered = new List<WordMasterEntry>();
            foreach (var word in allWords)
            {
                if (!string.IsNullOrEmpty(word.Reading) && members.IndexOf(word.Reading[0]) >= 0)
                {
                    filtered.Add(word);
                }
            }

            return filtered;
        }
    }
}
