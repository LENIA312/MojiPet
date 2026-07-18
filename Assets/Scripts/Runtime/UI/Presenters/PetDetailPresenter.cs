using Mojipet.Managers;
using Mojipet.Models;
using Mojipet.Systems;

namespace Mojipet.UI.Presenters
{
    public readonly struct PetDetailData
    {
        public readonly int CharacterId;
        public readonly string Character;
        public readonly int Level;
        public readonly int Exp;
        public readonly int RequiredExp;
        public readonly float Hunger;
        public readonly long ProductionRate;
        public readonly bool IsResearching;
        public readonly string ResearchingWordDisplay;
        public readonly float ResearchProgress;

        public PetDetailData(
            int characterId,
            string character,
            int level,
            int exp,
            int requiredExp,
            float hunger,
            long productionRate,
            bool isResearching,
            string researchingWordDisplay,
            float researchProgress)
        {
            CharacterId = characterId;
            Character = character;
            Level = level;
            Exp = exp;
            RequiredExp = requiredExp;
            Hunger = hunger;
            ProductionRate = productionRate;
            IsResearching = isResearching;
            ResearchingWordDisplay = researchingWordDisplay;
            ResearchProgress = researchProgress;
        }
    }

    public sealed class PetDetailPresenter
    {
        private readonly PetSystem _petSystem;
        private readonly ItemSystem _itemSystem;
        private readonly MasterManager _masterManager;
        private readonly ResearchSystem _researchSystem;
        private readonly WordSystem _wordSystem;

        public PetDetailPresenter(
            PetSystem petSystem,
            ItemSystem itemSystem,
            MasterManager masterManager,
            ResearchSystem researchSystem,
            WordSystem wordSystem)
        {
            _petSystem = petSystem;
            _itemSystem = itemSystem;
            _masterManager = masterManager;
            _researchSystem = researchSystem;
            _wordSystem = wordSystem;
        }

        public PetDetailData GetData(int characterId)
        {
            var pet = _petSystem.GetPet(characterId);

            var character = "?";
            foreach (var entry in _masterManager.PetMaster.Entries)
            {
                if (entry.CharacterId == characterId)
                {
                    character = entry.Character;
                    break;
                }
            }

            var requiredExp = -1;
            foreach (var entry in _masterManager.ExpMaster.Entries)
            {
                if (entry.Level == pet.Level + 1)
                {
                    requiredExp = entry.RequiredExp;
                    break;
                }
            }

            var isResearching = _researchSystem.IsResearching(characterId);
            string researchingWordDisplay = null;
            var progress = 0f;

            if (isResearching)
            {
                var research = _researchSystem.GetResearch(characterId);
                var word = _wordSystem.GetWord(research.WordId);
                researchingWordDisplay = word.Word;
                progress = _researchSystem.GetProgressRate(characterId);
            }

            return new PetDetailData(
                characterId,
                character,
                pet.Level,
                pet.Exp,
                requiredExp,
                pet.Hunger,
                _petSystem.GetProductionRate(characterId),
                isResearching,
                researchingWordDisplay,
                progress);
        }

        public bool Feed(int characterId)
        {
            foreach (var entry in _masterManager.ItemMaster.Entries)
            {
                if (entry.ItemType == ItemType.Food && _itemSystem.HasItem(entry.Id))
                {
                    return _itemSystem.Use(entry.Id, characterId);
                }
            }

            return false;
        }

        public void CancelResearch(int characterId)
        {
            _researchSystem.CancelResearch(characterId);
        }
    }
}
