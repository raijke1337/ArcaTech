using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Interrupt state IS")]
    
    public class InterruptStateCondition : SerializedStateTransitionCondition
    {
        [SerializeField] public bool KnockDownStateIs = false;
        [SerializeField] public bool DeadStateIs = false;
        [SerializeField] public bool DamageStateIs = false;
        public override bool CanTransition(StateMachineContext ctx)
        {
            return (ctx.DeadState == DeadStateIs && ctx.KnockDownState == KnockDownStateIs && ctx.InterruptQueued == DamageStateIs);
        }
    }
}