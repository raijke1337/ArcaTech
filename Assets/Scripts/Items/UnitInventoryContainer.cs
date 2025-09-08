using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items
{
    [Serializable]
    public class UnitInventoryContainer
    {
        [SerializeField] public List<Equipment> Equipment;
        [SerializeField] public List<Item> Inventory;

        // used to load items from save
        //public UnitInventoryContainer(UnitInventoryContainer cfg)
        //{

        //    Equipment = new List<Equipment>();
        //    Inventory = new List<Item>();

        //    if (cfg != null)
        //    {
        //        Equipment.AddRange(cfg.Equipment);
        //        Inventory.AddRange(cfg.Inventory);
        //    }
        //}

        // used for default inventory load
        //public UnitInventoryContainer(UnitItemsSO cfg)
        //{

        //    Equipment = new List<ItemSO>();
        //    Inventory = new List<ItemSO>();
        //    if (cfg != null)
        //    {
        //        Equipment.AddRange(cfg.Equipment);
        //        Inventory.AddRange(cfg.Inventory);
        //    }
        //}
        //// used to pack save data
        //public UnitInventoryContainer(List<Item> equipment, List<Item> inventory)
        //{
        //    foreach (Equipment item in equipment)
        //    {
        //        Equipment.Add(item.Config);
        //    }
        //    foreach (Item inventoryItem in inventory)
        //    {
        //        Inventory.Add(inventoryItem.Config);
        //    }
        //}
    }




}