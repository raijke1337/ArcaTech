using System.Collections.Generic;
using UnityEngine.Events;

namespace Arcatech.Items
{
    public interface IUnitInventoryView
    {
        public event UnityAction ViewChangedInventory;
        void RefreshView (UnitInventoryModel model);

    }

    public class UnitInventoryViewReference
    {
        public List<Equipment> Equips { get; set; }
        public List<Item> Inventory { get; set; }

    }
}