using System;

namespace Mojipet.Models
{
    public sealed class PetData
    {
        public int CharacterId { get; set; }
        public int Level { get; set; }
        public int Exp { get; set; }
        public float Hunger { get; set; }
        public bool Unlocked { get; set; }
        public bool HasHandwriting { get; set; }
        public float CheerMultiplier { get; set; } = 1f;
        public DateTime CheerExpiryUtc { get; set; }
    }
}
