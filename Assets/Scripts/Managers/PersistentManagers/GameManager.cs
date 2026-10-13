//using Newtonsoft.Json;

using Arcatech.SaveSystem;
using Arcatech.Scenes;
using Arcatech.UI;
using DG.Tweening;
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
       

        private void OnEnable()
        {
            DOTween.Init();
            _dataManager = DataManager.Instance;
        }

        #endregion

        #region levels management

        private DataManager _dataManager;


        [Space(10f), SerializeField] private SceneContainer _mainScene;
        [SerializeField] private SceneContainer _galleryScene;
        [SerializeField] private SceneContainer _newgameScene;
        [Tooltip("Сцена «Выбор уровня». «Продолжить» ведёт сюда, если нет сохранённого чекпоинта.")]
        [SerializeField] private SceneContainer _levelSelectScene;

        public SceneContainer GetCurrentLevelData { get => _currentLevel; }
        private SceneContainer _currentLevel;

        private bool _equipsDone = false;
        private SceneContainer _cachedGameLevel;

        void DoLoadScene(int index)
        {            
            Debug.Log($"Loading scene index {index} - {SceneManager.GetSceneByBuildIndex(index).name}\nCurrent level is: {SceneManager.GetActiveScene().name}");       
            _currentLevel = _dataManager.GetSceneContainer(index);            
            SceneManager.LoadScene(index);

        }
        #endregion
        #region save system

        /// <summary>Есть что продолжать: чекпоинт уровня или пройденные уровни.</summary>
        public bool HasSaveToContinue =>
            SaveManager.Instance.HasCheckpoint || SaveManager.Instance.MetaProgress.HasAnyProgress;

        /// <summary>«Продолжить»: уровень с сохранённого чекпоинта, иначе - «Выбор уровня».</summary>
        public void OnContinueButton()
        {
            if (SaveManager.Instance.TryGetCheckpoint(out var checkpoint) && checkpoint.sceneBuildIndex >= 0)
            {
                // LevelProgressController на загруженной сцене сам найдёт чекпоинт и применит его.
                DoLoadScene(checkpoint.sceneBuildIndex);
                return;
            }

            var target = _levelSelectScene != null ? _levelSelectScene : _newgameScene;
            DoLoadScene(target.SceneLoaderIndex);
        }

        /// <summary>Переход в сцену «Выбор уровня» (например, из «Сводки прохождения уровня»).</summary>
        public void OnLevelSelectButton()
        {
            var target = _levelSelectScene != null ? _levelSelectScene : _mainScene;
            DoLoadScene(target.SceneLoaderIndex);
        }

        /// <summary>Запуск уровня из «Выбора уровня». Старый чекпоинт (если был) стирается.</summary>
        public void StartLevel(SceneContainer level)
        {
            if (level == null) return;
            SaveManager.Instance.ClearCheckpoint();
            DoLoadScene(level.SceneLoaderIndex);
        }

        #endregion

        #region UNITY UI
        /// <summary>«Новая игра»: стирает прогресс (подтверждение показывает меню) и запускает первую сцену.</summary>
        public void OnStartNewGameButton()
        {
            SaveManager.Instance.ResetGameProgress();
            DoLoadScene(_newgameScene.SceneLoaderIndex);
        }
        public void OnGalleryButton()
        {
            DoLoadScene(_galleryScene.SceneLoaderIndex);
        }
        public void OnExitButton()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#endif
            Application.Quit();
        }
        public void OnReturnToMain()
        {
            DoLoadScene(_mainScene.SceneLoaderIndex);
        }
        
        #endregion
    }


}