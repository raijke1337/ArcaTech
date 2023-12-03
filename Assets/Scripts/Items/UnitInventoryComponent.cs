using Arcatech.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Arcatech.Units
{
    [Serializable]
    public class UnitInventoryComponent
    {
        /// <summary>
        /// item, isadded
        /// </summary>
        public event SimpleEventsHandler<InventoryItem,bool> EquipmentChangedEvent;
        public event SimpleEventsHandler<InventoryItem,bool> InventoryChangedEvent;

        private Dictionary<EquipItemType, EquipmentItem> _equips;//TODO
        private List<InventoryItem> _items;
        private List<InventoryItem> _drops;

        private BaseUnit _owner;


        public ItemsStringsSave PackSaveData // used by player unit
        { get
            {

                ItemsStringsSave cont =  new ItemsStringsSave();
                foreach (var e in _equips.Values)
                {
                    cont.Equips.Add(e.ID);
                }
                foreach (var e in _items)
                {
                    cont.Inventory.Add(e.ID);
                }
                foreach (var d in _drops)
                {
                    // NYI player should not drop anything
                }

                return cont;
            }
        }

        public IReadOnlyCollection<EquipmentItem> GetCurrentEquips => _equips.Values;
        public IReadOnlyCollection<InventoryItem> GetCurrentInventory => _items;




        public void EquipItem(EquipmentItem e)
        {
            _equips[e.ItemType] = e;
            e.Owner = _owner;
            e.OnEquip();
            EquipmentChangedEvent?.Invoke(e,true);
        }
        public void AddItem(InventoryItem i)
        {
            _items.Add(i);
            i.Owner = _owner;
            InventoryChangedEvent?.Invoke(i,true);
        }
        public InventoryItem RemoveItem(string ID)
        {
            InventoryItem ret = null;
            try
            {
                var i = _items.First(t => t.ID == ID);
                _items.Remove(i);
                ret = i;
                InventoryChangedEvent?.Invoke(ret, false);

                return ret;
            }
            catch
            {
                return ret;
            }
        }

        public bool RemoveItem(InventoryItem item)
        {
            bool ret = false;
            if (_items.Contains(item))
            {
                _items.Remove(item);
                ret = true;
            }
            return ret;
        }

        public EquipmentItem UnequipItem(string ID)
        {
            EquipmentItem ret = null;
            try
            {
                var i = _equips.Values.First(t => t.ID == ID);
                _items.Remove(i);
                ret = i;

                ret.OnUnequip();

                InventoryChangedEvent?.Invoke(ret, false);
                return ret;
            }
            catch
            {
                return ret;
            }
        }
        public EquipmentItem UnequipItem(EquipItemType type)
        {
            EquipmentItem ret = null;
            try
            {
                var i = _equips[type];
                _equips.Remove(type);



                ret = i;
                ret.OnUnequip();
                InventoryChangedEvent?.Invoke(ret, false);

                return ret;
            }
            catch
            {
                return ret;
            }
        }

        // for default inventory setup from config SOs
        public UnitInventoryComponent (UnitInventoryItemConfigsContainer cfg, BaseUnit owner)
        {
            _owner = owner;

            _equips = new Dictionary<EquipItemType, EquipmentItem>();
            _items = new List<InventoryItem>();
            _drops = new List<InventoryItem>();

            foreach (var e in cfg.Equipment)
            {
                _equips[e.ItemType] = new EquipmentItem(e, _owner);
            }
            foreach (var i in cfg.Inventory)
            {
                if (i is Equip e)
                {
                    _items.Add(new EquipmentItem(e, _owner));
                }
                else _items.Add(new InventoryItem(i, _owner));
            }
            foreach (var i in cfg.Drops)
            {
                if (i is Equip e)
                {
                    _drops.Add(new EquipmentItem(e, _owner));
                }
                else _drops.Add(new InventoryItem(i, _owner));
            }

            

        } 
        // for player laod from save
        public UnitInventoryComponent(ItemsStringsSave strings, BaseUnit owner)
        {
            _owner = owner;

            _equips = new Dictionary<EquipItemType, EquipmentItem>();
            _items = new List<InventoryItem>();
            _drops = new List<InventoryItem>();

            foreach (var e in strings.Equips)
            {
                var item = DataManager.Instance.GetConfigByID<Equip>(e);
                _equips[item.ItemType] = new EquipmentItem(item, _owner);
            }

            foreach (var i in strings.Inventory)
            {
                var item = DataManager.Instance.GetConfigByID<Item>(i);
                if (item is Equip e)                {
                    
                    _items.Add(new EquipmentItem(e, _owner));
                }

                else
                {
                    _items.Add(new InventoryItem(item, _owner));
                }
            }


        }


    }
}