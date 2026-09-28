using Arcatech.Units;

namespace Arcatech.Items
{
    public interface IEquipmentPart : IEquippable
    {
        public void TriggerState (StateMachineNotifyType notification);
    }
}