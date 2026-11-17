using Arcatech.Units.Control;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/VelocityInput")]
    public class CurrentMovementInputCondition : SerializedStateTransitionCondition
    {
        public float valueTreshold = 0.1f;
        public bool requiredInput = true;


        public override bool CanTransition(StateMachineContext ctx)
        {            
            var mover = ctx?.PrimaryMover;
            if (mover == null) return false;
            var input = mover.MovementVector;
            return requiredInput? input.sqrMagnitude > valueTreshold : input.sqrMagnitude < valueTreshold;
        }
    }
}
