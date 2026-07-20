using System;
using System.Globalization;
using System.IO;
using Mojipet.Master;
using Mojipet.Models;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace Mojipet.Editor.MasterData
{
    public static class MasterDataImporter
    {
        private const string CsvFolder = "Assets/MasterData";
        private const string OutputFolder = "Assets/AddressableAssets/Master";
        private const string AddressableGroupName = "Master";

        [MenuItem("Tools/Import MasterData")]
        public static void ImportAll()
        {
            ImportWordMaster();
            ImportPetMaster();
            ImportExpMaster();
            ImportItemMaster();
            ImportFacilityMaster();
            ImportShopMaster();
            ImportResearchMaster();
            ImportGameBalanceMaster();
            ImportCategoryMaster();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MasterDataImporter] Import completed.");
        }

        private static void ImportWordMaster()
        {
            var rows = ReadDataRows("WordMaster.csv");
            var entries = new WordMasterEntry[rows.Count];

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var reading = row[2];
                var characters = new string[reading.Length];
                for (var c = 0; c < reading.Length; c++)
                {
                    characters[c] = reading[c].ToString();
                }

                entries[i] = new WordMasterEntry
                {
                    WordId = ParseInt(row[0]),
                    Word = row[1],
                    Reading = reading,
                    Length = reading.Length,
                    Difficulty = ParseInt(row[3]),
                    Category = ParseEnum<CategoryId>(row[4]),
                    RequiredLevel = ParseInt(row[5]),
                    ResearchTimeSeconds = ParseInt(row[6]),
                    Characters = characters
                };
            }

            var asset = LoadOrCreateAsset<WordMasterSO>("WordMaster");
            asset.SetEntries(entries);
            MarkDirty(asset);
        }

        private static void ImportPetMaster()
        {
            var rows = ReadDataRows("PetMaster.csv");
            var entries = new PetMasterEntry[rows.Count];

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                entries[i] = new PetMasterEntry
                {
                    CharacterId = ParseInt(row[0]),
                    Character = row[1],
                    DisplayName = row[2],
                    InitialLevel = ParseInt(row[3]),
                    BaseProduction = ParseInt(row[4]),
                    BaseResearchSpeed = ParseFloat(row[5])
                };
            }

            var asset = LoadOrCreateAsset<PetMasterSO>("PetMaster");
            asset.SetEntries(entries);
            MarkDirty(asset);
        }

        private static void ImportExpMaster()
        {
            var rows = ReadDataRows("ExpMaster.csv");
            var entries = new ExpMasterEntry[rows.Count];

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                entries[i] = new ExpMasterEntry
                {
                    Level = ParseInt(row[0]),
                    RequiredExp = ParseInt(row[1]),
                    ProductionMultiplier = ParseFloat(row[2]),
                    ResearchSpeedMultiplier = ParseFloat(row[3])
                };
            }

            var asset = LoadOrCreateAsset<ExpMasterSO>("ExpMaster");
            asset.SetEntries(entries);
            MarkDirty(asset);
        }

        private static void ImportItemMaster()
        {
            var rows = ReadDataRows("ItemMaster.csv");
            var entries = new ItemMasterEntry[rows.Count];

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                entries[i] = new ItemMasterEntry
                {
                    Id = ParseInt(row[0]),
                    Name = row[1],
                    Description = row[2],
                    ItemType = ParseEnum<ItemType>(row[3]),
                    Price = ParseInt(row[4]),
                    Value = ParseFloat(row[5]),
                    DurationSeconds = row.Length > 6 ? ParseInt(row[6]) : 0
                };
            }

            var asset = LoadOrCreateAsset<ItemMasterSO>("ItemMaster");
            asset.SetEntries(entries);
            MarkDirty(asset);
        }

        private static void ImportFacilityMaster()
        {
            var rows = ReadDataRows("FacilityMaster.csv");
            var entries = new FacilityMasterEntry[rows.Count];

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                entries[i] = new FacilityMasterEntry
                {
                    Id = ParseInt(row[0]),
                    FacilityType = ParseEnum<FacilityId>(row[1]),
                    Level = ParseInt(row[2]),
                    UpgradeCost = ParseLong(row[3]),
                    EffectValue = ParseFloat(row[4])
                };
            }

            var asset = LoadOrCreateAsset<FacilityMasterSO>("FacilityMaster");
            asset.SetEntries(entries);
            MarkDirty(asset);
        }

        private static void ImportShopMaster()
        {
            var rows = ReadDataRows("ShopMaster.csv");
            var entries = new ShopMasterEntry[rows.Count];

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                entries[i] = new ShopMasterEntry
                {
                    Id = ParseInt(row[0]),
                    ItemId = ParseInt(row[1]),
                    Price = ParseInt(row[2]),
                    UnlockCondition = row[3]
                };
            }

            var asset = LoadOrCreateAsset<ShopMasterSO>("ShopMaster");
            asset.SetEntries(entries);
            MarkDirty(asset);
        }

        private static void ImportResearchMaster()
        {
            var rows = ReadDataRows("ResearchMaster.csv");
            var entries = new ResearchMasterEntry[rows.Count];

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                entries[i] = new ResearchMasterEntry
                {
                    Difficulty = ParseInt(row[0]),
                    RequiredSeconds = ParseInt(row[1])
                };
            }

            var asset = LoadOrCreateAsset<ResearchMasterSO>("ResearchMaster");
            asset.SetEntries(entries);
            MarkDirty(asset);
        }

        private static void ImportGameBalanceMaster()
        {
            var rows = ReadDataRows("GameBalanceMaster.csv");
            var values = new System.Collections.Generic.Dictionary<string, string>();
            foreach (var row in rows)
            {
                values[row[0]] = row[1];
            }

            var asset = LoadOrCreateAsset<GameBalanceMasterSO>("GameBalanceMaster");
            asset.MaxOfflineHours = ParseInt(values["MaxOfflineHours"]);
            asset.MaxPetLevel = ParseInt(values["MaxPetLevel"]);
            asset.MaxFood = ParseInt(values["MaxFood"]);
            asset.InitialSeedCount = ParseInt(values["InitialSeedCount"]);
            asset.DefaultResearchSlots = ParseInt(values["DefaultResearchSlots"]);
            asset.HungerDecayPerHour = ParseFloat(values["HungerDecayPerHour"]);
            asset.HungerLowThreshold = ParseFloat(values["HungerLowThreshold"]);
            asset.HungerLowMultiplier = ParseFloat(values["HungerLowMultiplier"]);
            asset.HungerStarvingMultiplier = ParseFloat(values["HungerStarvingMultiplier"]);
            asset.BaseExp = ParseInt(values["BaseExp"]);
            asset.LengthBonusMultiplier2 = ParseFloat(values["LengthBonusMultiplier2"]);
            asset.LengthBonusMultiplier3 = ParseFloat(values["LengthBonusMultiplier3"]);
            asset.LengthBonusMultiplier4 = ParseFloat(values["LengthBonusMultiplier4"]);
            asset.LengthBonusMultiplier5Plus = ParseFloat(values["LengthBonusMultiplier5Plus"]);
            asset.CheerCost = ParseInt(values["CheerCost"]);
            asset.CheerMultiplier = ParseFloat(values["CheerMultiplier"]);
            asset.CheerDurationSeconds = ParseInt(values["CheerDurationSeconds"]);
            MarkDirty(asset);
        }

        private static void ImportCategoryMaster()
        {
            var rows = ReadDataRows("CategoryMaster.csv");
            var entries = new CategoryMasterEntry[rows.Count];

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                entries[i] = new CategoryMasterEntry
                {
                    Category = ParseEnum<CategoryId>(row[0]),
                    RequiredLibraryLevel = ParseInt(row[1])
                };
            }

            var asset = LoadOrCreateAsset<CategoryMasterSO>("CategoryMaster");
            asset.SetEntries(entries);
            MarkDirty(asset);
        }

        private static System.Collections.Generic.List<string[]> ReadDataRows(string fileName)
        {
            var path = Path.Combine(CsvFolder, fileName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"MasterData CSV not found: {path}");
            }

            var rows = CsvReader.ReadRows(path);
            rows.RemoveAt(0);
            return rows;
        }

        private static T LoadOrCreateAsset<T>(string assetName) where T : ScriptableObject
        {
            var path = $"{OutputFolder}/{assetName}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            MarkAddressable(asset, $"Master/{assetName}");
            return asset;
        }

        private static void MarkDirty(ScriptableObject asset)
        {
            EditorUtility.SetDirty(asset);
        }

        private static void MarkAddressable(UnityEngine.Object asset, string address)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning(
                    "[MasterDataImporter] Addressable Asset Settings not found. " +
                    "Open Window > Asset Management > Addressables > Groups once to initialize Addressables, " +
                    "then re-run Tools > Import MasterData to register these assets as Addressable.");
                return;
            }

            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            var group = settings.FindGroup(AddressableGroupName) ?? settings.CreateGroup(
                AddressableGroupName, false, false, true, null, typeof(BundledAssetGroupSchema));

            var entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = address;
        }

        private static int ParseInt(string value)
        {
            return int.Parse(value, CultureInfo.InvariantCulture);
        }

        private static long ParseLong(string value)
        {
            return long.Parse(value, CultureInfo.InvariantCulture);
        }

        private static float ParseFloat(string value)
        {
            return float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        private static TEnum ParseEnum<TEnum>(string value) where TEnum : struct
        {
            return (TEnum)Enum.Parse(typeof(TEnum), value, true);
        }
    }
}
