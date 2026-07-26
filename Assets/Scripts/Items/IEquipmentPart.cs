using Arcatech.Units;

namespace Arcatech.Items
{
    public interface IEquipmentPart
    {
        public void TriggerState (StateMachineNotifyType notification);
    }
}