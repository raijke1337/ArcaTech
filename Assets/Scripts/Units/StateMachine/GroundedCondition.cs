using Arcatech.Units.Control;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Unit grounded")]
    public class GroundedCondition : SerializedStateTransitionCondition
    {
        public bool requireGrounded = true;


        public override bool CanTransition(StateMachineContext ctx)
        {
            var mover = ctx?.RootMover;
            if (mover == null) return false;
            return mover.IsGrounded == requireGrounded;
        }
    }
}
