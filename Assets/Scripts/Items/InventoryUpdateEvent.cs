using Arcatech.EventBus;
using Arcatech.Units;

namespace Arcatech.Items
{
    public struct InventoryUpdateEvent : IEvent
    {
        public InventoryUpdateEvent(EquippedUnitOLD unit, UnitInventoryControllerOLD inventory)
        {
            Unit = unit;
            Inventory = inventory;
        }

        public EquippedUnitOLD Unit { get; }
        public UnitInventoryControllerOLD Inventory { get; }
    }
}