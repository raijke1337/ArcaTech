using System;
using System.Collections.Generic;
using Arcatech.Items;
using Arcatech.Managers;

namespace Arcatech.SaveSystem
{
    [Serializable]
    public class SavedEntityInventory : IEntityItemsList
    {
        public SavedEntityInventory()
        { }
        
        public string EntityID;
        public List<string> EntityEquipmentIDs;
        public string[] EntityItemIDs;
        public int[] EntityItemsCount;
        public List<Item> GetEquipment(BaseGameEntityComponent owner)
        {
            List<Item> equipList = new List<Item>();

            foreach (string equip in EntityEquipmentIDs)
            {
                equipList.Add(DataManager.Instance.MakeItem(equip,owner));
            }
            return equipList;
        }

        public Dictionary<Item, int> GetInventory(BaseGameEntityComponent owner)
        {
            Dictionary<Item, int> invList = new();

            for (int i = 0; i < EntityItemIDs.Length; i++)
            {
                invList[DataManager.Instance.MakeItem(EntityEquipmentIDs[i],owner)] = EntityItemsCount[i];
            }
            return invList;
        }
    }
}