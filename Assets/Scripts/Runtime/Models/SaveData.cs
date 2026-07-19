using System;
using System.Collections.Generic;

namespace Mojipet.Models
{
    public sealed class SaveData
    {
        public int Version { get; set; }
        public DateTime LastSaveUtc { get; set; }
        public CurrencyData Currency { get; set; } = new CurrencyData();
        public List<PetData> Pets { get; set; } = new List<PetData>();
        public List<DictionaryEntryData> Dictionary { get; set; } = new List<DictionaryEntryData>();
        public List<ResearchData> Research { get; set; } = new List<ResearchData>();
        public List<FacilityData> Facilities { get; set; } = new List<FacilityData>();
        public IdleData Idle { get; set; } = new IdleData();
        public InventoryData Inventory { get; set; } = new InventoryData();
        public SettingsData Settings { get; set; } = new SettingsData();
        public DateTime ResearchBoostExpiryUtc { get; set; }
        public float ResearchBoostMultiplier { get; set; } = 1f;
    }
}
