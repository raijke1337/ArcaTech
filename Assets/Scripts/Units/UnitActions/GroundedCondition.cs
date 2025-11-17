using Arcatech.Units.Control;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Unit grounded")]
    public class GroundedCondition : SerializedStateTransitionCondition
    {
        public bool requireGrounded = true;

        public override string ConditionName => "Grounded" +  requireGrounded;

        public override bool CanTransition(StateMachineContext ctx)
        {
            if (ctx?.Owner == null) return false;
            var mover = ctx.Owner.GetComponent<IMove>();
            if (mover == null) return false;
            return mover.IsGrounded == requireGrounded;
        }
    }
}