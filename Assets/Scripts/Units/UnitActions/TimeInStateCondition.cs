using Arcatech.Actions;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/TimeInState")]
    public class TimeInStateCondition : SerializedStateTransitionCondition
    {
        public float minTimeInState = 0.0f;

        public override bool CanTransition(StateMachineContext ctx)
        {
            if (ctx?.CurrentState == null) return false;
            return ctx.CurrentState.TimeInState >= minTimeInState;
        }
    }
}
