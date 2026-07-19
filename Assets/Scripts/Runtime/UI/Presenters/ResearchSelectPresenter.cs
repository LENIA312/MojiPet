using System.Collections.Generic;
using Mojipet.Managers;
using Mojipet.Master;
using Mojipet.Models;
using Mojipet.Systems;

namespace Mojipet.UI.Presenters
{
    public sealed class ResearchSelectPresenter
    {
        private readonly WordSystem _wordSystem;
        private readonly DictionarySystem _dictionarySystem;
        private readonly ResearchSystem _researchSystem;
        private readonly PetSystem _petSystem;
        private readonly FacilitySystem _facilitySystem;
        private readonly MasterManager _masterManager;

        public ResearchSelectPresenter(
            WordSystem wordSystem,
            DictionarySystem dictionarySystem,
            ResearchSystem researchSystem,
            PetSystem petSystem,
            FacilitySystem facilitySystem,
            MasterManager masterManager)
        {
            _wordSystem = wordSystem;
            _dictionarySystem = dictionarySystem;
            _researchSystem = researchSystem;
            _petSystem = petSystem;
            _facilitySystem = facilitySystem;
            _masterManager = masterManager;
        }

        public IReadOnlyList<WordMasterEntry> GetCandidates(int characterId)
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

        public bool StartResearch(int characterId, int wordId)
        {
            if (!_researchSystem.CanStartResearch(characterId, wordId))
            {
                return false;
            }

            _researchSystem.StartResearch(characterId, wordId);
            return true;
        }

        public (bool Success, string ErrorMessage) TryStartResearchByReading(int characterId, string reading)
        {
            var word = _wordSystem.FindByReading(reading);
            if (word == null)
            {
                return (false, "その単語は辞書にありません");
            }

            if (_dictionarySystem.IsUnlocked(word.WordId))
            {
                return (false, "すでに理解している単語です");
            }

            if (!_wordSystem.ContainsCharacter(word.WordId, characterId))
            {
                return (false, "この文字を含む単語ではありません");
            }

            var pet = _petSystem.GetPet(characterId);
            if (pet.Level < word.RequiredLevel)
            {
                return (false, $"レベルが足りません（必要Lv{word.RequiredLevel}）");
            }

            var libraryLevel = _facilitySystem.GetLevel(FacilityId.Library);
            if (!IsCategoryUnlocked(word.Category, libraryLevel))
            {
                return (false, "このカテゴリはまだ図書館で解放されていません");
            }

            if (BuildExcludedWordIds().Contains(word.WordId))
            {
                return (false, "他の文字が研究中です");
            }

            if (!StartResearch(characterId, word.WordId))
            {
                return (false, "研究を開始できませんでした");
            }

            return (true, null);
        }

        private HashSet<int> BuildExcludedWordIds()
        {
            var excluded = new HashSet<int>();

            foreach (var entry in _dictionarySystem.GetDictionary())
            {
                excluded.Add(entry.WordId);
            }

            foreach (var research in _researchSystem.GetAllResearch())
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
    }
}
