using Arcatech.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Arcatech.Items
{
    [Serializable]
    public class UnitInventoryModel
    {

        #region serialize

       [SerializeField] [ReadOnlyText] string status = "not init";

        #endregion

       
        bool initialized = false;
        List<Item> inventory;
        Dictionary<ItemSlot, Equipment> equipments;

        public IReadOnlyList<Equipment> ListEquipped => equipments.Values.ToList().AsReadOnly();
        public IReadOnlyList<Item> ListInventory => inventory.AsReadOnly();
        public event UnityAction ModelUpdatedEvent = delegate { };

        public UnitInventoryModel(IEntityItemsList items, BaseGameEntityComponent o)
        {

            inventory = new();
            equipments = new();
            
            status = $"No items loaded!";
            
            if (items == null) return;
            
            PickUpItems(items.GetInventory(o));
            Dictionary<ItemSlot, List<Equipment>> temporaryDict = new();
            

            foreach (Equipment e in items.GetEquipment(o))
            {
                if (!temporaryDict.ContainsKey(e.Slot))
                {
                    temporaryDict[e.Slot] = new List<Equipment>();
                }
                temporaryDict[e.Slot].Add(e);
               // EquipItem(e, out _);
            }
            // here is the case if multiple items of the same type are in the "equipment".
            // pick a random and more the rest to inventory

            foreach (var pair in temporaryDict)
            {
                var randomIndex = Random.Range(0, pair.Value.Count-1);
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    if (i == randomIndex)
                    {
                        EquipItem(pair.Value[i],out _);
                    }
                    else
                    {
                        PickUpItem(pair.Value[i]);
                    }
                }
            }
            
            // for example small bots have different items that they equip but with the same stats

            status = $"items loaded";
            initialized = true;
            ModelUpdatedEvent.Invoke();
        }
        
        public void PickUpItem(Item item)
        {
            inventory.Add(item);
            if (initialized) 
            {ModelUpdatedEvent.Invoke();}
        }

        public void PickUpItems(IEnumerable<Item> items)
        {
            foreach (Item item in items) PickUpItem(item);
        }


        public bool HasItem(ItemSO item)
        {
            return inventory.FirstOrDefault(t => t.ID == item.ID) != null;
        }
        public bool DropItem(Item item)
        {
            if (item == null) return false;
            if (inventory.Contains(item))
            {
                inventory.Remove(item);
                return true;
            }

            return false;
        }
    
    public bool DropItem(ItemSO item)
        {
            var it = inventory.First(t => t.ID == item.ID);
            return DropItem(it);    
        }


        public void EquipItem (Equipment toEquip, out EquipSO dropped)
        {
            dropped = null;

            if (equipments.TryGetValue(toEquip.Slot, out var drop))
            {
               // dropped = drop.Config as EquipSO;
                equipments.Remove(toEquip.Slot);
                drop.OnUnequip();
                
            }

            equipments[toEquip.Slot] =toEquip;

            if (initialized) ModelUpdatedEvent.Invoke();
        }
        
        /// <summary>
        /// replaces the getcurrentmods
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IEquipmentStatsProvider> EnumerateProviders()
        {

            var list =  new List<IEquipmentStatsProvider>();
            foreach (var equipment in equipments.Values)
            {
                list.Add(equipment);
            }
            return list;
        }


    }


}