using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    [CreateAssetMenu(fileName = "JSON Save/Load Provider", menuName = "System Asset/Save Load Service/JSON Serializer")]
    public class JsonSaveLoadService : SaveService
    {
        // Соль не сериализуется как публичное поле ассета, чтобы её нельзя было
        // случайно/намеренно поменять через инспектор в билде.
        private const string HashSalt = "KittyTitties";
        private const string BackupSuffix = ".bak";

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
                    // Предыдущая версия файла становится резервной копией (*.bak).
                    File.Replace(tempPath, filePath, filePath + BackupSuffix);
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
            => LoadFromPath(GetFilePath(fileName), out data);

        public override LoadDataResult TryLoadBackup<T>(string fileName, out T data)
            => LoadFromPath(GetFilePath(fileName) + BackupSuffix, out data);

        public override void DeleteData(string fileName)
        {
            var filePath = GetFilePath(fileName);
            try
            {
                if (File.Exists(filePath)) File.Delete(filePath);
                if (File.Exists(filePath + BackupSuffix)) File.Delete(filePath + BackupSuffix);
            }
            catch (Exception ex)
            {
                Debug.LogError($"DeleteData({fileName}) failed: {ex}");
            }
        }

        private static readonly JsonSerializerSettings ReadSettings = new()
        {
            // Иначе строки, похожие на дату (timestamp), будут переформатированы, и хеш не сойдётся.
            DateParseHandling = DateParseHandling.None
        };

        private LoadDataResult LoadFromPath<T>(string filePath, out T data) where T : class
        {
            data = null;

            if (!File.Exists(filePath))
            {
                return LoadDataResult.Missing;
            }

            try
            {
                var json = File.ReadAllText(filePath, Encoding.UTF8);
                var root = JsonConvert.DeserializeObject<JObject>(json, ReadSettings);
                var dataToken = root?["Data"];
                var hash = root?["Hash"]?.Value<string>();

                if (dataToken == null || dataToken.Type == JTokenType.Null || string.IsNullOrEmpty(hash))
                {
                    // Файл есть, но структура сломана - это повреждение, а не отсутствие.
                    Debug.LogWarning($"Corrupted save file structure: {filePath}");
                    return LoadDataResult.HashFail;
                }

                // Хеш считается по JSON из файла, а не по повторной сериализации объекта:
                // так добавление новых полей в классы сохранения не ломает старые сейвы.
                var recalculatedHash = ComputeHash(dataToken.ToString(Formatting.None));
                if (!string.Equals(hash, recalculatedHash, StringComparison.Ordinal))
                {
                    return LoadDataResult.HashFail;
                }

                data = dataToken.ToObject<T>();
                return data != null ? LoadDataResult.Success : LoadDataResult.HashFail;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Load {typeof(T).Name} from {filePath} failed: {ex}");
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
