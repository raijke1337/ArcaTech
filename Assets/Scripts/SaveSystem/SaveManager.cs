using System;
using System.Globalization;
using Arcatech.Managers;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    /// <summary>Единственный персистентный синглтон системы сохранений.
    /// Хранит канонические in-memory объекты Settings и MetaProgress
    /// и является единственной точкой записи их на диск.</summary>
    public class SaveManager : GenericLazySingleton<SaveManager>
    {

        [SerializeField] private SaveService service;
        [SerializeField, ReadOnlyText] private int settingsVersion = 1;
        [SerializeField, ReadOnlyText] private int metaVersion = 1;
        [SerializeField] private bool showDebugs = false;
        [SerializeField] private float settingsSaveDebounceSeconds = 0.5f;

        private const string SettingsFileName = "settings.json";
        private const string MetaProgressFileName = "meta_progress.json";

        private GameSettingsData _settings;
        private MetaProgressData _metaProgress;
        private bool _initialized;

        public GameSettingsData Settings
        {
            get { EnsureInitialized(); return _settings; }
        }

        public MetaProgressData MetaProgress
        {
            get { EnsureInitialized(); return _metaProgress; }
        }

        protected void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            if (service == null)
            {
                Debug.LogError("SaveManager: no SaveService asset assigned!");
                _settings = new GameSettingsData { version = settingsVersion };
                _metaProgress = new MetaProgressData { version = metaVersion };
                return;
            }

            LoadSettings();
            LoadMetaProgress();
        }

        private void LoadSettings()
        {
            switch (service.TryLoadData(SettingsFileName, out _settings))
            {
                case LoadDataResult.Success:
                    MigrateIfNeeded(_settings, settingsVersion);
                    if (showDebugs) Debug.Log("Settings loaded.");
                    break;
                case LoadDataResult.HashFail:
                    Debug.LogError("Settings hash check failed - using defaults.");
                    _settings = new GameSettingsData { version = settingsVersion };
                    break;
                case LoadDataResult.Missing:
                    if (showDebugs) Debug.Log("No settings save found - creating defaults.");
                    _settings = new GameSettingsData { version = settingsVersion };
                    SaveSettingsImmediate();
                    break;
            }
        }

        private void LoadMetaProgress()
        {
            switch (service.TryLoadData(MetaProgressFileName, out _metaProgress))
            {
                case LoadDataResult.Success:
                    MigrateIfNeeded(_metaProgress, metaVersion);
                    if (showDebugs) Debug.Log("Meta progress loaded.");
                    break;
                case LoadDataResult.HashFail:
                    Debug.LogError("Meta progress hash check failed - using defaults.");
                    _metaProgress = new MetaProgressData { version = metaVersion };
                    break;
                case LoadDataResult.Missing:
                    if (showDebugs) Debug.Log("No meta progress save found - creating defaults.");
                    _metaProgress = new MetaProgressData { version = metaVersion };
                    SaveMetaProgress();
                    break;
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

        // -------------------------------------------------------------
        // Settings: часто меняются (слайдеры) - пишем с дебаунсом
        // -------------------------------------------------------------

        public void RequestSaveSettings()
        {
            CancelInvoke(nameof(SaveSettingsImmediate));
            Invoke(nameof(SaveSettingsImmediate), settingsSaveDebounceSeconds);
        }

        private void SaveSettingsImmediate()
        {
            _settings.timestamp = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            if (!service.SaveData(_settings, SettingsFileName))
            {
                Debug.LogError("Failed to save settings!");
            }
        }

        // -------------------------------------------------------------
        // MetaProgress: редкие, важные события - пишем немедленно
        // -------------------------------------------------------------

        public void SaveMetaProgress()
        {
            _metaProgress.timestamp = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            if (!service.SaveData(_metaProgress, MetaProgressFileName))
            {
                Debug.LogError("Failed to save meta progress!");
            }
        }
    }
}