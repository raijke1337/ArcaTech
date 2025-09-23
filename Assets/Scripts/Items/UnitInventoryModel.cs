using Arcatech.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [SerializeField] public UsablesHandler Handler;
        List<Item> inventory;
        Dictionary<ItemType, Equipment> equipments;

        public IReadOnlyList<Equipment> ListEquipped => equipments.Values.ToList().AsReadOnly();
        public IReadOnlyList<Item> ListInventory => inventory.AsReadOnly();

        bool initialized = false;

        public event UnityAction<IDrawItemStrategy> DrawStrategyChangedEvent = delegate { };
        public event UnityAction ModelUpdatedEvent = delegate { };

        public UnitInventoryModel(IEntityItemsList items, BaseGameEntityComponent o)
        {


            inventory = new();
            equipments = new();

            PickUpItems(items.GetInventory(o));

            Dictionary<ItemType, List<Equipment>> equipmentDict = new();
            

            foreach (Equipment e in items.GetEquipment(o))
            {
                if (!equipmentDict.ContainsKey(e.Type))
                {
                    equipmentDict[e.Type] = new List<Equipment>();
                }
                equipmentDict[e.Type].Add(e);
               // EquipItem(e, out _);
            }
            // here is the case if multiple items of the same type are in the "equipment".
            // pick a random and more the rest to inventory

            foreach (var pair in equipmentDict)
            {
                var selected =  pair.Value[Random.Range(0, pair.Value.Count-1)];
                EquipItem(selected, out _);
            }
            
            // for example small bots have different items that they equip but with the same stats

            Handler = new UsablesHandler();
            Handler.DrawStrategyUpdateEvent += OnDrawStrategyUpdate;

            status = $"init";
            initialized = true;
            ModelUpdatedEvent.Invoke();
        }

        private void OnDrawStrategyUpdate(IDrawItemStrategy t) => DrawStrategyChangedEvent.Invoke(t);

        public void UpdateDeltaModel(float delta)
        {
            Handler?.Update(delta);
            status = $"updating OK";
        }

        public void PickUpItem(Item item)
        {
            inventory.Add(item);
            if (initialized) ModelUpdatedEvent.Invoke();
        }

        public void PickUpItems(IEnumerable<IItem> items)
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

            if (equipments.TryGetValue(toEquip.Type, out var drop))
            {
                dropped = drop.Config as EquipSO;
                equipments.Remove(toEquip.Type);
                drop.OnUnequip();
            }

            equipments[toEquip.Type] =toEquip;

            if (initialized)
            {
                if (toEquip is IWeapon w)
                {
                    DrawStrategyChangedEvent.Invoke(w.DrawStrategy);
                }
                ModelUpdatedEvent.Invoke();
            }
        }


        public StatsMod[] GetCurrentMods
        {
            get
            {
                var list = new List<StatsMod>();
                foreach (var equip in equipments)
                {
                    if (equip.Value.StatMods != null)
                    {
                        list.AddRange(equip.Value.StatMods);
                    }
                }
                return list.ToArray();
            }
        }


    }


}