namespace Mojipet.Events
{
    public readonly struct OnResearchStarted
    {
        public readonly int CharacterId;
        public readonly int WordId;

        public OnResearchStarted(int characterId, int wordId)
        {
            CharacterId = characterId;
            WordId = wordId;
        }
    }

    public readonly struct OnResearchCompleted
    {
        public readonly int CharacterId;
        public readonly int WordId;

        public OnResearchCompleted(int characterId, int wordId)
        {
            CharacterId = characterId;
            WordId = wordId;
        }
    }

    public readonly struct OnResearchCanceled
    {
        public readonly int CharacterId;

        public OnResearchCanceled(int characterId)
        {
            CharacterId = characterId;
        }
    }
}
