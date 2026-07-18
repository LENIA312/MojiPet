using Mojipet.Models;
using Newtonsoft.Json;

namespace Mojipet.Save
{
    public static class SaveDataSerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.None
        };

        public static string Serialize(SaveData saveData)
        {
            return JsonConvert.SerializeObject(saveData, Settings);
        }

        public static SaveData Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<SaveData>(json, Settings);
        }
    }
}
