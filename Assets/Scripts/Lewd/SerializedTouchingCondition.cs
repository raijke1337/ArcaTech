using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Lewding
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/H/Touching command")]
    public class TouchingCondition : SerializedStateTransitionCondition
    {
        public TouchZoneType TypeOfTouch;
        public override string ConditionName => $"Touched {TypeOfTouch}";

        public override bool CanTransition(StateMachineContext ctx)
        {
            return ctx.EcchiContext?.LastTouchCommand == TypeOfTouch;
        }
    }
}