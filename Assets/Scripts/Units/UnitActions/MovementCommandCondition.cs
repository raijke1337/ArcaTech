using Arcatech.Units.Control;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Movement Input")]
    public class MovementCommandCondition : SerializedStateTransitionCondition
    {
        public float threshold = 0.1f;
        public bool requireMoving = true;

        public override bool CanTransition(StateMachineContext ctx)
        {
            var mover = ctx.Owner.GetComponentInChildren<UnitInputsComponent>();
            bool moving = mover.InputMovement.sqrMagnitude > threshold * threshold;
            return requireMoving ? moving : !moving;
        }
    }
}