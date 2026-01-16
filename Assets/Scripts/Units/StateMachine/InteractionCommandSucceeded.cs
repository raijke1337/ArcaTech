using Arcatech.Interactions;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Interaction attempt result")]
    public class InteractionCommandSucceeded : SerializedStateTransitionCondition
    {
        public bool resultIs = true;


        public override bool CanTransition(StateMachineContext ctx)
        {
            if (ctx.Interactor == null) return false;
            
            bool hasR = ctx.Interactor.InteractionContext.HasInteractionResult(out var result);
            //Debug.Log($"Peek result: available {hasR}, result: {result}");
            if (!hasR)  return false;
            return result == resultIs;
        }
    }
}