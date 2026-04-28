using System;
using Arcatech.Managers;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    public class SaveManager : GenericLazySingleton<SaveManager>
    {
        [SerializeField] InterfaceRef<ISaveLoadService> service;
        public GameData GetGameData =>_gameData;
        private GameData _gameData;

        private void OnEnable()
        {
            if (service.Value == null)
            {
                Debug.LogError("No save load service asset selected!");
                return;
            }

            switch (service.Value.TryLoadData(out _gameData))
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
                    service.Value.SaveData(_gameData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UpdateData(ISaveable updated)
        {
            updated.PopulateSaveData(_gameData);
            if (!service.Value.SaveData(_gameData)) Debug.LogError("Failed to save game data!");
        }
    }
}