using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;
using Arcatech.Scenes;
using Arcatech.Usables;
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

        private Dictionary<string, ItemSO> _itemsData;
        
        public Item MakeItem(ItemSO config, BaseGameEntityComponent owner)
        {
            return config switch
            {
                UsablesSO weapon => weapon.BuildItem(owner),
                EquipSO eq => eq.BuildItem(owner),
                _ => config.BuildItem(owner)
            };
        }

        public Item MakeItem(string id, BaseGameEntityComponent owner)
        {
            if (_itemsData == null)
            {
                _itemsData = new Dictionary<string, ItemSO>();
                var found = Resources.FindObjectsOfTypeAll(typeof(ItemSO)) as ItemSO[];
                if (found == null)
                {
                    Debug.LogError($"No ItemSO found for ID {id}! Failed to init database.");
                    return null;
                }
                foreach (var item in found)
                {
                    _itemsData[item.ID.ToString()] = item;
                }
                Debug.Log($"Init items database. Total items: {_itemsData.Count}");
            }
            
            return MakeItem(_itemsData[id], owner);
        }

        
        #endregion
        
    }
}
