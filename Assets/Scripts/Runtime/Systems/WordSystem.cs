using System;
using System.Collections.Generic;
using Mojipet.Managers;
using Mojipet.Master;
using Mojipet.Models;

namespace Mojipet.Systems
{
    public sealed class WordSystem
    {
        private readonly MasterManager _masterManager;
        private readonly Random _random = new Random();

        private readonly Dictionary<int, WordMasterEntry> _wordsById = new Dictionary<int, WordMasterEntry>();
        private readonly Dictionary<int, List<WordMasterEntry>> _wordsByCharacterId = new Dictionary<int, List<WordMasterEntry>>();
        private readonly Dictionary<CategoryId, List<WordMasterEntry>> _wordsByCategory = new Dictionary<CategoryId, List<WordMasterEntry>>();
        private readonly Dictionary<string, int> _characterIdByCharacter = new Dictionary<string, int>();

        public WordSystem(MasterManager masterManager)
        {
            _masterManager = masterManager ?? throw new ArgumentNullException(nameof(masterManager));
            BuildCaches();
        }

        private void BuildCaches()
        {
            _characterIdByCharacter.Clear();
            foreach (var petEntry in _masterManager.PetMaster.Entries)
            {
                _characterIdByCharacter[petEntry.Character] = petEntry.CharacterId;
            }

            _wordsById.Clear();
            _wordsByCharacterId.Clear();
            _wordsByCategory.Clear();

            foreach (var word in _masterManager.WordMaster.Entries)
            {
                _wordsById[word.WordId] = word;

                if (!_wordsByCategory.TryGetValue(word.Category, out var categoryList))
                {
                    categoryList = new List<WordMasterEntry>();
                    _wordsByCategory[word.Category] = categoryList;
                }

                categoryList.Add(word);

                foreach (var character in word.Characters)
                {
                    if (!_characterIdByCharacter.TryGetValue(character, out var characterId))
                    {
                        continue;
                    }

                    if (!_wordsByCharacterId.TryGetValue(characterId, out var charList))
                    {
                        charList = new List<WordMasterEntry>();
                        _wordsByCharacterId[characterId] = charList;
                    }

                    charList.Add(word);
                }
            }
        }

        public WordMasterEntry GetWord(int wordId)
        {
            if (!_wordsById.TryGetValue(wordId, out var word))
            {
                throw new ArgumentException($"WordId not found: {wordId}", nameof(wordId));
            }

            return word;
        }

        public IReadOnlyList<WordMasterEntry> GetWords()
        {
            return _masterManager.WordMaster.Entries;
        }

        public IReadOnlyList<WordMasterEntry> GetWordsByCharacter(int characterId)
        {
            return _wordsByCharacterId.TryGetValue(characterId, out var list)
                ? list
                : Array.Empty<WordMasterEntry>();
        }

        public IReadOnlyList<WordMasterEntry> GetWordsByCategory(CategoryId category)
        {
            return _wordsByCategory.TryGetValue(category, out var list)
                ? list
                : Array.Empty<WordMasterEntry>();
        }

        public IReadOnlyList<WordMasterEntry> GetWordsByDifficulty(int difficulty)
        {
            var result = new List<WordMasterEntry>();
            foreach (var word in _masterManager.WordMaster.Entries)
            {
                if (word.Difficulty == difficulty)
                {
                    result.Add(word);
                }
            }

            return result;
        }

        public TimeSpan GetResearchTime(int wordId)
        {
            return TimeSpan.FromSeconds(GetWord(wordId).ResearchTimeSeconds);
        }

        public int GetDifficulty(int wordId)
        {
            return GetWord(wordId).Difficulty;
        }

        public int GetRequiredLevel(int wordId)
        {
            return GetWord(wordId).RequiredLevel;
        }

        public CategoryId GetCategory(int wordId)
        {
            return GetWord(wordId).Category;
        }

        public IReadOnlyList<int> GetCharacters(int wordId)
        {
            var word = GetWord(wordId);
            var result = new List<int>(word.Characters.Length);
            foreach (var character in word.Characters)
            {
                if (_characterIdByCharacter.TryGetValue(character, out var characterId))
                {
                    result.Add(characterId);
                }
            }

            return result;
        }

        public bool ContainsCharacter(int wordId, int characterId)
        {
            var characters = GetCharacters(wordId);
            for (var i = 0; i < characters.Count; i++)
            {
                if (characters[i] == characterId)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsLevelUnlocked(int wordId, int petLevel)
        {
            return petLevel >= GetRequiredLevel(wordId);
        }

        public List<WordMasterEntry> GetCandidateWords(int characterId, int petLevel, HashSet<int> excludedWordIds)
        {
            var candidates = new List<WordMasterEntry>();
            var words = GetWordsByCharacter(characterId);

            for (var i = 0; i < words.Count; i++)
            {
                var word = words[i];
                if (petLevel < word.RequiredLevel)
                {
                    continue;
                }

                if (excludedWordIds != null && excludedWordIds.Contains(word.WordId))
                {
                    continue;
                }

                candidates.Add(word);
            }

            return candidates;
        }

        public WordMasterEntry SelectRandomWord(IReadOnlyList<WordMasterEntry> candidates)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }

            var index = _random.Next(candidates.Count);
            return candidates[index];
        }
    }
}
