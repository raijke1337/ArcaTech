using UnityEngine.Events;

namespace Arcatech.Items
{
    public interface IUnitInventoryView
    {
        public event UnityAction ViewChangedInventory;
        void RefreshView (UnitInventoryModel model);
    }
}