using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;

namespace Arcatech.SaveSystem
{
    [Serializable]
    public class GameData
    {
        public int version;
        public string timestamp;
        public List<LevelProgressData> levelRecords = new();
        public List<SavedEntityInventory> inventoryRecords = new();

        public void AddOrUpdateInventory(SavedEntityInventory toAdd)
        {
            var existing = inventoryRecords.Find(t=>t.EntityID ==  toAdd.EntityID);
            if (existing != null) inventoryRecords.Remove(existing);
            inventoryRecords.Add(toAdd);
        }

        public bool TryGetInventoryForEntity(string entityID, out IEntityItemsList inventory)
        {
            if (inventoryRecords.Any(t => t.EntityID == entityID))
            {
                inventory = inventoryRecords.First(t => t.EntityID == entityID);
                return true;
            }
            inventory = null;
            return false;
        }
    }
}