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
}
