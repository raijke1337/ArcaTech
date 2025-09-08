using Arcatech.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
namespace Arcatech.Items
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Unit Items Preset", menuName = "Items/Inventory preset", order = 3)]
    public class UnitItemsSO : ScriptableObjectID, IEntityItemsList
    {
        [SerializeField] List<EquipSO> Equipment;
        [SerializeField, Space] List<ItemSO> Inventory;

        public List<IItem> GetEquipment(BaseGameEntityComponent owner)
        {
            List<IItem> equipList = new List<IItem>();

            foreach (EquipSO equip in Equipment)
            {
                equipList.Add(DataManager.Instance.MakeItem(equip,owner));
            }
            return equipList;
        }

        public List<IItem> GetInventory(BaseGameEntityComponent owner)
        {
            List<IItem> invList = new List<IItem>();
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
        public List<IItem> GetEquipment(BaseGameEntityComponent owner);
        public List<IItem> GetInventory(BaseGameEntityComponent owner);


    }
}