using Mojipet.Models;

namespace Mojipet.Events
{
    public readonly struct OnWordUnlocked
    {
        public readonly int WordId;

        public OnWordUnlocked(int wordId)
        {
            WordId = wordId;
        }
    }

    public readonly struct OnCompletionUpdated
    {
        public readonly float CompletionRate;

        public OnCompletionUpdated(float completionRate)
        {
            CompletionRate = completionRate;
        }
    }

    public readonly struct OnCategoryCompletionUpdated
    {
        public readonly CategoryId Category;
        public readonly float CompletionRate;

        public OnCategoryCompletionUpdated(CategoryId category, float completionRate)
        {
            Category = category;
            CompletionRate = completionRate;
        }
    }

    // Fired when GetCompletionRate() crosses a new GameBalanceMaster.MilestonePercentStep
    // threshold (e.g. every 5%). MilestoneIndex is 1-based (1 = first threshold crossed).
    public readonly struct OnMilestoneReached
    {
        public readonly int MilestoneIndex;
        public readonly int PercentReached;
        public readonly long BonusMoney;

        public OnMilestoneReached(int milestoneIndex, int percentReached, long bonusMoney)
        {
            MilestoneIndex = milestoneIndex;
            PercentReached = percentReached;
            BonusMoney = bonusMoney;
        }
    }
}
