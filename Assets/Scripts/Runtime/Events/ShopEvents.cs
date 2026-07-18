namespace Mojipet.Events
{
    public readonly struct OnItemPurchased
    {
        public readonly int ShopEntryId;
        public readonly int ItemId;

        public OnItemPurchased(int shopEntryId, int itemId)
        {
            ShopEntryId = shopEntryId;
            ItemId = itemId;
        }
    }

    public readonly struct OnPurchaseFailed
    {
        public readonly int ShopEntryId;
        public readonly string Reason;

        public OnPurchaseFailed(int shopEntryId, string reason)
        {
            ShopEntryId = shopEntryId;
            Reason = reason;
        }
    }
}
