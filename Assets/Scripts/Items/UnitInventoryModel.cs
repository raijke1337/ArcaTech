using Arcatech.Managers;
using Arcatech.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Items
{
    [Serializable]
    public class UnitInventoryModel
    {
        UnitInventoryModel()
        {
            status = "not init";
        }

        #region serialize
        [SerializeField][ReadOnlyText] string status;
        #endregion

        public BaseGameEntityComponent Owner {get;}
        public UsablesHandler Handler { get; protected set; }
        public ObservableArray<Item> Inventory { get; protected set; }
        public ObservableDictionary<EquipmentType, Equipment> Equipments { get; protected set; }

        public event UnityAction DrawStrategyChangedEvent;
        public IDrawItemStrategy CurrentDrawStrategy { get; private set; }

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


        public void UpdateModel(float delta)
        {
            Handler?.Update(delta);
            status = $"updating OK {Owner.GetName}";
        }


        public UnitInventoryModel(UnitInventoryItemConfigsContainer cfgs,BaseGameEntityComponent o)
        {
            Owner = o;  
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
            Handler = new UsablesHandler(Equipments);
            Handler.DrawStrategyUpdateEvent += OnDrawStrategyUpdate;

            status = $"init for {Owner.GetName}";
        }

        private void OnDrawStrategyUpdate(IDrawItemStrategy t)
        {
            CurrentDrawStrategy = t;
            DrawStrategyChangedEvent?.Invoke();
        }

        public bool PickUpItem (ItemSO item) => Inventory.TryAdd(Itemfactory.Instance.ProduceItem(item, Owner) as Item);
        public bool EquipItem (EquipSO item, out Equipment unequipped)
        {
            unequipped = null;
            var eq = Itemfactory.Instance.ProduceItem(item, Owner) as Equipment;
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