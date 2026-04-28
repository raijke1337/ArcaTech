using Arcatech.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using Arcatech.SaveSystem;
using UnityEngine;
using UnityEngine.Assertions;
namespace Arcatech.Items
{
    [Serializable]
    [CreateAssetMenu(fileName = "inventory_", menuName = "Items/Inventory preset", order = 3)]
    public class UnitItemsSO : ScriptableObjectID, IEntityItemsList
    {
        [SerializeField] List<EquipSO> Equipment;
        [SerializeField, Space] List<ItemSO> Inventory;

        public List<Item> GetEquipment(BaseGameEntityComponent owner)
        {
            List<Item> equipList = new List<Item>();

            foreach (EquipSO equip in Equipment)
            {
                equipList.Add(DataManager.Instance.MakeItem(equip,owner));
            }
            return equipList;
        }

        public List<Item> GetInventory(BaseGameEntityComponent owner)
        {
            List<Item> invList = new List<Item>();
            foreach (ItemSO item in Inventory)
            {
                invList.Add(DataManager.Instance.MakeItem(item, owner));
            }
            return invList;
        }

        private void OnValidate()
        {
            foreach (var item in Inventory)
            {
                Assert.IsNotNull(item);
            }
            foreach (var item in Equipment)
            {
                Assert.IsNotNull(item);
            }
        }

    }


    public interface IEntityItemsList
    {
        public List<Item> GetEquipment(BaseGameEntityComponent owner);
        public List<Item> GetInventory(BaseGameEntityComponent owner);


    }
}