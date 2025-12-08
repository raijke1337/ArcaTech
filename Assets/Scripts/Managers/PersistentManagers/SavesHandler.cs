using System.IO;
using UnityEngine;

namespace Arcatech.Managers.Save
{
    public class SavesHandler : ISavesService
    {
        private ISerializer _serializer;
        string path;
        readonly string filename = "save";

        public SavesHandler(ISerializer serializer)
        {
            _serializer = serializer;
            path = Application.persistentDataPath;
        }

        public GameSaveData Load()
        {
            string location = BuildPathToFile(filename);
            if (!File.Exists(location))
            {
                throw new IOException();
            }
            Debug.Log($"load from {location}");
            return _serializer.Deserialize<GameSaveData>(File.ReadAllText(location));
        }

        public void Save(GameSaveData data, bool overwrite = true)
        {
            string location = BuildPathToFile(filename);
            if (!overwrite && File.Exists(location))
            {
                throw new IOException();
            }
            Debug.Log($"Save to {location}");
            File.WriteAllText(location, _serializer.Serialize(data));
        }




        string BuildPathToFile(string filename)
        {
            return Path.Combine(path, string.Concat(filename, '.', _serializer.Extension));
        }
    }

    public class GameSaveData
    {}
}

