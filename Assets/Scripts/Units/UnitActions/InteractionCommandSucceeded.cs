using Arcatech.Interactions;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Interaction attempt result")]
    public class InteractionCommandSucceeded : SerializedStateTransitionCondition
    {
        public bool resultIs = true;

        public override string ConditionName => "Interaction attempt result "+resultIs;

        public override bool CanTransition(StateMachineContext ctx)
        {
            if (ctx?.Owner == null) return false;
            var inter = ctx.Owner.GetComponent<IInteractor>();
            return inter.InteractionContext.WasUpdated && inter.InteractionContext.LastInteractionWasSuccessful ==  resultIs;
        }
    }
}