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
        // Соль не сериализуется как публичное поле ассета, чтобы её нельзя было
        // случайно/намеренно поменять через инспектор в билде.
        private const string HashSalt = "KittyTitties";

        private string GetFilePath(string fileName) => Path.Combine(Application.persistentDataPath, fileName);

        private struct SaveFileWrapper<T>
        {
            public T Data;
            public string Hash;
        }

        public override bool SaveData<T>(T data, string fileName)
        {
            if (data == null)
            {
                Debug.LogWarning($"SaveData<{typeof(T).Name}> called with null data.");
                return false;
            }

            var filePath = GetFilePath(fileName);

            try
            {
                var payload = JsonConvert.SerializeObject(data);
                var hash = ComputeHash(payload);
                var wrapper = new SaveFileWrapper<T> { Data = data, Hash = hash };
                var json = JsonConvert.SerializeObject(wrapper, Formatting.Indented);

                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var tempPath = filePath + ".tmp";
                File.WriteAllText(tempPath, json, Encoding.UTF8);

                if (File.Exists(filePath))
                {
                    File.Replace(tempPath, filePath, filePath + ".bak");
                }
                else
                {
                    File.Move(tempPath, filePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"SaveData<{typeof(T).Name}> failed: {ex}");
                return false;
            }
        }

        public override LoadDataResult TryLoadData<T>(string fileName, out T data)
        {
            data = null;
            var filePath = GetFilePath(fileName);

            if (!File.Exists(filePath))
            {
                return LoadDataResult.Missing;
            }

            try
            {
                var json = File.ReadAllText(filePath, Encoding.UTF8);
                var wrapper = JsonConvert.DeserializeObject<SaveFileWrapper<T>>(json);

                if (wrapper.Data == null || string.IsNullOrEmpty(wrapper.Hash))
                {
                    Debug.LogWarning($"Corrupted save file structure: {fileName}");
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
                Debug.LogError($"TryLoadData<{typeof(T).Name}> failed: {ex}");
                return LoadDataResult.HashFail;
            }
        }

        private string ComputeHash(string payload)
        {
            var bytes = Encoding.UTF8.GetBytes(payload + HashSalt);
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