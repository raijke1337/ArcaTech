using Arcatech.Managers;
using Arcatech.Units;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arcatech.Items
{
    public class UnitInventoryModel
    {
        public BaseGameEntityComponent Owner {get;}
        public ObservableArray<Item> Inventory { get; protected set; }
        public ObservableDictionary<EquipmentType, Equipment> Equipments { get; protected set; }


        public event Action<IEnumerable<Item>> OnInventoryChange
        {
            add => Inventory.AnyRecordChanged += value;
            remove => Inventory.AnyRecordChanged -= value;
        }
        public event Action<IEnumerable<Equipment>> OnEquipsChange
        {
            add => Equipments.AnyValueChanged += value;
            remove => Equipments.AnyValueChanged -= value;
        }


        public UnitInventoryModel(UnitInventoryItemConfigsContainer cfgs)
        {
          //  Owner = owner;  
            Inventory = new ObservableArray<Item>();
            List<KeyValuePair<EquipmentType, Equipment>> list = new();
            Equipments = new ObservableDictionary<EquipmentType, Equipment>(list.ToArray());


            foreach (ItemSO item in cfgs.Inventory)
            {
                PickUpItem(item);
            }
            foreach (EquipSO e in cfgs.Equipment)
            {
                EquipItem(e, out var un);
                if (un != null)
                {
                    PickUpItem(un.Config);
                }
            }
        }
        public bool PickUpItem (ItemSO item) => Inventory.TryAdd(DataManager.Instance.ItemsFactory.ProduceItem(item, Owner) as Item);
        public bool EquipItem (EquipSO item, out Equipment unequipped)
        {
            unequipped = null;
            var eq = DataManager.Instance.ItemsFactory.ProduceItem(item, Owner) as Equipment;
            if (Equipments.TryGetValue(eq.Type, out unequipped))
            {
                PickUpItem(unequipped.Config);
            }
            Equipments.SetPair(eq.Type, eq);
            //Add(new KeyValuePair<EquipmentType, Equipment>(eq.Type, eq as Equipment));
            return true;
        }
        public bool HasItem(ItemSO check)
        {
            if (check == null) return false;
            else
            {
                return Inventory.items.Any(t => t.ID == check.ID) || Equipments.GetAllValues().Any(t => t.ID == check.ID); 
            }
        }
    }


}