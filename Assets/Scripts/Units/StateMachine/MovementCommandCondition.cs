using Arcatech.Units.Control;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Movement Input")]
    public class MovementCommandCondition : SerializedStateTransitionCondition
    {
        public float velocityThreshold = 0.1f;
        public bool requireMoving = true;


        public override bool CanTransition(StateMachineContext ctx)
        {
            var mover = ctx.Owner.GetComponentInChildren<IMove>();
            if (mover == null) return false;
            bool moving = mover.ActualMovementVelocity > velocityThreshold;
           // if (ctx.Owner.ShowingDebugs) Debug.Log($"Check {ctx.Owner.GetName} moving: result is {moving}");
            if (requireMoving) return moving;
            return !moving;
        }
    }
}