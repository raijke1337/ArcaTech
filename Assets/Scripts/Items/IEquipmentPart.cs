using Arcatech.Units;

namespace Arcatech.Items
{
    public interface IEquipmentPart : IEquippable
    {
        /// <summary>
        /// called vby state machine on state 
        /// </summary>
        /// <param name="notification"></param>
        public void TriggerState (StateMachineNotifyType notification);
    }
}