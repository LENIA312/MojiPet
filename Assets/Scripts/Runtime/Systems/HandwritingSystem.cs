using System;
using System.IO;
using Mojipet.Events;
using Mojipet.Models;
using UnityEngine;

namespace Mojipet.Systems
{
    // Stores each character's hand-drawn glyph as a PNG on disk, keyed by CharacterId.
    // Kept out of SaveData/save.json (which is small JSON) since texture bytes would
    // bloat it; PetData.HasHandwriting just flags whether a file exists.
    public sealed class HandwritingSystem
    {
        private const int TextureSize = 256;

        private readonly SaveSystem _saveSystem;
        private readonly EventBus _eventBus;
        private readonly string _directoryPath;

        public HandwritingSystem(SaveSystem saveSystem, EventBus eventBus)
        {
            _saveSystem = saveSystem ?? throw new ArgumentNullException(nameof(saveSystem));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _directoryPath = Path.Combine(Application.persistentDataPath, "Handwriting");
        }

        public int TextureResolution => TextureSize;

        public bool HasDrawing(int characterId)
        {
            return File.Exists(GetFilePath(characterId));
        }

        public void SaveDrawing(int characterId, Texture2D texture)
        {
            if (texture == null)
            {
                throw new ArgumentNullException(nameof(texture));
            }

            Directory.CreateDirectory(_directoryPath);
            File.WriteAllBytes(GetFilePath(characterId), texture.EncodeToPNG());

            var pet = FindPet(characterId);
            if (pet != null)
            {
                pet.HasHandwriting = true;
                _saveSystem.Save();
            }

            _eventBus.Publish(new OnHandwritingSaved(characterId));
        }

        public Texture2D LoadDrawing(int characterId)
        {
            var path = GetFilePath(characterId);
            if (!File.Exists(path))
            {
                return null;
            }

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(path));
            return texture;
        }

        private PetData FindPet(int characterId)
        {
            foreach (var pet in _saveSystem.Data.Pets)
            {
                if (pet.CharacterId == characterId)
                {
                    return pet;
                }
            }

            return null;
        }

        private string GetFilePath(int characterId)
        {
            return Path.Combine(_directoryPath, $"{characterId}.png");
        }
    }
}
