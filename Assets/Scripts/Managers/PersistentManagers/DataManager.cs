using System;
using Arcatech.EventBus;
using Arcatech.Items;
using Arcatech.Scenes;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Usables;
using TMPro.EditorUtilities;
using UnityEngine;

namespace Arcatech.Managers
{
    public class DataManager : GenericLazySingleton<DataManager>
    {
        
        public static class GameRules
        {
            public const string ValidHitsLayer = "Entities";
            public const string InvalidHitsLayer = "SolidObject";
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
        
    }
}
