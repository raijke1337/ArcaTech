using UnityEngine.Events;

namespace Arcatech.Items
{
    public interface IUnitInventoryView
    {
        /// <summary>Вьюха просит изменить инвентарь (меню, UI): тип изменения, предмет, количество.
        /// Обрабатывает EntityInventoryComponent. Рендереры (как drawer) событие не вызывают.</summary>
        public event UnityAction<InventoryChangeType, Item, int> ViewChangedInventory;
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