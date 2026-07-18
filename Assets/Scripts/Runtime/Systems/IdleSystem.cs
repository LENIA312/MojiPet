using System;
using Mojipet.Events;
using Mojipet.Managers;
using Mojipet.Utilities;
using UnityEngine;

namespace Mojipet.Systems
{
    public sealed class IdleSystem
    {
        private readonly SaveSystem _saveSystem;
        private readonly PetSystem _petSystem;
        private readonly ResearchSystem _researchSystem;
        private readonly CurrencySystem _currencySystem;
        private readonly MasterManager _masterManager;
        private readonly EventBus _eventBus;

        private bool _isCalculating;
        private bool _rewardApplied = true;

        public TimeSpan ElapsedTime { get; private set; }
        public long RewardMoney { get; private set; }

        public IdleSystem(
            SaveSystem saveSystem,
            PetSystem petSystem,
            ResearchSystem researchSystem,
            CurrencySystem currencySystem,
            MasterManager masterManager,
            EventBus eventBus)
        {
            _saveSystem = saveSystem ?? throw new ArgumentNullException(nameof(saveSystem));
            _petSystem = petSystem ?? throw new ArgumentNullException(nameof(petSystem));
            _researchSystem = researchSystem ?? throw new ArgumentNullException(nameof(researchSystem));
            _currencySystem = currencySystem ?? throw new ArgumentNullException(nameof(currencySystem));
            _masterManager = masterManager ?? throw new ArgumentNullException(nameof(masterManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void CalculateOfflineProgress()
        {
            if (_isCalculating)
            {
                return;
            }

            _isCalculating = true;
            try
            {
                var currentUtc = TimeUtility.CurrentUtc;
                var lastLoginUtc = _saveSystem.Data.Idle.LastLoginUtc;

                var elapsed = currentUtc - lastLoginUtc;
                if (elapsed < TimeSpan.Zero)
                {
                    elapsed = TimeSpan.Zero;
                }

                var cap = TimeSpan.FromHours(_masterManager.GameBalanceMaster.MaxOfflineHours);
                if (elapsed > cap)
                {
                    elapsed = cap;
                }

                ElapsedTime = elapsed;

                if (elapsed <= TimeSpan.Zero)
                {
                    RewardMoney = 0;
                    _rewardApplied = true;
                    _eventBus.Publish(new OnOfflineSkipped());
                    return;
                }

                _petSystem.UpdateHunger(elapsed);
                _researchSystem.UpdateResearch();

                RewardMoney = _petSystem.CalculateProduction(elapsed);
                if (RewardMoney < 0)
                {
                    RewardMoney = 0;
                }

                _rewardApplied = false;

                _saveSystem.Save();
                _eventBus.Publish(new OnOfflineCalculated(ElapsedTime, RewardMoney));
            }
            catch (Exception e)
            {
                Debug.LogError($"[IdleSystem] CalculateOfflineProgress failed. {e}");
                ElapsedTime = TimeSpan.Zero;
                RewardMoney = 0;
                _rewardApplied = true;
            }
            finally
            {
                _isCalculating = false;
            }
        }

        public TimeSpan GetOfflineTime()
        {
            return ElapsedTime;
        }

        public void ApplyOfflineReward()
        {
            if (_rewardApplied)
            {
                return;
            }

            if (RewardMoney > 0)
            {
                _currencySystem.AddMoney(RewardMoney);
            }

            _rewardApplied = true;
            _saveSystem.Save();
            _eventBus.Publish(new OnOfflineRewardApplied(RewardMoney));
        }

        public void SaveLoginTime()
        {
            _saveSystem.Data.Idle.LastLoginUtc = TimeUtility.CurrentUtc;
            _saveSystem.Save();
        }

        public long GetRewardMoney()
        {
            return RewardMoney;
        }

        public bool HasOfflineReward()
        {
            return !_rewardApplied && RewardMoney > 0;
        }
    }
}
