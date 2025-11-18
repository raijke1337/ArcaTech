using Arcatech.Units.Control;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Movement Input")]
    public class MovementCommandCondition : SerializedStateTransitionCondition
    {
        public float velocityThreshold = 0.5f;
        public bool requireMoving = true;

        public override string ConditionName => $"Movement is {true}";

        public override bool CanTransition(StateMachineContext ctx)
        {
            var mover = ctx.Owner.GetComponentInChildren<IMove>();
            bool moving = mover.ActualMovementVelocity > velocityThreshold;
            return requireMoving ? moving : !moving;
        }
    }
}