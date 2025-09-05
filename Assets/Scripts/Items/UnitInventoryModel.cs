using Arcatech.Stats;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Items
{
    [Serializable]
    public class UnitInventoryModel
    {

        #region serialize
        [SerializeField][ReadOnlyText] string status = "not init";
        #endregion

        public BaseGameEntityComponent Owner {get;}
        public UsablesHandler Handler { get; protected set; }
        List<Item> Inventory { get; set; }
        ObservableDictionary<EquipmentType, Equipment> Equipments { get;  set; }

        public event UnityAction DrawStrategyChangedEvent;
        public IDrawItemStrategy CurrentDrawStrategy { get; private set; }

        public event UnityAction<Item> ItemAddedToInventoryEvent = delegate { };
        public event UnityAction<Item> ItemRemovedFromInventoryEvent = delegate { };
        public event UnityAction<Equipment> ItemEquippedEvent = delegate { };
        public event UnityAction<Equipment> ItemUnequippedEvent = delegate { };


        //public event Action<IEnumerable<Item>> OnInventoryChange
        //{
        //    add => Inventory.AnyRecordChanged += value;
        //    remove => Inventory.AnyRecordChanged -= value;
        //}
        //public event Action<IEnumerable<Equipment>> OnEquipsChange
        //{
        //    add => Equipments.AnyValueChanged += value;
        //    remove => Equipments.AnyValueChanged -= value;
        //}


        public void UpdateDeltaModel(float delta)
        {
            Handler?.Update(delta);
            status = $"updating OK {Owner.GetName}";
        }


        public UnitInventoryModel(UnitInventoryItemConfigsContainer cfgs,BaseGameEntityComponent o)
        {
            Owner = o;
            //Inventory = new ObservableArray<Item>();
            // Equipments = new ObservableDictionary<EquipmentType, Equipment>(list.ToArray());

            List<KeyValuePair<EquipmentType, Equipment>> list = new();
            Inventory = new();
            Equipments = new ObservableDictionary<EquipmentType, Equipment>(list.ToArray());


            foreach (ItemSO item in cfgs.Inventory)
            {
                ItemAddedToInventoryEvent?.Invoke(PickUpItem(item));
            }
            foreach (EquipSO e in cfgs.Equipment)
            {
                ItemEquippedEvent.Invoke(EquipItem(e, out var un));
                if (un != null) { ItemUnequippedEvent.Invoke(un); }
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

        public Item PickUpItem (ItemSO item, int amount = 1)
        {
            var i = (Itemfactory.Instance.ProduceItem(item, Owner) as Item);
            Debug.Log($"added item {i} to inventory");
            Inventory.Add(i);
            return i;
        }
        Equipment EquipItem (EquipSO item, out Equipment unequipped)
        {
            unequipped = null;
            var eq = Itemfactory.Instance.ProduceItem(item, Owner) as Equipment;
            if (Equipments.TryGetValue(eq.Type, out unequipped))
            {
                PickUpItem(unequipped.Config);
            }
            Equipments.SetPair(eq.Type, eq);
            //Add(new KeyValuePair<EquipmentType, Equipment>(eq.Type, eq as Equipment));
            return eq;
        }


        public StatsMod[] GetCurrentMods
        {
            get
            {
                var list = new List<StatsMod>();
                foreach (var equip in Equipments.GetAllValues())
                {
                    if (equip.StatMods != null)
                    {
                        list.AddRange(equip.StatMods);
                    }
                }
                return list.ToArray();
            }
        }
        public IReadOnlyList<Equipment> ListEquipped => Equipments.GetAllValues();
        public IReadOnlyList<Item> ListInventory => Inventory.AsReadOnly();

    }


}