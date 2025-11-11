using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Pending unit command")]
    public class PendingCommandCondition : SerializedStateTransitionCondition
    {
        public UnitActionType requiredCommand;
        public override bool CanTransition(StateMachineContext ctx)
        {
           // Debug.Log($"check pending: need {requiredCommand}, context {ctx.PendingCommand}");
            return ctx.PendingCommand == requiredCommand;
        }
    }
}

