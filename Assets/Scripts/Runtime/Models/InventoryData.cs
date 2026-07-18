using System.Collections.Generic;

namespace Mojipet.Models
{
    public sealed class InventoryData
    {
        public Dictionary<int, int> ItemCounts { get; set; } = new Dictionary<int, int>();
    }
}
