using System;
using Arcatech.Interactions;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Entity Interaction State Is")]
    public class InteractStateCondition : SerializedStateTransitionCondition
    {
        public InteractionState requiredState = InteractionState.InProgress;
        [Header ("'OR' condition, overwrites the one above")]
        public InteractionState[] requiredStates = Array.Empty<InteractionState>();
        public override bool CanTransition(StateMachineContext ctx)
        {
            if (ctx.Interactor == null) return false;

            var current = ctx.Interactor.State;

            if (requiredStates != null && requiredStates.Length > 0)
            {
                foreach (var s in requiredStates)
                    if (s == current) return true;
                return false;
            }

            return current == requiredState;
        }
    }
}