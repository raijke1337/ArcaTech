using System;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Command is succesfully consumed")]
    public class CommandConsumedCondition : SerializedStateTransitionCondition
    {
        public override bool CanTransition(StateMachineContext ctx)
        {
            return ctx.PendingCommand == UnitActionType.None;
        }
    }
}