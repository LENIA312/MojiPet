using System;

namespace Mojipet.Events
{
    public readonly struct OnOfflineCalculated
    {
        public readonly TimeSpan ElapsedTime;
        public readonly long RewardMoney;

        public OnOfflineCalculated(TimeSpan elapsedTime, long rewardMoney)
        {
            ElapsedTime = elapsedTime;
            RewardMoney = rewardMoney;
        }
    }

    public readonly struct OnOfflineRewardApplied
    {
        public readonly long RewardMoney;

        public OnOfflineRewardApplied(long rewardMoney)
        {
            RewardMoney = rewardMoney;
        }
    }

    public readonly struct OnOfflineSkipped
    {
    }
}
