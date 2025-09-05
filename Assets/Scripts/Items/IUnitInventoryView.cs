using System.Collections.Generic;
using UnityEngine.Events;

namespace Arcatech.Items
{
    public interface IUnitInventoryView
    {
        public event UnityAction<UnitInventoryViewReference> ViewChangedInventory;
        void RefreshView (UnitInventoryModel model);

    }

    public class UnitInventoryViewReference
    {
        public List<Equipment> Equips { get; }
        public List<Item> Inventory { get; }

    }
}