using UnityEngine.Events;

namespace Arcatech.Items
{
    public interface IUnitInventoryView
    {
        public event UnityAction ViewChangedInventory;
        void RefreshView (InventoryChangeNotification notification);
    }

    public struct InventoryChangeNotification
    {
        public UnitInventoryModel InventorySnapshot;
        public Item ChangedItem;
        public int ChangedQuantity;
        public InventoryChangeType ChangeType;
    }

    public enum InventoryChangeType
    {
        Initialization,
        PickUp,
        Use,
        Equip,
        Unequip
    }
}