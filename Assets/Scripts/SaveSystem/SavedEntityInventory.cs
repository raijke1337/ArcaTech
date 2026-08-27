using System;
using System.Collections.Generic;
using Arcatech.Items;
using Arcatech.Managers;

namespace Arcatech.SaveSystem
{
    [Serializable]
    public class SavedEntityInventory : IEntityItemsList
    {
        public string EntityID;
        public List<string> EntityEquipmentIDs = new();
        public string[] EntityItemIDs = Array.Empty<string>();
        public int[] EntityItemsCount = Array.Empty<int>();

        public SavedEntityInventory() { }

        /// <summary>Честный deep copy - иначе checkpoint- и current-снапшоты
        /// будут делить один и тот же список/массив, и мутация одного
        /// незаметно испортит другой.</summary>
        public SavedEntityInventory(SavedEntityInventory other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            EntityID = other.EntityID;
            EntityEquipmentIDs = other.EntityEquipmentIDs != null
                ? new List<string>(other.EntityEquipmentIDs)
                : new List<string>();
            EntityItemIDs = other.EntityItemIDs != null
                ? (string[])other.EntityItemIDs.Clone()
                : Array.Empty<string>();
            EntityItemsCount = other.EntityItemsCount != null
                ? (int[])other.EntityItemsCount.Clone()
                : Array.Empty<int>();
        }

        public List<Item> GetEquipment(BaseGameEntityComponent owner)
        {
            var equipList = new List<Item>();
            if (EntityEquipmentIDs == null) return equipList;

            foreach (var equip in EntityEquipmentIDs)
            {
                equipList.Add(DataManager.Instance.MakeItem(equip, owner));
            }
            return equipList;
        }

        public Dictionary<Item, int> GetInventory(BaseGameEntityComponent owner)
        {
            var invList = new Dictionary<Item, int>();
            if (EntityItemIDs == null || EntityItemsCount == null) return invList;

            if (EntityItemIDs.Length != EntityItemsCount.Length)
            {
                UnityEngine.Debug.LogError(
                    $"SavedEntityInventory: EntityItemIDs.Length ({EntityItemIDs.Length}) != " +
                    $"EntityItemsCount.Length ({EntityItemsCount.Length}). Data is corrupted.");
                return invList;
            }

            for (int i = 0; i < EntityItemIDs.Length; i++)
            {
                invList[DataManager.Instance.MakeItem(EntityItemIDs[i], owner)] = EntityItemsCount[i];
            }
            return invList;
        }
    }
}