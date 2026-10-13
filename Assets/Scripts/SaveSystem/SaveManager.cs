using System;
using System.Collections.Generic;
using System.Globalization;
using Arcatech.Managers;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    /// <summary>Файлы сохранения. Профиль один.</summary>
    public enum SaveSlot
    {
        Settings,
        MetaProgress,
        Checkpoint
    }

    /// <summary>Единственный персистентный синглтон системы сохранений.
    /// Хранит канонические in-memory объекты Settings, MetaProgress и чекпоинт уровня
    /// и является единственной точкой записи их на диск.</summary>
    public class SaveManager : GenericLazySingleton<SaveManager>
    {
        [SerializeField] private SaveService service;
        [SerializeField, ReadOnlyText] private int settingsVersion = 1;
        [SerializeField, ReadOnlyText] private int metaVersion = 1;
        [SerializeField, ReadOnlyText] private int checkpointVersion = 1;
        [SerializeField] private bool showDebugs = false;
        [SerializeField] private float settingsSaveDebounceSeconds = 0.5f;

        private const string SettingsFileName = "settings.json";
        private const string MetaProgressFileName = "meta_progress.json";
        private const string CheckpointFileName = "level_checkpoint.json";

        private GameSettingsData _settings;
        private MetaProgressData _metaProgress;
        private LevelProgressData _checkpoint; // null - чекпоинта нет
        private bool _initialized;

        // Повреждённые файлы. Пока игрок не решил, что с ними делать,
        // запись в них заблокирована - иначе File.Replace затрёт *.bak,
        // и восстанавливать будет не из чего.
        private readonly HashSet<SaveSlot> _corrupted = new();

        /// <summary>Срабатывает, если при загрузке не сошёлся хеш. UI предлагает восстановить из *.bak.</summary>
        public event Action CorruptionDetected;

        public GameSettingsData Settings
        {
            get { EnsureInitialized(); return _settings; }
        }

        public MetaProgressData MetaProgress
        {
            get { EnsureInitialized(); return _metaProgress; }
        }

        public IReadOnlyCollection<SaveSlot> CorruptedSlots
        {
            get { EnsureInitialized(); return _corrupted; }
        }

        public bool HasCorruptedSaves => CorruptedSlots.Count > 0;

        protected void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }
            instance = this;
            // Все записи идут на диск сразу, поэтому экземпляр не обязан переживать
            // смену сцен: новый экземпляр просто перечитает файлы.
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            if (service == null)
            {
                // У JSON-сервиса нет настроек, поэтому его можно создать на лету.
                Debug.LogWarning("SaveManager: no SaveService asset assigned - using default JsonSaveLoadService.");
                service = ScriptableObject.CreateInstance<JsonSaveLoadService>();
            }

            _settings = Load(SaveSlot.Settings, SettingsFileName,
                () => new GameSettingsData { version = settingsVersion }, settingsVersion);
            _metaProgress = Load(SaveSlot.MetaProgress, MetaProgressFileName,
                () => new MetaProgressData { version = metaVersion }, metaVersion);
            _checkpoint = Load<LevelProgressData>(SaveSlot.Checkpoint, CheckpointFileName,
                () => null, checkpointVersion);

            if (_corrupted.Count > 0) CorruptionDetected?.Invoke();
        }

        private T Load<T>(SaveSlot slot, string fileName, Func<T> createDefault, int currentVersion)
            where T : VersionedSaveData
        {
            switch (service.TryLoadData(fileName, out T data))
            {
                case LoadDataResult.Success:
                    MigrateIfNeeded(data, currentVersion);
                    if (showDebugs) Debug.Log($"{slot} loaded.");
                    return data;

                case LoadDataResult.HashFail:
                    Debug.LogError($"{slot}: hash check failed. Waiting for player decision (restore backup / reset).");
                    _corrupted.Add(slot);
                    return createDefault();

                default: // Missing
                    if (showDebugs) Debug.Log($"No {slot} save found - using defaults.");
                    return createDefault();
            }
        }

        private void MigrateIfNeeded<T>(T data, int currentVersion) where T : VersionedSaveData
        {
            if (data.version == currentVersion) return;
            // TODO: реализовать реальную миграцию по мере изменения формата.
            Debug.LogWarning($"{typeof(T).Name} version mismatch: file={data.version}, code={currentVersion}. " +
                              "No migration implemented yet - using data as-is.");
            data.version = currentVersion;
        }

        private bool Write<T>(SaveSlot slot, T data, string fileName) where T : VersionedSaveData
        {
            if (_corrupted.Contains(slot))
            {
                Debug.LogWarning($"{slot}: write skipped - save file is corrupted and not resolved yet.");
                return false;
            }

            data.timestamp = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            if (service.SaveData(data, fileName)) return true;

            Debug.LogError($"Failed to save {slot}!");
            return false;
        }

        // -------------------------------------------------------------
        // Settings: часто меняются (слайдеры) - пишем с дебаунсом
        // -------------------------------------------------------------

        public void RequestSaveSettings()
        {
            CancelInvoke(nameof(SaveSettingsImmediate));
            Invoke(nameof(SaveSettingsImmediate), settingsSaveDebounceSeconds);
        }

        private void SaveSettingsImmediate() => Write(SaveSlot.Settings, Settings, SettingsFileName);

        // -------------------------------------------------------------
        // MetaProgress: редкие, важные события - пишем немедленно
        // -------------------------------------------------------------

        public void SaveMetaProgress() => Write(SaveSlot.MetaProgress, MetaProgress, MetaProgressFileName);

        /// <summary>«Новая игра»: стирает мета-прогресс и чекпоинт. Настройки не трогает.</summary>
        public void ResetGameProgress()
        {
            EnsureInitialized();
            _corrupted.Remove(SaveSlot.MetaProgress);
            _corrupted.Remove(SaveSlot.Checkpoint);
            service.DeleteData(MetaProgressFileName);
            _metaProgress = new MetaProgressData { version = metaVersion };
            SaveMetaProgress();
            ClearCheckpoint();
        }

        // -------------------------------------------------------------
        // Чекпоинт уровня: один на профиль
        // -------------------------------------------------------------

        public bool HasCheckpoint
        {
            get { EnsureInitialized(); return _checkpoint != null; }
        }

        /// <summary>Возвращает КОПИЮ сохранённого чекпоинта.</summary>
        public bool TryGetCheckpoint(out LevelProgressData checkpoint)
        {
            EnsureInitialized();
            checkpoint = _checkpoint != null ? new LevelProgressData(_checkpoint) : null;
            return checkpoint != null;
        }

        public void SaveCheckpoint(LevelProgressData checkpoint)
        {
            EnsureInitialized();
            if (checkpoint == null) return;

            _checkpoint = new LevelProgressData(checkpoint) { version = checkpointVersion };
            Write(SaveSlot.Checkpoint, _checkpoint, CheckpointFileName);
        }

        public void ClearCheckpoint()
        {
            EnsureInitialized();
            _checkpoint = null;
            _corrupted.Remove(SaveSlot.Checkpoint);
            service.DeleteData(CheckpointFileName);
            if (showDebugs) Debug.Log("Checkpoint cleared.");
        }

        // -------------------------------------------------------------
        // Повреждённые сохранения
        // -------------------------------------------------------------

        private string FileNameOf(SaveSlot slot) => slot switch
        {
            SaveSlot.Settings => SettingsFileName,
            SaveSlot.MetaProgress => MetaProgressFileName,
            _ => CheckpointFileName
        };

        /// <summary>Пробует восстановить файл из резервной копии (предыдущего успешного сохранения).</summary>
        public bool TryRestoreFromBackup(SaveSlot slot)
        {
            EnsureInitialized();
            if (!_corrupted.Contains(slot)) return true;

            var fileName = FileNameOf(slot);
            bool restored = slot switch
            {
                SaveSlot.Settings => TryRestore(fileName, ref _settings),
                SaveSlot.MetaProgress => TryRestore(fileName, ref _metaProgress),
                _ => TryRestore(fileName, ref _checkpoint)
            };

            if (!restored)
            {
                Debug.LogWarning($"{slot}: backup is missing or corrupted too.");
                return false;
            }

            _corrupted.Remove(slot);
            // Удаляем повреждённый файл вместе со старым *.bak и пишем восстановленные данные начисто.
            service.DeleteData(fileName);
            switch (slot)
            {
                case SaveSlot.Settings: SaveSettingsImmediate(); break;
                case SaveSlot.MetaProgress: SaveMetaProgress(); break;
                default: Write(SaveSlot.Checkpoint, _checkpoint, CheckpointFileName); break;
            }

            Debug.Log($"{slot} restored from backup.");
            return true;
        }

        private bool TryRestore<T>(string fileName, ref T target) where T : class
        {
            if (service.TryLoadBackup(fileName, out T data) != LoadDataResult.Success) return false;
            target = data;
            return true;
        }

        /// <summary>Игрок отказался от восстановления (или его не удалось сделать): начинаем этот файл с нуля.</summary>
        public void ResetCorrupted(SaveSlot slot)
        {
            EnsureInitialized();
            _corrupted.Remove(slot);
            service.DeleteData(FileNameOf(slot));

            switch (slot)
            {
                case SaveSlot.Settings:
                    _settings = new GameSettingsData { version = settingsVersion };
                    SaveSettingsImmediate();
                    break;
                case SaveSlot.MetaProgress:
                    _metaProgress = new MetaProgressData { version = metaVersion };
                    SaveMetaProgress();
                    break;
                default:
                    _checkpoint = null;
                    break;
            }
        }
    }
}
