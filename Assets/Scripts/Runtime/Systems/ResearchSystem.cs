using System;
using System.Collections.Generic;
using Mojipet.Events;
using Mojipet.Managers;
using Mojipet.Master;
using Mojipet.Models;
using Mojipet.Utilities;

namespace Mojipet.Systems
{
    public sealed class ResearchSystem
    {
        private readonly SaveSystem _saveSystem;
        private readonly MasterManager _masterManager;
        private readonly WordSystem _wordSystem;
        private readonly PetSystem _petSystem;
        private readonly DictionarySystem _dictionarySystem;
        private readonly FacilitySystem _facilitySystem;
        private readonly EventBus _eventBus;
        private readonly Random _random = new Random();

        private readonly Dictionary<int, ResearchData> _researchByCharacterId = new Dictionary<int, ResearchData>();

        public ResearchSystem(
            SaveSystem saveSystem,
            MasterManager masterManager,
            WordSystem wordSystem,
            PetSystem petSystem,
            DictionarySystem dictionarySystem,
            FacilitySystem facilitySystem,
            EventBus eventBus)
        {
            _saveSystem = saveSystem ?? throw new ArgumentNullException(nameof(saveSystem));
            _masterManager = masterManager ?? throw new ArgumentNullException(nameof(masterManager));
            _wordSystem = wordSystem ?? throw new ArgumentNullException(nameof(wordSystem));
            _petSystem = petSystem ?? throw new ArgumentNullException(nameof(petSystem));
            _dictionarySystem = dictionarySystem ?? throw new ArgumentNullException(nameof(dictionarySystem));
            _facilitySystem = facilitySystem ?? throw new ArgumentNullException(nameof(facilitySystem));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            BuildCache();
        }

        private void BuildCache()
        {
            _researchByCharacterId.Clear();
            foreach (var research in _saveSystem.Data.Research)
            {
                _researchByCharacterId[research.CharacterId] = research;
            }
        }

        public bool IsResearching(int characterId)
        {
            return _researchByCharacterId.ContainsKey(characterId);
        }

        public ResearchData GetResearch(int characterId)
        {
            if (!_researchByCharacterId.TryGetValue(characterId, out var research))
            {
                throw new InvalidOperationException($"No active research for character: {characterId}");
            }

            return research;
        }

        public IReadOnlyList<ResearchData> GetAllResearch()
        {
            return _saveSystem.Data.Research;
        }

        public bool CanStartResearch(int characterId, int wordId)
        {
            if (!_petSystem.IsUnlocked(characterId))
            {
                return false;
            }

            if (IsResearching(characterId))
            {
                return false;
            }

            var pet = _petSystem.GetPet(characterId);
            if (pet.Hunger <= 0f)
            {
                return false;
            }

            if (_dictionarySystem.IsUnlocked(wordId))
            {
                return false;
            }

            if (!_wordSystem.ContainsCharacter(wordId, characterId))
            {
                return false;
            }

            return pet.Level >= _wordSystem.GetRequiredLevel(wordId);
        }

        public void StartResearch(int characterId, int wordId)
        {
            if (!_petSystem.IsUnlocked(characterId))
            {
                throw new InvalidOperationException($"Pet not unlocked: {characterId}");
            }

            if (IsResearching(characterId))
            {
                throw new InvalidOperationException($"Character is already researching: {characterId}");
            }

            var pet = _petSystem.GetPet(characterId);
            if (pet.Hunger <= 0f)
            {
                throw new InvalidOperationException($"Character is starving, cannot start research: {characterId}");
            }

            if (_dictionarySystem.IsUnlocked(wordId))
            {
                throw new InvalidOperationException($"Word already unlocked: {wordId}");
            }

            var word = _wordSystem.GetWord(wordId);

            if (pet.Level < word.RequiredLevel)
            {
                throw new InvalidOperationException($"Pet level too low to research word: {wordId}");
            }

            var speedMultiplier = _petSystem.GetResearchSpeed(characterId);
            if (speedMultiplier <= 0f)
            {
                speedMultiplier = 0.01f;
            }

            var adjustedSeconds = word.ResearchTimeSeconds / speedMultiplier;
            var startUtc = TimeUtility.CurrentUtc;

            var research = new ResearchData
            {
                CharacterId = characterId,
                WordId = wordId,
                Status = ResearchStatus.Researching,
                StartUtc = startUtc,
                FinishUtc = startUtc.AddSeconds(adjustedSeconds)
            };

            _saveSystem.Data.Research.Add(research);
            _researchByCharacterId[characterId] = research;

            _saveSystem.Save();
            _eventBus.Publish(new OnResearchStarted(characterId, wordId));
        }

        public void CancelResearch(int characterId)
        {
            if (!_researchByCharacterId.TryGetValue(characterId, out var research))
            {
                return;
            }

            _saveSystem.Data.Research.Remove(research);
            _researchByCharacterId.Remove(characterId);

            _saveSystem.Save();
            _eventBus.Publish(new OnResearchCanceled(characterId));
        }

