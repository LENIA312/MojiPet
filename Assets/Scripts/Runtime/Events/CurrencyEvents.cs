namespace Mojipet.Events
{
    public readonly struct OnMoneyAdded
    {
        public readonly long AddedAmount;
        public readonly long CurrentMoney;

        public OnMoneyAdded(long addedAmount, long currentMoney)
        {
            AddedAmount = addedAmount;
            CurrentMoney = currentMoney;
        }
    }

    public readonly struct OnMoneyConsumed
    {
        public readonly long ConsumedAmount;
        public readonly long CurrentMoney;

        public OnMoneyConsumed(long consumedAmount, long currentMoney)
        {
            ConsumedAmount = consumedAmount;
            CurrentMoney = currentMoney;
        }
    }

    public readonly struct OnMoneyChanged
    {
        public readonly long CurrentMoney;

        public OnMoneyChanged(long currentMoney)
        {
            CurrentMoney = currentMoney;
        }
    }

    public readonly struct OnMoneyInsufficient
    {
        public readonly long RequiredMoney;
        public readonly long CurrentMoney;

        public OnMoneyInsufficient(long requiredMoney, long currentMoney)
        {
            RequiredMoney = requiredMoney;
            CurrentMoney = currentMoney;
        }
    }
}
