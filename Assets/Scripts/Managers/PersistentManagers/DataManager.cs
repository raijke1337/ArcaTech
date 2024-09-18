using Arcatech.EventBus;
using Arcatech.Items;
using Arcatech.Managers.Save;
using Arcatech.Scenes;
using Arcatech.Units;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Arcatech.Managers
{

    public class DataManager : MonoBehaviour
    {
        public Itemfactory ItemsFactory;
        public static DataManager Instance;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                ItemsFactory = new Itemfactory();
                _bindInv = new EventBinding<InventoryUpdateEvent>(OnInventoryUpdate);
                _bindLvls = new EventBinding<LevelCompletedEvent>(OnLevelComplete);

                SaveService = new SavesHandler(new JsonSerializer());
                ReloadSave();

                EventBus<InventoryUpdateEvent>.Register(_bindInv);
                EventBus<LevelCompletedEvent>.Register(_bindLvls);

            }

            else Destroy(gameObject);
        }

        private void OnDisable()
        {
            EventBus<InventoryUpdateEvent>.Deregister(_bindInv);
            EventBus<LevelCompletedEvent>.Deregister(_bindLvls);
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

        internal UnitInventoryItemConfigsContainer GetPlayerSaveEquips
        {
            get
            {
                return new UnitInventoryItemConfigsContainer(_loadedSave.Inventory);
            }
        }
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

        public bool PlayerHasItem(ItemSO item)
        {
            return _loadedSave.Inventory.Inventory.Contains(item) || _loadedSave.Inventory.Equipment.Contains(item);
        }


        #endregion





        #region saving

        private GameSaveData _loadedSave;

        private ISavesService SaveService;
        EventBinding<InventoryUpdateEvent> _bindInv;
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
            SaveService.Save(_loadedSave);
        }

        #endregion
        #region observing channels


        private void OnLevelComplete(LevelCompletedEvent lvl)
        {
            _loadedSave.OpenedLevelsID.Add(lvl.CompletedLevel.ID.ToString());
        }

        private void OnInventoryUpdate(InventoryUpdateEvent arg)
        {
            if (_loadedSave == null)
            {
                return;
            }
            // for debug use
            if (arg.Unit is PlayerUnit)
            {
                _loadedSave.UpdateInventory(arg.Inventory.PackPlayerData());
            }
        }

        #endregion

        private void OnApplicationQuit()
        {
            SaveGame();
        }


        public class Itemfactory
        {
            public IItem ProduceItem (ItemSO cfg, EquippedUnit owner)
            {
                return cfg.Type switch
                {
                    EquipmentType.MeleeWeap => new Weapon(cfg as WeaponSO, owner),
                    EquipmentType.RangedWeap => new Weapon(cfg as WeaponSO, owner),
                    EquipmentType.Shield => new Shield(cfg as ShieldSO, owner),
                    EquipmentType.Booster => new Equipment(cfg as EquipSO, owner),
                    _ => new Item(cfg, owner),
                };
            }
        }




    }
}
