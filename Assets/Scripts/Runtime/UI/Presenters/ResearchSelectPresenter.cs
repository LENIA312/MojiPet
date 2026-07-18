using System.Collections.Generic;
using Mojipet.Master;
using Mojipet.Systems;

namespace Mojipet.UI.Presenters
{
    public sealed class ResearchSelectPresenter
    {
        private readonly WordSystem _wordSystem;
        private readonly DictionarySystem _dictionarySystem;
        private readonly ResearchSystem _researchSystem;
        private readonly PetSystem _petSystem;

        public ResearchSelectPresenter(
            WordSystem wordSystem,
            DictionarySystem dictionarySystem,
            ResearchSystem researchSystem,
            PetSystem petSystem)
        {
            _wordSystem = wordSystem;
            _dictionarySystem = dictionarySystem;
            _researchSystem = researchSystem;
            _petSystem = petSystem;
        }

        public IReadOnlyList<WordMasterEntry> GetCandidates(int characterId)
        {
            var pet = _petSystem.GetPet(characterId);
            var excluded = new HashSet<int>();

            foreach (var entry in _dictionarySystem.GetDictionary())
            {
                excluded.Add(entry.WordId);
            }

            foreach (var research in _researchSystem.GetAllResearch())
            {
                excluded.Add(research.WordId);
            }

            return _wordSystem.GetCandidateWords(characterId, pet.Level, excluded);
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
    }
}