        public void UpdateResearch()
        {
            var characterIds = new List<int>(_researchByCharacterId.Keys);
            var now = TimeUtility.CurrentUtc;

            foreach (var characterId in characterIds)
            {
                if (!_researchByCharacterId.TryGetValue(characterId, out var research))
                {
                    continue;
                }

                // Starving characters stop researching entirely (not just slower) --
                // resumes automatically once fed and re-picked by AutoStartResearch.
                if (_petSystem.GetPet(characterId).Hunger <= 0f)
                {
                    CancelResearch(characterId);
                    continue;
                }

                if (now >= research.FinishUtc)
                {
                    CompleteResearch(characterId);
                }
            }

            AutoStartResearchForIdleCharacters();
        }

        // Research has no manual word-selection UI -- each idle, non-starving
        // character autonomously picks a random eligible candidate word and starts
        // researching it, respecting the same level/category/duplicate rules that
        // manual selection used to enforce.
        public void AutoStartResearchForIdleCharacters()
        {
            foreach (var pet in _petSystem.GetAllPets())
            {
                if (IsResearching(pet.CharacterId) || pet.Hunger <= 0f)
                {
                    continue;
                }

                var candidates = GetEligibleCandidates(pet.CharacterId);
                if (candidates.Count == 0)
                {
                    continue;
                }

                var chosen = candidates[_random.Next(candidates.Count)];
                StartResearch(pet.CharacterId, chosen.WordId);
            }
        }

        private List<WordMasterEntry> GetEligibleCandidates(int characterId)
        {
            var pet = _petSystem.GetPet(characterId);
            var excluded = BuildExcludedWordIds();
            var rawCandidates = _wordSystem.GetCandidateWords(characterId, pet.Level, excluded);
            var libraryLevel = _facilitySystem.GetLevel(FacilityId.Library);

            var result = new List<WordMasterEntry>();
            foreach (var word in rawCandidates)
            {
                if (IsCategoryUnlocked(word.Category, libraryLevel))
                {
                    result.Add(word);
                }
            }

            return result;
        }

        private HashSet<int> BuildExcludedWordIds()
        {
            var excluded = new HashSet<int>();

            foreach (var entry in _dictionarySystem.GetDictionary())
            {
                excluded.Add(entry.WordId);
            }

            foreach (var research in _researchByCharacterId.Values)
            {
                excluded.Add(research.WordId);
            }

            return excluded;
        }

        private bool IsCategoryUnlocked(CategoryId category, int libraryLevel)
        {
            foreach (var entry in _masterManager.CategoryMaster.Entries)
            {
                if (entry.Category == category)
                {
                    return libraryLevel >= entry.RequiredLibraryLevel;
                }
            }

            return true;
        }

        public void CompleteResearch(int characterId)
        {
            if (!_researchByCharacterId.TryGetValue(characterId, out var research))
            {
                throw new InvalidOperationException($"No active research for character: {characterId}");
            }

            var word = _wordSystem.GetWord(research.WordId);

            _saveSystem.Data.Research.Remove(research);
            _researchByCharacterId.Remove(characterId);

            _dictionarySystem.UnlockWord(research.WordId);

            var expAmount = CalculateExperience(word);
            foreach (var memberCharacterId in _wordSystem.GetCharacters(research.WordId))
            {
                if (_petSystem.IsUnlocked(memberCharacterId))
                {
                    _petSystem.AddExperience(memberCharacterId, expAmount);
                }
            }

            _saveSystem.Save();
            _eventBus.Publish(new OnResearchCompleted(characterId, research.WordId));
        }

        public TimeSpan GetRemainingTime(int characterId)
        {
            if (!_researchByCharacterId.TryGetValue(characterId, out var research))
            {
                return TimeSpan.Zero;
            }

            var remaining = research.FinishUtc - TimeUtility.CurrentUtc;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        public float GetProgressRate(int characterId)
        {
            if (!_researchByCharacterId.TryGetValue(characterId, out var research))
            {
                return 0f;
            }

            var totalSeconds = (research.FinishUtc - research.StartUtc).TotalSeconds;
            if (totalSeconds <= 0)
            {
                return 1f;
            }

            var elapsedSeconds = (TimeUtility.CurrentUtc - research.StartUtc).TotalSeconds;
            var rate = (float)(elapsedSeconds / totalSeconds);

            if (rate < 0f)
            {
                return 0f;
            }

            if (rate > 1f)
            {
                return 1f;
            }

            return rate;
        }

        private int CalculateExperience(WordMasterEntry word)
        {
            var balance = _masterManager.GameBalanceMaster;
            var lengthBonus = GetLengthBonus(word.Length, balance);
            return (int)(balance.BaseExp * lengthBonus);
        }

        private static float GetLengthBonus(int length, GameBalanceMasterSO balance)
        {
            if (length <= 2)
            {
                return balance.LengthBonusMultiplier2;
            }

            if (length == 3)
            {
                return balance.LengthBonusMultiplier3;
            }

            if (length == 4)
            {
                return balance.LengthBonusMultiplier4;
            }

            return balance.LengthBonusMultiplier5Plus;
        }
    }
}
