using Arcatech.Items;
using Arcatech.Managers;
using Arcatech.Managers.Save;
using Arcatech.Scenes;
using Arcatech.Triggers;
using Arcatech.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace Arcatech
{

    #region saves


    #endregion

    #region const
    public static class Constants
    {
        public static class Configs
        {
            public const string c_AllConfigsPath = "/Resources/Configurations/";
            public const string c_SavesPath = "/Saves/Save.xml";
            public const string c_LevelsPath = "/Resources/Levels";
        }
        public static class Objects
        {
            public const string c_isoCameraTargetObjectName = "IsoCamTarget";
        }
        public static class Combat
        {
            public const float c_RemainsDisappearTimer = 3f;
            public const float c_StaggeringHitHealthPercent = 0.1f; // 10% max hp
        }
        public static class PrefabsPaths
        {
            public const string c_ItemPrefabsPath = "/Resources/Prefabs/Items/";
            public const string c_SkillPrefabs = "/Resources/Prefabs/Skills/";
            public const string c_InterfacePrefabs = "/Resources/Prefabs/Interface/";
        }
        public static class Texts
        {
            public const string c_TextsPath = "/Resources/Texts/";
            public const string c_WeaponsDesc = "Assets/Resources/Texts/Descriptions/Weapons/";
            public const string c_SkillsDesc = "Assets/Resources/Texts/Descriptions/Skills/";
        }
        public static class StateMachineData
        {
            public const string c_MethodPrefix = "Fsm_";
        }

        #endregion
        #region tools

    }

    #endregion


    #region items

    // used for npc inventory init
    [Serializable]
    public class UnitInventoryItemConfigsContainer
    {
        public string ID;
        [SerializeField] public List<Equip> Equipment;
        [SerializeField, Space] public List<Item> Inventory;
        public UnitInventoryItemConfigsContainer(UnitItemsSO cfg)
        {
            Equipment = new List<Equip>(cfg.Equipment);
            Inventory = new List<Item>();

            foreach (Item i in cfg.Inventory)
            {
                if (i is Equip e)
                {
                    Inventory.Add(e);
                }
                else
                {
                    Inventory.Add(i);
                }
            }
            ID = cfg.ID;
        }
        // used for player inventory load
        public UnitInventoryItemConfigsContainer(SerializedUnitInventory inv)
        {
            Equipment = new List<Equip>();
            Inventory = new List<Item>();

            foreach (string id in inv.Inventory)
            {
                var i = DataManager.Instance.GetConfigByID<Item>(id);
                if (i is Equip e)
                {
                    Inventory.Add(e);
                }
                else
                {
                    Inventory.Add(i);
                }
            }
            foreach (string id in inv.Equips)
            {
                var i = DataManager.Instance.GetConfigByID<Item>(id);
                if (i is Equip e)
                {
                    Equipment.Add(e);
                }
            }
        }
        // fpr saving
        public UnitInventoryItemConfigsContainer(UnitInventoryComponent cfg)
        {
            Equipment = new List<Equip>();
            Inventory = new List<Item>();

            foreach (var item in cfg.GetCurrentInventory)
            {
                var i = DataManager.Instance.GetConfigByID<Item>(item.ID);
                if (i is Equip e)
                {
                    Inventory.Add(e);
                }
                else
                {
                    Inventory.Add(i);
                }
            }
            foreach (var item in cfg.GetCurrentEquips)
            {
                var i = DataManager.Instance.GetConfigByID<Item>(item.ID);
                if (i is Equip e)
                {
                    Equipment.Add(e);
                }
                else
                {
                    Debug.Log($"Trying to save {item} as equipped and failed");
                }
            }


        }
        public UnitInventoryItemConfigsContainer()
        {

        }
    }

    [Serializable]
    public class RangedWeaponConfig
    {

        public SerializedProjectileConfiguration Projectile;

        [Space,SerializeField, Range(1, 20), Tooltip("How projectiles will be spawned until reload is started")] public int Ammo;
        [SerializeField, Range(1, 12), Tooltip("How many projectiles are created per each use")] public int ShotsPerUse;
        [SerializeField, Range(0, 5), Tooltip("Time in reload")] public float Reload;
        [SerializeField, Range(0, 1), Tooltip("Spread of shots (inaccuaracy)")] public float Spread;
    }

    #endregion

}