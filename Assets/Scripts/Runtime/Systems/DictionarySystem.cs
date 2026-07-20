using System;
using System.Collections.Generic;
using Mojipet.Events;
using Mojipet.Managers;
using Mojipet.Master;
using Mojipet.Models;
using Mojipet.Utilities;

namespace Mojipet.Systems
{
    public sealed class DictionarySystem
    {
        private readonly SaveSystem _saveSystem;
        private readonly WordSystem _wordSystem;
        private readonly MasterManager _masterManager;
        private readonly CurrencySystem _currencySystem;
        private readonly EventBus _eventBus;

        private readonly HashSet<int> _unlockedWordIds = new HashSet<int>();
        private readonly Dictionary<CategoryId, int> _categoryTotalCount = new Dictionary<CategoryId, int>();

        public DictionarySystem(
            SaveSystem saveSystem,
            WordSystem wordSystem,
            MasterManager masterManager,
            CurrencySystem currencySystem,
            EventBus eventBus)
        {
            _saveSystem = saveSystem ?? throw new ArgumentNullException(nameof(saveSystem));
            _wordSystem = wordSystem ?? throw new ArgumentNullException(nameof(wordSystem));
            _masterManager = masterManager ?? throw new ArgumentNullException(nameof(masterManager));
            _currencySystem = currencySystem ?? throw new ArgumentNullException(nameof(currencySystem));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            BuildCaches();
        }

        private void BuildCaches()
        {
            _unlockedWordIds.Clear();
            foreach (var entry in _saveSystem.Data.Dictionary)
            {
                _unlockedWordIds.Add(entry.WordId);
            }

            _categoryTotalCount.Clear();
            foreach (var word in _wordSystem.GetWords())
            {
                _categoryTotalCount.TryGetValue(word.Category, out var count);
                _categoryTotalCount[word.Category] = count + 1;
            }
        }

        public void UnlockWord(int wordId)
        {
            var word = _wordSystem.GetWord(wordId);

            if (_unlockedWordIds.Contains(wordId))
            {
                return;
            }

            _saveSystem.Data.Dictionary.Add(new DictionaryEntryData
            {
                WordId = wordId,
                UnlockedUtc = TimeUtility.CurrentUtc
            });
            _unlockedWordIds.Add(wordId);

            _saveSystem.Save();

            _eventBus.Publish(new OnWordUnlocked(wordId));
            _eventBus.Publish(new OnCompletionUpdated(GetCompletionRate()));
            _eventBus.Publish(new OnCategoryCompletionUpdated(word.Category, GetCategoryCompletionRate(word.Category)));

            CheckMilestones();
        }

        // Breaks the long grind toward 100% completion into chapters: every
        // MilestonePercentStep% crossed pays out a lump-sum bonus, so progress
        // has periodic beats instead of feeling perfectly uniform throughout.
        private void CheckMilestones()
        {
            var step = _masterManager.GameBalanceMaster.MilestonePercentStep;
            if (step <= 0)
            {
                return;
            }

            var percent = (int)(GetCompletionRate() * 100f);
            var currentMilestoneIndex = percent / step;

            while (_saveSystem.Data.HighestMilestoneClaimed < currentMilestoneIndex)
            {
                var nextIndex = _saveSystem.Data.HighestMilestoneClaimed + 1;
                var bonus = (long)_masterManager.GameBalanceMaster.MilestoneBonusPerStep * nextIndex;

                _saveSystem.Data.HighestMilestoneClaimed = nextIndex;
                _currencySystem.AddMoney(bonus);

                _eventBus.Publish(new OnMilestoneReached(nextIndex, nextIndex * step, bonus));
            }
        }

        public bool IsUnlocked(int wordId)
        {
            return _unlockedWordIds.Contains(wordId);
        }

        public IReadOnlyList<DictionaryEntryData> GetDictionary()
        {
            return _saveSystem.Data.Dictionary;
        }

        public IReadOnlyList<WordMasterEntry> GetUnlockedWords()
        {
            var result = new List<WordMasterEntry>(_unlockedWordIds.Count);
            foreach (var word in _wordSystem.GetWords())
            {
                if (_unlockedWordIds.Contains(word.WordId))
                {
                    result.Add(word);
                }
            }

            return result;
        }

        public IReadOnlyList<WordMasterEntry> GetLockedWords()
        {
            var result = new List<WordMasterEntry>();
            foreach (var word in _wordSystem.GetWords())
            {
                if (!_unlockedWordIds.Contains(word.WordId))
                {
                    result.Add(word);
                }
            }

            return result;
        }

        public float GetCompletionRate()
        {
            var total = _wordSystem.GetWords().Count;
            if (total == 0)
            {
                return 0f;
            }

            return (float)_unlockedWordIds.Count / total;
        }

        public float GetCategoryCompletionRate(CategoryId category)
        {
            if (!_categoryTotalCount.TryGetValue(category, out var total) || total == 0)
            {
                return 0f;
            }

            var unlockedInCategory = 0;
            foreach (var word in _wordSystem.GetWordsByCategory(category))
            {
                if (_unlockedWordIds.Contains(word.WordId))
                {
                    unlockedInCategory++;
                }
            }

            return (float)unlockedInCategory / total;
        }

        public int GetUnlockedCount()
        {
            return _unlockedWordIds.Count;
        }

        public int GetTotalWordCount()
        {
            return _wordSystem.GetWords().Count;
        }
    }
}
