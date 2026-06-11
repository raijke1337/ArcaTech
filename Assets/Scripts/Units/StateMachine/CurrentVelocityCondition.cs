using Arcatech.Units.Control;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Velocity")]
    public class CurrentVelocityCondition : SerializedStateTransitionCondition
    {
        public float velocityThreshold = 0.1f;
        public bool thresholdSatisfied = true;


        public override bool CanTransition(StateMachineContext ctx)
        {
            var mover = ctx.Owner.GetComponentInChildren<IMove>();
            if (mover == null) return false;
            var velocity = mover.ActualMovementVelocity;

            return thresholdSatisfied ? velocity > velocityThreshold : velocity < velocityThreshold;
        }
    }
}