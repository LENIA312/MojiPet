using Mojipet.Models;

namespace Mojipet.Events
{
    public readonly struct OnPetUnlocked
    {
        public readonly int CharacterId;

        public OnPetUnlocked(int characterId)
        {
            CharacterId = characterId;
        }
    }

    public readonly struct OnPetLevelUp
    {
        public readonly int CharacterId;
        public readonly int OldLevel;
        public readonly int NewLevel;

        public OnPetLevelUp(int characterId, int oldLevel, int newLevel)
        {
            CharacterId = characterId;
            OldLevel = oldLevel;
            NewLevel = newLevel;
        }
    }

    public readonly struct OnPetFed
    {
        public readonly int CharacterId;
        public readonly ItemType ItemType;
        public readonly float OldHunger;
        public readonly float NewHunger;

        public OnPetFed(int characterId, ItemType itemType, float oldHunger, float newHunger)
        {
            CharacterId = characterId;
            ItemType = itemType;
            OldHunger = oldHunger;
            NewHunger = newHunger;
        }
    }

    public readonly struct OnPetUpdated
    {
        public readonly int CharacterId;

        public OnPetUpdated(int characterId)
        {
            CharacterId = characterId;
        }
    }

    public readonly struct OnHandwritingSaved
    {
        public readonly int CharacterId;

        public OnHandwritingSaved(int characterId)
        {
            CharacterId = characterId;
        }
    }
}
