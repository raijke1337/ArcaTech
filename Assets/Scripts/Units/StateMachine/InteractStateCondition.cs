using Arcatech.Interactions;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Entity Interaction State Is")]
    public class InteractStateCondition : SerializedStateTransitionCondition
    {
        public InteractionState requiredState = InteractionState.InProgress;
        public override bool CanTransition(StateMachineContext ctx)
        {
            return ctx.Interactor != null && ctx.Interactor.State ==  requiredState;
        }
    }
}