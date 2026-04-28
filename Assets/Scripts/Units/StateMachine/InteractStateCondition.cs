using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Entity Interaction State Is")]
    public class InteractStateCondition : SerializedStateTransitionCondition
    {
        public bool RequiredState = false;
        public override bool CanTransition(StateMachineContext ctx)
        {
            return ctx.InInteraction ==  RequiredState;
        }
    }
}