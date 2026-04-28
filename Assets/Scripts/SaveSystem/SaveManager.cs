using System;
using Arcatech.Managers;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    public class SaveManager : GenericLazySingleton<SaveManager>
    {
        [SerializeField] SaveService service;
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
                    Debug.Log("Load Success");
                    break;
                case LoadDataResult.HashFail:
                    Debug.Log("Hash Fail! Can't load data.");
                    break;
                case LoadDataResult.Missing:
                    Debug.Log("Missing Save! Creating new.");
                    _gameData = new GameData();
                    service.SaveData(_gameData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void UpdateData(ISaveable updated)
        {
            updated.PopulateSaveData(_gameData);
            if (!service.SaveData(_gameData)) Debug.LogError("Failed to save game data!");
        }
    }
}