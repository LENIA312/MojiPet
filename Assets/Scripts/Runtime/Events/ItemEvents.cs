namespace Mojipet.Events
{
    public readonly struct OnItemAdded
    {
        public readonly int ItemId;
        public readonly int NewCount;

        public OnItemAdded(int itemId, int newCount)
        {
            ItemId = itemId;
            NewCount = newCount;
        }
    }

    public readonly struct OnItemRemoved
    {
        public readonly int ItemId;
        public readonly int NewCount;

        public OnItemRemoved(int itemId, int newCount)
        {
            ItemId = itemId;
            NewCount = newCount;
        }
    }

    public readonly struct OnItemUsed
    {
        public readonly int ItemId;
        public readonly int CharacterId;

        public OnItemUsed(int itemId, int characterId)
        {
            ItemId = itemId;
            CharacterId = characterId;
        }
    }
}
