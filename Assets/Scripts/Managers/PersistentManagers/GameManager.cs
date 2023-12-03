//using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.SceneManagement;

namespace Arcatech.Managers
{
    public class GameManager : MonoBehaviour
    {

        //public GameMode GameMode { get; private set; }
        [SerializeField] private LoadedManagers GameControllersPrefab;
        [SerializeField] private GameInterfaceManager gameInterfaceManagerPrefab;
        [SerializeField] private IsoCameraController gameCameraPrefab;
        private LoadedManagers _gameControllers;

        public LoadedManagers GetGameControllers => _gameControllers;
        public GameInterfaceManager GetGameInterfacePrefab => gameInterfaceManagerPrefab;
        public IsoCameraController GetGameCameraPrefab => gameCameraPrefab;



        public LevelData GetLevelData(string ID) => _levels[ID];
        private Dictionary<string, LevelData> _levels;

        private LevelData _currentLevel;
        private string gameLevelID;
        public LevelData GetCurrentLevelData { get => _currentLevel; }

        #region default
        private void OnEnable()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            var levelCards = DataManager.Instance.GetAssetsOfType<LevelCardSO>(Constants.Configs.c_LevelsPath);
            _levels = new Dictionary<string, LevelData> {
                {"",null}
            };

            foreach (var level in levelCards)
            {
                _levels[level.ID] = new LevelData(level);
            }
            _currentLevel = _levels["main"]; // TODO: Hardcode

            //_gameControllers = Instantiate(GameControllersPrefab);
            //_gameControllers.Initiate(_currentLevel);
        }

        private void Update()
        {
            if (_currentLevel != null && _currentLevel.Type == LevelType.Game && _gameControllers != null)
            {

                _gameControllers.UpdateManagers(Time.deltaTime);
            }
        }
        #endregion
        public void RequestLevelLoad(string ID)
        {
            LoadLevel(ID);
        }

        public void OnFinishedEquips()
        {
            var newequips = _gameControllers.UnitsManager.GetPlayerUnit.GetUnitInventory.PackSaveData;
            DataManager.Instance.UpdateSaveData(newequips);
            LoadLevel(gameLevelID, true);
        }

        private void LoadLevel(string ID, bool forceLoad = false)
        {
            AudioManager.Instance.CleanUpOnSceneChange();
            if (ID == "") { LoadLevel("main", true); } // it happens in debug on level complete - the card has no "next level" ID
            if (forceLoad) // used in equips level 
            {
                _currentLevel = _levels[ID];
                SceneManager.LoadScene(_currentLevel.SceneLoaderIndex);
            }
            else
            {
                try
                {
                    var typeLoad = _levels[ID].Type;
                    switch (typeLoad)
                    {
                        case LevelType.Menu:
                            LoadLevel("equips", true);
                            break;
                        case LevelType.Scene:
                            LoadLevel(ID, true);
                            break;
                        case LevelType.Game:
                            gameLevelID = ID;
                            LoadLevel("equips", true);
                            break;
                    }
                }
                catch
                {
                    Debug.Log($"Something went wrong when switching to level {ID} from {_currentLevel.LevelID}");
                }
            }
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            //switch (_currentLevel.Type)
            //{
            //    case LevelType.Menu:
            //        break;
            //    case LevelType.Game:
            //        _gameControllers = Instantiate(GameControllersPrefab);
            //        _gameControllers.Initiate(_currentLevel);
            //        break;
            //    case LevelType.Scene:
            //        _gameControllers = Instantiate(GameControllersPrefab);
            //        _gameControllers.Initiate(_currentLevel);
            //        break;
            //}
            Debug.ClearDeveloperConsole();
            _gameControllers = Instantiate(GameControllersPrefab);
            _gameControllers.Initiate(_currentLevel);
        }

        public void OnStartNewGame()
        {
            DataManager.Instance.OnNewGame();
            RequestLevelLoad("intro");
        }
        public void OnReturnToMain()
        {
            LoadLevel("main", true);
        }


        #region game events

        public void OnLevelComplete()
        {
            OnFinishedEquips(); // update save data with picked up items
            if (_gameControllers != null)
            {
                _gameControllers.Stop();
            }

            var next = _currentLevel.NextLevelID;

            DataManager.Instance.UpdateSaveData(next);

            
            LoadLevel(next);
        }


        public void OnPlayerDead()
        {
            _gameControllers.GameInterfaceManager.GameOver();
        }
        public void OnItemPickup(string itemID)
        {
            Item item = DataManager.Instance.GetConfigByID<Item>(itemID);
            _gameControllers.UnitsManager.GetPlayerUnit.GetUnitInventory.AddItem(new Items.InventoryItem(item, _gameControllers.UnitsManager.GetPlayerUnit));
        }
        public void QuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();

#endif
        }

        #endregion

        #region SingletonLogic

        public static GameManager Instance;
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else Destroy(gameObject);
        }
        #endregion
    }


}