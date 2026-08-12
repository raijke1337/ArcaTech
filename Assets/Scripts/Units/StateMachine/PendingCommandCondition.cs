using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Pending unit command")]
    public class PendingCommandCondition : SerializedStateTransitionCondition
    {
        public UnitActionType requiredCommand;

        public override bool CanTransition(StateMachineContext ctx)
        {
            return ctx.PendingCommand.Type == requiredCommand;
        }
    }
}

