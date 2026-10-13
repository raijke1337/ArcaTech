using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;
using Arcatech.Scenes;
using Arcatech.Triggers;
using Arcatech.Usables;
using Arcatech.Usables.Effects;
using UnityEngine;

namespace Arcatech.Managers
{
    public class DataManager : GenericLazySingleton<DataManager>, IDifficultyDamageProvider
    {
        
        public static class GameRules
        {
            public const string ValidHitsLayer = "Entities";
            public const string InvalidHitsLayer = "SolidObject";
        }

        private void Awake()
        {
            TriggerLayerUtility.InvalidateCache();
        }

        public float GetOutgoingMult(BaseGameEntityComponent _) => 1f;
        public float GetIncomingMult(BaseGameEntityComponent _) => 1f;


        #region SceneContainers
        private List<SceneContainer> _scenes;
        
        public SceneContainer GetSceneContainer(int index)
        {
            EnsureScenes();
            return _scenes.FirstOrDefault(t=>t.SceneLoaderIndex == index);
        }


        private void EnsureScenes()
        {
            _scenes ??= new List<SceneContainer>(Resources.FindObjectsOfTypeAll<SceneContainer>());
        }

        /// <summary>Игровые уровни, открытые по умолчанию или в мета-прогрессе.</summary>
        public List<SceneContainer> GetAvailableLevels
        {
            get
            {
                EnsureScenes();
                var meta = SaveSystem.SaveManager.Instance.MetaProgress;
                return _scenes
                    .Where(s => s.LevelType == LevelType.Game)
                    .Where(s => s.IsUnlockedByDefault
                                || (meta.TryGetLevelRecord(GetLevelID(s), out var r) && r.isUnlocked))
                    .Distinct()
                    .OrderBy(s => s.SceneLoaderIndex)
                    .ToList();
            }
        }

        /// <summary>ID уровня в сохранениях - имя сцены (так же его определяет LevelProgressController).</summary>
        public static string GetLevelID(SceneContainer container)
        {
            if (container == null) return string.Empty;
            var path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(container.SceneLoaderIndex);
            return System.IO.Path.GetFileNameWithoutExtension(path);
        }
        #endregion

        

        #region items

        [Tooltip("Все ItemSO игры. Нужен, чтобы в билде собрать предмет по ID из сохранения, " +
                 "даже если на него нет ссылок в загруженной сцене.")]
        [SerializeField] private List<ItemSO> itemsRegistry = new();

        private Dictionary<string, ItemSO> _itemsData;

        private void EnsureItemsDatabase()
        {
            if (_itemsData != null) return;
            _itemsData = new Dictionary<string, ItemSO>();

            foreach (var item in itemsRegistry)
            {
                if (item != null) _itemsData[item.ID] = item;
            }
            // Дополнительно - всё, что уже загружено (в редакторе это обычно все ассеты).
            foreach (var item in Resources.FindObjectsOfTypeAll<ItemSO>())
            {
                if (item != null && !string.IsNullOrEmpty(item.ID)) _itemsData.TryAdd(item.ID, item);
            }
            Debug.Log($"Init items database. Total items: {_itemsData.Count}");
        }

        public bool TryGetItemConfig(string id, out ItemSO config)
        {
            EnsureItemsDatabase();
            config = null;
            return !string.IsNullOrEmpty(id) && _itemsData.TryGetValue(id, out config);
        }

        public Item MakeItem(ItemSO config, BaseGameEntityComponent owner)
        {
            return config switch
            {
                UsablesSO weapon => weapon.BuildItem(owner),
                EquipSO eq => eq.BuildItem(owner),
                _ => config.BuildItem(owner)
            };
        }

        /// <returns>null, если предмета с таким ID нет в базе.</returns>
        public Item MakeItem(string id, BaseGameEntityComponent owner)
        {
            if (!TryGetItemConfig(id, out var config))
            {
                Debug.LogError($"No ItemSO with ID {id}! Add it to DataManager.itemsRegistry.");
                return null;
            }
            return MakeItem(config, owner);
        }

#if UNITY_EDITOR
        /// <summary>Заполняет реестр всеми ItemSO проекта. Запускать после добавления новых предметов.</summary>
        [ContextMenu("Собрать все ItemSO в реестр")]
        private void CollectAllItems()
        {
            UnityEditor.Undo.RecordObject(this, "Collect ItemSO into registry");
            itemsRegistry = UnityEditor.AssetDatabase.FindAssets("t:ItemSO")
                .Select(g => UnityEditor.AssetDatabase.LoadAssetAtPath<ItemSO>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)))
                .Where(i => i != null)
                .OrderBy(i => i.name)
                .ToList();
            UnityEditor.EditorUtility.SetDirty(this);
            _itemsData = null;
            Debug.Log($"DataManager: itemsRegistry содержит {itemsRegistry.Count} предметов.", this);
        }
#endif

        #endregion


    }
}
