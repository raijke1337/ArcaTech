using System.Collections.Generic;
using Arcatech.Actions;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/Actions/StateMachineNotify")]
    public class StateMachineNotificationActionSO : SerializedActionResult
    {
        [FormerlySerializedAs("NotifyType")] public StateMachineNotifyType notifyType;

        public override ActionResult BuildActionResult()
        {
            return new StateMachineNotification(notifyType);
        }
    }

    public class StateMachineNotification : ActionResult
    {
        public StateMachineNotification(StateMachineNotifyType notifyType)=>_type = notifyType;
        private readonly StateMachineNotifyType _type;
        private List<IStateMachineNotificationReceiver> _receivers;
        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Vector3 place, Quaternion placeRot)
        {
            if (_receivers == null)
            {
                _receivers = new List<IStateMachineNotificationReceiver>(user.GetComponentsInChildren<IStateMachineNotificationReceiver>());
            }

            foreach (var receiver in _receivers)
            {
                receiver.StateMachineNotification(_type);
            }
            return true;
        }
    }

    public interface IStateMachineNotificationReceiver
    {
        void StateMachineNotification(StateMachineNotifyType notifyType);
    }

    public enum StateMachineNotifyType
    {
        NoNotify,
        Starting,
        Use,
        EndUse,
        Cancel,
    }
}