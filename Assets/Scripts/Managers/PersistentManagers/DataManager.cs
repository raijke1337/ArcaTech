using Arcatech.EventBus;
using Arcatech.Items;
using Arcatech.Managers.Save;
using Arcatech.Scenes;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Usables;
using UnityEngine;

namespace Arcatech.Managers
{
    public class DataManager : GenericLazySingleton<DataManager>
    {

        // awake is from generic class for now

        //private void Awake()
        //{
        //    if (_instance == null)
        //    {
        //        _instance = this;
        //        _bindLvls = new EventBinding<LevelCompletedEvent>(OnLevelComplete);


        //        SaveService = new SavesHandler(new JsonSerializer());
        //        ReloadSave();

        //        EventBus<LevelCompletedEvent>.Register(_bindLvls);
        //      //  Debug.Log($"register event binds in {this} at {Time.time}");
        //    }

        //    else Destroy(gameObject);
        //}

        private void OnDisable()
        {
            EventBus<LevelCompletedEvent>.Deregister(_bindLvls);
           // Debug.Log($"deregister event binds in {this} at {Time.time}");
        }

        #region SceneContainers
        private List<SceneContainer> _scenes;
        
        public SceneContainer GetSceneContainer(int index)
        {
            if (_scenes == null)
            {
                _scenes = new List<SceneContainer>(Resources.FindObjectsOfTypeAll<SceneContainer>());
            }
            return _scenes.FirstOrDefault(t=>t.SceneLoaderIndex == index);
        }



        #endregion


        #region external checks
        bool _newGame = true;
        public bool IsNewGame
        {
            get
            {
                Debug.Log("TODO: new game check");
                return _newGame;
            }
            set
            {
                _newGame = value;
            }
        }

        //internal UnitInventoryContainer GetPlayerSaveEquips
        //{
        //    get
        //    {
        //        return new UnitInventoryContainer(_loadedSave.Inventory);
        //    }
        //}
        public List <SceneContainer> GetAvailableLevels
        {
            get
            {
                var containers = _scenes.Where((t) =>
                {
                   return _scenes.First((q) => t.ID == q.ID);
                }
                );
                Debug.Log($"check this : found {containers.Count()} unlocked levels");
                return null;
            }
        }

        //public bool PlayerHasItem(ItemSO item)
        //{
        //    return _loadedSave.Inventory.Inventory.Contains(item) || _loadedSave.Inventory.Equipment.Contains(item);
        //}


        #endregion

        #region items

        public Item MakeItem(ItemSO config, BaseGameEntityComponent owner)
        {
            return config switch
            {
                UsablesSO weapon => weapon.BuildItem(owner),
                EquipSO eq => eq.BuildItem(owner),
                _ => config.BuildItem(owner)
            };
        }

        #endregion



        #region saving

        private GameSaveData _loadedSave;

        private ISavesService SaveService;
        EventBinding<LevelCompletedEvent> _bindLvls;
        public void OnNewGame()
        {
            _newGame = true;
            ReloadSave();
        }

        public void ReloadSave()
        {
            _loadedSave = SaveService.Load();
        }
        public void SaveGame()
        {
           // SaveService.Save(_loadedSave);
        }

        #endregion
        #region observing channels


        private void OnLevelComplete(LevelCompletedEvent lvl)
        {
           // _loadedSave.OpenedLevelsID.Add(lvl.CompletedLevel.ID.ToString());
        }


        #endregion

        private void OnApplicationQuit()
        {
            SaveGame();
        }
    }
}
