using System;
using Mojipet.Events;

namespace Mojipet.Systems
{
    public sealed class CurrencySystem
    {
        private readonly SaveSystem _saveSystem;
        private readonly EventBus _eventBus;

        public CurrencySystem(SaveSystem saveSystem, EventBus eventBus)
        {
            _saveSystem = saveSystem ?? throw new ArgumentNullException(nameof(saveSystem));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public long GetMoney()
        {
            return _saveSystem.Data.Currency.Money;
        }

        public void AddMoney(long amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "amount must be positive.");
            }

            var currency = _saveSystem.Data.Currency;
            var newMoney = currency.Money + amount;
            if (newMoney < currency.Money)
            {
                newMoney = long.MaxValue;
            }

            currency.Money = newMoney;
            _saveSystem.Save();

            _eventBus.Publish(new OnMoneyAdded(amount, currency.Money));
            _eventBus.Publish(new OnMoneyChanged(currency.Money));
        }

        public bool ConsumeMoney(long amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "amount must be positive.");
            }

            var currency = _saveSystem.Data.Currency;
            if (!CanConsume(amount))
            {
                _eventBus.Publish(new OnMoneyInsufficient(amount, currency.Money));
                return false;
            }

            currency.Money -= amount;
            _saveSystem.Save();

            _eventBus.Publish(new OnMoneyConsumed(amount, currency.Money));
            _eventBus.Publish(new OnMoneyChanged(currency.Money));
            return true;
        }

        public bool CanConsume(long amount)
        {
            return _saveSystem.Data.Currency.Money >= amount;
        }

        public void SetMoney(long amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "amount must not be negative.");
            }

            _saveSystem.Data.Currency.Money = amount;
            _saveSystem.Save();
            _eventBus.Publish(new OnMoneyChanged(amount));
        }
    }
}
