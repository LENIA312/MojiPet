using System;
using System.Collections.Generic;
using Mojipet.Events;
using Mojipet.Models;
using Mojipet.Save;
using Mojipet.Utilities;
using UnityEngine;

namespace Mojipet.Systems
{
    public sealed class SaveSystem
    {
        public const int CurrentVersion = 1;

        private readonly SaveRepository _repository;
        private readonly EventBus _eventBus;
        private bool _isSaving;

        public SaveData Data { get; private set; }
        public bool WasNewGame { get; private set; }

        public SaveSystem(SaveRepository repository, EventBus eventBus)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public bool Exists()
        {
            return _repository.Exists();
        }

        public int GetVersion()
        {
            return Data?.Version ?? 0;
        }

        public SaveData Load()
        {
            if (!_repository.Exists())
            {
                NewGame();
                return Data;
            }

            SaveData loaded;
            try
            {
                var json = _repository.Read();
                loaded = SaveDataSerializer.Deserialize(json);
                if (loaded == null)
                {
                    throw new InvalidOperationException("Deserialized SaveData is null.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Load failed, creating new game. {e}");
                NewGame();
                return Data;
            }

            var oldVersion = loaded.Version;
            loaded = RunMigration(loaded);
            Validate(loaded);

            Data = loaded;

            if (oldVersion != Data.Version)
            {
                _eventBus.Publish(new OnMigrationCompleted(oldVersion, Data.Version));
            }

            _eventBus.Publish(new OnSaveLoaded(Data));
            return Data;
        }

        public void NewGame()
        {
            WasNewGame = true;
            var now = TimeUtility.CurrentUtc;
            Data = new SaveData
            {
                Version = CurrentVersion,
                LastSaveUtc = now,
                Currency = new CurrencyData(),
                Pets = new List<PetData>(),
                Dictionary = new List<DictionaryEntryData>(),
                Research = new List<ResearchData>(),
                Facilities = new List<FacilityData>(),
                Idle = new IdleData { LastLoginUtc = now },
                Inventory = new InventoryData(),
                Settings = new SettingsData
                {
                    BgmVolume = 1f,
                    SeVolume = 1f,
                    Language = "ja",
                    Quality = 2
                }
            };

            Save();
            _eventBus.Publish(new OnNewGameCreated());
        }

        public void Save()
        {
            if (_isSaving)
            {
                return;
            }

            if (Data == null)
            {
                throw new InvalidOperationException("SaveData is not initialized. Call Load() or NewGame() first.");
            }

            _isSaving = true;
            try
            {
                Data.LastSaveUtc = TimeUtility.CurrentUtc;
                var json = SaveDataSerializer.Serialize(Data);
                _repository.Write(json);
                _eventBus.Publish(new OnSaveCompleted(Data.LastSaveUtc));
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Save failed. {e}");
            }
            finally
            {
                _isSaving = false;
            }
        }

        public void AutoSave()
        {
            Save();
        }

        public void DeleteSave()
        {
            _repository.Delete();
            NewGame();
        }

        private static SaveData RunMigration(SaveData data)
        {
            if (data.Version <= 0)
            {
                data.Version = CurrentVersion;
            }

            // Version1: no migration steps defined yet.
            return data;
        }

        private static void Validate(SaveData data)
        {
            if (data.Currency == null)
            {
                data.Currency = new CurrencyData();
            }

            if (data.Currency.Money < 0)
            {
                data.Currency.Money = 0;
            }

            if (data.Pets == null)
            {
                data.Pets = new List<PetData>();
            }

            if (data.Dictionary == null)
            {
                data.Dictionary = new List<DictionaryEntryData>();
            }

            if (data.Research == null)
            {
                data.Research = new List<ResearchData>();
            }

            if (data.Facilities == null)
            {
                data.Facilities = new List<FacilityData>();
            }

            if (data.Idle == null)
            {
                data.Idle = new IdleData { LastLoginUtc = TimeUtility.CurrentUtc };
            }

            if (data.Inventory == null)
            {
                data.Inventory = new InventoryData();
            }

            if (data.Inventory.ItemCounts == null)
            {
                data.Inventory.ItemCounts = new Dictionary<int, int>();
            }

            if (data.Settings == null)
            {
                data.Settings = new SettingsData
                {
                    BgmVolume = 1f,
                    SeVolume = 1f,
                    Language = "ja",
                    Quality = 2
                };
            }
        }
    }
}
