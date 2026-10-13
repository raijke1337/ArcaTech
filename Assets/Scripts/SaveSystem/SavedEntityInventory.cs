using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>Снимок живой модели инвентаря. Одинаковые предметы
        /// (разные экземпляры Item с одним ID) складываются в одну запись.</summary>
        public static SavedEntityInventory FromModel(UnitInventoryModel model, string entityID)
        {
            var result = new SavedEntityInventory { EntityID = entityID };
            if (model == null) return result;

            result.EntityEquipmentIDs = model.ListEquipped
                .Where(e => e != null)
                .Select(e => e.ID)
                .ToList();

            var counts = new Dictionary<string, int>();
            foreach (var pair in model.ListInventory)
            {
                if (pair.Key == null || pair.Value <= 0) continue;
                counts.TryGetValue(pair.Key.ID, out var c);
                counts[pair.Key.ID] = c + pair.Value;
            }

            result.EntityItemIDs = counts.Keys.ToArray();
            result.EntityItemsCount = counts.Values.ToArray();
            return result;
        }

        public List<Item> GetEquipment(BaseGameEntityComponent owner)
        {
            var equipList = new List<Item>();
            if (EntityEquipmentIDs == null) return equipList;

            foreach (var equip in EntityEquipmentIDs)
            {
                var item = DataManager.Instance.MakeItem(equip, owner);
                if (item != null) equipList.Add(item);
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
                var item = DataManager.Instance.MakeItem(EntityItemIDs[i], owner);
                if (item != null) invList[item] = EntityItemsCount[i];
            }
            return invList;
        }
    }
}
