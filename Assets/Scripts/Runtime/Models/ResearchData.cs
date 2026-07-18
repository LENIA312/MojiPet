using System;

namespace Mojipet.Models
{
    public sealed class ResearchData
    {
        public int CharacterId { get; set; }
        public int WordId { get; set; }
        public ResearchStatus Status { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime FinishUtc { get; set; }
    }
}
