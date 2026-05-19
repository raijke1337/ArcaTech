using Arcatech.Stats;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private bool _initialized = false;
        Dictionary<Item,int> _inventory;
        Dictionary<ItemSlot, Equipment> _equipments;

        public IReadOnlyList<Equipment> ListEquipped => _equipments.Values.ToList().AsReadOnly();

        public IReadOnlyDictionary<Item, int> ListInventory
        {
            get
            {
                 var readOnlyDictionary =  new ReadOnlyDictionary<Item, int>(_inventory);
                 return readOnlyDictionary;
            }
        }
        public event UnityAction ModelUpdatedEvent = delegate { };

        public UnitInventoryModel(IEntityItemsList items, BaseGameEntityComponent o)
        {

            _inventory = new();
            _equipments = new();
            
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
                        EquipEquipment(pair.Value[i],out _);
                    }
                    else
                    {
                        PickUpItem(pair.Value[i],1);
                    }
                }
            }
            
            // for example small bots have different items that they equip but with the same stats
            _initialized = true;
            ModelUpdatedEvent.Invoke();
        }
        
        public void PickUpItem(Item item, int count)
        {
            _inventory.Add(item,count);
            if (_initialized)
            {
                ModelUpdatedEvent.Invoke();
            }
        }
        public void PickUpItems(IDictionary<Item,int> items)
        {
            foreach (var p in items) PickUpItem(p.Key,p.Value);
        }
        public bool HasItem(ItemSO item,int amount)
        {
            if (item == null) return false;
            var neededID = item.ID; 
            return SearchInventory(neededID,amount);
        }

        public bool HasItem(Item item, int amount)
        {
            if (item == null) return false;
            var neededID = item.ID; 
            return SearchInventory(neededID,amount);
        }

        public bool HasItem (string id, int amount) => SearchInventory(id,amount);
        private bool SearchInventory(string id, int amount)
        {
            var list = _inventory.Keys.ToList();
            var found = list.FirstOrDefault(x => x.ID == id);
            if (found == null) return false;
            return _inventory[found] >= amount;
        }
        
        public bool UseItem(ItemSO item,int amount)
        {
            if (!HasItem(item,amount)) return false;
            var neededID = item.ID; 
            var itemInQuestion = _inventory.Keys.First(x => x.ID == neededID);
            _inventory[itemInQuestion] -= amount;
            if (_inventory[itemInQuestion] == 0) _inventory.Remove(itemInQuestion);
            return true;
        }

        public void EquipEquipment (Equipment toEquip, out Equipment dropped)
        {
            dropped = null;

            if (_equipments.TryGetValue(toEquip.Slot, out var drop))
            {
                _equipments.Remove(toEquip.Slot);
                drop.OnUnequip();
                dropped = drop;
            }
            _equipments[toEquip.Slot] = toEquip;
            if (_initialized) ModelUpdatedEvent.Invoke();
        }
        
        /// <summary>
        /// replaces the getcurrentmods
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IEquipmentStatsProvider> EnumerateProviders()
        {

            var list =  new List<IEquipmentStatsProvider>();
            foreach (var equipment in _equipments.Values)
            {
                list.Add(equipment);
            }
            return list;
        }


    }


}