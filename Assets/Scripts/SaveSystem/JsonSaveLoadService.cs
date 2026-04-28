using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    [CreateAssetMenu(fileName = "JSON Save/Load Provider", menuName = "System Asset/Save Load Service/JSON Serializer")]
    public class JsonSaveLoadService : SaveService
    {
        
        [SerializeField] private string fileName = "save.json";
        [SerializeField] private string hashSalt = "Arcatech_Save_v1";

        private string FilePath => Path.Combine(Application.persistentDataPath, fileName);

        private struct SaveFileWrapper
        {
            public GameData Data;
            public string Hash;
        }

        public override bool SaveData(GameData data)
        {
            if (data == null)
            {
                Debug.LogWarning("SaveData called with null GameData");
                return false;
            }

            try
            {
                var payload = JsonConvert.SerializeObject(data);
                var hash = ComputeHash(payload);
                var wrapper = new SaveFileWrapper { Data = data, Hash = hash };
                var json = JsonConvert.SerializeObject(wrapper, Formatting.Indented);

                var directory = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var tempPath = FilePath + ".tmp";
                File.WriteAllText(tempPath, json, Encoding.UTF8);
                if (File.Exists(FilePath))
                {
                    File.Replace(tempPath, FilePath, FilePath + ".bak");
                }
                else
                {
                    File.Move(tempPath, FilePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"SaveData failed: {ex}");
                return false;
            }
        }

        public override LoadDataResult TryLoadData(out GameData data)
        {
            data = null;

            if (!File.Exists(FilePath))
            {
                return LoadDataResult.Missing;
            }

            try
            {
                var json = File.ReadAllText(FilePath, Encoding.UTF8);
                var wrapper = JsonConvert.DeserializeObject<SaveFileWrapper>(json);

                if (wrapper.Data == null || string.IsNullOrEmpty(wrapper.Hash))
                {
                    Debug.LogWarning("Corrupted save file structure.");
                    return LoadDataResult.Missing;
                }

                var recalculatedHash = ComputeHash(JsonConvert.SerializeObject(wrapper.Data));
                if (!string.Equals(wrapper.Hash, recalculatedHash, StringComparison.Ordinal))
                {
                    return LoadDataResult.HashFail;
                }

                data = wrapper.Data;
                return LoadDataResult.Success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"TryLoadData failed: {ex}");
                return LoadDataResult.HashFail;
            }
        }

        private string ComputeHash(string payload)
        {
            var bytes = Encoding.UTF8.GetBytes(payload + hashSalt);
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(bytes);
            var sb = new StringBuilder(hashBytes.Length * 2);
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}