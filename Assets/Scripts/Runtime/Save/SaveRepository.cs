using System.IO;
using System.Text;
using UnityEngine;

namespace Mojipet.Save
{
    public sealed class SaveRepository
    {
        private const string FileName = "save.json";
        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);

        private string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public bool Exists()
        {
            return File.Exists(FilePath);
        }

        public string Read()
        {
            return File.ReadAllText(FilePath, Utf8NoBom);
        }

        public void Write(string json)
        {
            File.WriteAllText(FilePath, json, Utf8NoBom);
        }

        public void Delete()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }
    }
}
