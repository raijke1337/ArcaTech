using System;
using System.Globalization;
using Arcatech.Managers;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    public class SaveManager : GenericLazySingleton<SaveManager>
    {
        [SerializeField] SaveService service;
        [SerializeField] readonly int SaveVersion = 2;
        private GameData _gameData;
        
        public GameData GetGameData
        {
            get
            {
                if (_gameData == null) Initialize();
                return _gameData;
            }
        }

        protected override void Awake()
        {
            if (_gameData == null) Initialize();
        }

        void Initialize()
        {
            Debug.Log("Initializing SaveManager");
            if (service == null)
            {
                Debug.LogError("No save load service asset selected!");
                return;
            }

            switch (service.TryLoadData(out _gameData))
            {
                case LoadDataResult.Success:
                    if (_gameData.version != SaveVersion)
                    {
                        Debug.LogError($"Version {_gameData.version} not equal to Version {SaveVersion}");
                    }
                    Debug.Log("Load Success");
                    break;
                case LoadDataResult.HashFail:
                    Debug.LogError("Hash Fail! Can't load data.");
                    break;
                case LoadDataResult.Missing:
                    Debug.Log("Missing Save! Creating new.");
                    _gameData = new GameData()
                    {
                        version = SaveVersion,
                        timestamp = DateTime.Now.ToString(CultureInfo.InvariantCulture)
                    };
                    service.SaveData(_gameData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void UpdateData(ISaveable updated)
        {
            updated.PopulateSaveData(_gameData);
            _gameData.timestamp = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            if (!service.SaveData(_gameData)) Debug.LogError("Failed to save game data!");
        }
    }
}