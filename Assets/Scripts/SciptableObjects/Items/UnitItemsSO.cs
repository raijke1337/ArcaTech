using System;
using System.Collections.Generic;
using Arcatech.Managers;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Arcatech.Items
{
    [Serializable]
    [CreateAssetMenu(fileName = "inventory_", menuName = "Items/Inventory preset", order = 3)]
    public class UnitItemsSO : ScriptableObjectID, IEntityItemsList
    {
        [SerializeField] List<EquipSO> Equipment;
        [SerializeField, Space] SerializedDictionary<ItemSO,int> Inventory;

        public List<Item> GetEquipment(BaseGameEntityComponent owner)
        {
            List<Item> equipList = new List<Item>();

            foreach (EquipSO equip in Equipment)
            {
                equipList.Add(DataManager.Instance.MakeItem(equip,owner));
            }
            return equipList;
        }

        public Dictionary<Item, int> GetInventory(BaseGameEntityComponent owner)
        {
            Dictionary<Item, int> invList = new();
            foreach (var item in Inventory)
            {
                invList.Add(DataManager.Instance.MakeItem(item.Key, owner),item.Value);
            }
            return invList;
        }
    }
}