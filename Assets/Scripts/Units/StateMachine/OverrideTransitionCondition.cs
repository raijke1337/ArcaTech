using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Interrupt state IS")]
    
    public class OverrideTransitionCondition : SerializedStateTransitionCondition
    {
        [SerializeField] public bool StunnedStateIs = false;
        [SerializeField] public bool DeadStateIs = false;
        [SerializeField] public bool InterruptPendingIs = false;
        [SerializeField] public bool OverChargeStateIs = false;
        public override bool CanTransition(StateMachineContext ctx)
        {
            return (ctx.DeadState == DeadStateIs && ctx.StunnedState == StunnedStateIs && ctx.InterruptQueued == InterruptPendingIs
                && ctx.OverchargeTriggerPending == OverChargeStateIs);
        }
    }
}