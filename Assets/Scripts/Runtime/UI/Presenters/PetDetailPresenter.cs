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
        public readonly bool IsResearchBoostActive;
        public readonly System.TimeSpan ResearchBoostRemaining;
        public readonly bool IsCheerActive;
        public readonly System.TimeSpan CheerRemaining;
        public readonly int CheerCost;
        public readonly bool CanAffordCheer;
        public readonly bool CanStroke;
        public readonly System.TimeSpan StrokeCooldownRemaining;

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
            float researchProgress,
            bool isResearchBoostActive,
            System.TimeSpan researchBoostRemaining,
            bool isCheerActive,
            System.TimeSpan cheerRemaining,
            int cheerCost,
            bool canAffordCheer,
            bool canStroke,
            System.TimeSpan strokeCooldownRemaining)
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
            IsResearchBoostActive = isResearchBoostActive;
            ResearchBoostRemaining = researchBoostRemaining;
            IsCheerActive = isCheerActive;
            CheerRemaining = cheerRemaining;
            CheerCost = cheerCost;
            CanAffordCheer = canAffordCheer;
            CanStroke = canStroke;
            StrokeCooldownRemaining = strokeCooldownRemaining;
        }
    }

    public sealed class PetDetailPresenter
    {
        private readonly PetSystem _petSystem;
        private readonly ItemSystem _itemSystem;
        private readonly MasterManager _masterManager;
        private readonly ResearchSystem _researchSystem;
        private readonly WordSystem _wordSystem;
        private readonly CurrencySystem _currencySystem;

        public PetDetailPresenter(
            PetSystem petSystem,
            ItemSystem itemSystem,
            MasterManager masterManager,
            ResearchSystem researchSystem,
            WordSystem wordSystem,
            CurrencySystem currencySystem)
        {
            _petSystem = petSystem;
            _itemSystem = itemSystem;
            _masterManager = masterManager;
            _researchSystem = researchSystem;
            _wordSystem = wordSystem;
            _currencySystem = currencySystem;
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
                progress,
                _petSystem.IsResearchBoostActive(),
                _petSystem.GetResearchBoostRemaining(),
                _petSystem.IsCheerActive(characterId),
                _petSystem.GetCheerRemaining(characterId),
                _masterManager.GameBalanceMaster.CheerCost,
                _currencySystem.CanConsume(_masterManager.GameBalanceMaster.CheerCost),
                _petSystem.CanStroke(characterId),
                _petSystem.GetStrokeCooldownRemaining(characterId));
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

        public bool Cheer(int characterId)
        {
            return _petSystem.Cheer(characterId);
        }

        public bool Stroke(int characterId)
        {
            return _petSystem.Stroke(characterId);
        }
    }
}
