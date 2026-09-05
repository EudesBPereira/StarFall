using System.IO;
using Starfall.Logic;
using UnityEngine;

namespace Starfall.Save
{
    /// <summary>Stores the save as a JSON file in Application.persistentDataPath.</summary>
    public sealed class FileSaveStorage : ISaveStorage
    {
        private readonly string _path;

        public FileSaveStorage(string fileName = "starfall_save.json")
        {
            _path = System.IO.Path.Combine(Application.persistentDataPath, fileName);
        }

        public string FilePath => _path;

        public bool Exists() => File.Exists(_path);

        public string Read() => File.ReadAllText(_path);

        public void Write(string content)
        {
            string dir = System.IO.Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string tmp = _path + ".tmp";
            File.WriteAllText(tmp, content);
            if (File.Exists(_path)) File.Delete(_path);
            File.Move(tmp, _path);
        }

        public void Delete()
        {
            if (File.Exists(_path)) File.Delete(_path);
        }
    }

    /// <summary>JsonUtility-backed serializer. Returns null for content that does not look like our payload.</summary>
    public sealed class JsonUtilitySaveSerializer : ISaveSerializer
    {
        public string Serialize(SaveData data) => JsonUtility.ToJson(data, true);

        public SaveData Deserialize(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;
            string trimmed = content.TrimStart();
            if (!trimmed.StartsWith("{")) return null;
            var data = JsonUtility.FromJson<SaveData>(content);
            return data;
        }
    }
}
