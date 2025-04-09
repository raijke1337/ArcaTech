//using Newtonsoft.Json;
using Arcatech.Effects;
using Arcatech.EventBus;
using Arcatech.Items;
using Arcatech.Scenes;
using Arcatech.Scenes.Cameras;
using Arcatech.UI;
using Arcatech.Units;
using DG.Tweening;
using KBCore.Refs;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Arcatech.Managers
{
    public class GameManager : MonoBehaviour
    {
        #region controllers

        #region SingletonLogic

        public static GameManager Instance;
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else Destroy(gameObject);
        }
        #endregion
        [SerializeField] private PlayerUnit PlayerPrefab;
        [SerializeField] private MenuPrefabControllerComp _mainMenuPrefab;

        [SerializeField] private GameInterfaceManager gameInterfaceManagerPrefab;
        [SerializeField] private IsoCameraController gameCameraPrefab;        
        public GameInterfaceManager GetGameInterfacePrefab => gameInterfaceManagerPrefab;
        public IsoCameraController GetGameCameraPrefab => gameCameraPrefab;

        private void OnEnable()
        {
            Bindings(true);
            DOTween.Init();
            _dataManager = DataManager.Instance;

#if UNITY_EDITOR
            Assert.IsNotNull(_mainScene);
            Assert.IsNotNull(_equipsScene);
            Assert.IsNotNull(_galleryScene);
            Assert.IsNotNull(_newgameScene);
            Assert.IsNotNull(_noEquipsLevel);
            Assert.IsNotNull(_mainMenuPrefab);

            // for quicker load into debug level 
#endif
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            _currentLevel = _dataManager.GetSceneContainer(sceneIndex);
            if (_currentLevel == _mainScene)
            {
                Instantiate(_mainMenuPrefab);
            }
        }


        private void OnDisable()
        {
            Bindings(false);
        }

        #endregion

        #region levels management

        private DataManager _dataManager;


        [Space(10f), SerializeField] private SceneContainer _mainScene;
        [SerializeField] private SceneContainer _galleryScene;
        [SerializeField] private SceneContainer _newgameScene;
        [SerializeField] private SceneContainer _noEquipsLevel; // tutorial
        [SerializeField] private SceneContainer _equipsScene;

        public SceneContainer GetCurrentLevelData { get => _currentLevel; }
        private SceneContainer _currentLevel;

        private bool _equipsDone = false;
        private SceneContainer _cachedGameLevel;
        public void RequestLoadSceneFromContainer(SceneContainer sc)
        {
            DOTween.KillAll();
           // Debug.Log($"Placeholder - starting level as GAME level");

            if (sc == _mainScene)
            {
                Instantiate(_mainMenuPrefab);
            }

            switch (sc.LevelType)
            {
                default:
                    DoLoadScene(sc.SceneLoaderIndex);
                    break;
                case LevelType.Game:
                    if (sc == _noEquipsLevel)
                    {
                        _cachedGameLevel = sc;
                        DoLoadScene(_noEquipsLevel.SceneLoaderIndex);
                        break;
                    }// override default equips menu

                    if (_equipsDone)
                    {
                        _equipsDone = false;
                        DoLoadScene(_cachedGameLevel.SceneLoaderIndex);
                    }
                    else
                    {
                        _cachedGameLevel = sc;
                        DoLoadScene(_equipsScene.SceneLoaderIndex);
                    }

                    break;
            }
        }

        void DoLoadScene(int index)
        {            
            Debug.Log($"Loading scene index {index} - {SceneManager.GetSceneByBuildIndex(index).name}\nCurrent level is: {SceneManager.GetActiveScene().name}");       
            _currentLevel = _dataManager.GetSceneContainer(index);            
            SceneManager.LoadScene(index);

        }
        #endregion
        #region UNITY UI
        public void OnStartNewGameButton()
        {
            DataManager.Instance.OnNewGame();
            RequestLoadSceneFromContainer(_newgameScene);
        }
        public void OnGalleryButton()
        {
            RequestLoadSceneFromContainer(_galleryScene);
        }
        public void OnExitButton()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#endif
            Application.Quit();
        }

        public void OnFinishedEquips()
        {
            _equipsDone = true;
            RequestLoadSceneFromContainer(_cachedGameLevel);
        }

        
        #endregion


        #region level complete
        EventBinding<LevelCompletedEvent> _levelBind;
        private void Bindings(bool isStart)
        {
            if (isStart)
            {
                _levelBind = new EventBinding<LevelCompletedEvent>(OnLevelCompleteTrigger);
                EventBus<LevelCompletedEvent>.Register(_levelBind);
                //Debug.Log($"register event binds in {this} at {Time.time}");
            }
            else
            {
                EventBus<LevelCompletedEvent>.Deregister(_levelBind);
                //Debug.Log($"deregister event binds in {this} at {Time.time}");
            }
        }

        private void OnLevelCompleteTrigger(LevelCompletedEvent obj)
        {

            if (obj.CompletedLevel.NextLevel == null)
            {
                Debug.Log($"Level complete trigger has no next lv assigned, returning to main");
                OnReturnToMain();
            }
            else
            {
                RequestLoadSceneFromContainer(obj.CompletedLevel.NextLevel);
            }
        }
        #endregion


        public void OnReturnToMain()
        {
            DoLoadScene(_mainScene.SceneLoaderIndex);
        }

        public void QuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#endif
        }



    }


}