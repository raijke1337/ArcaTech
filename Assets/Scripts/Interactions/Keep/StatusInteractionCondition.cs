using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
    public class StatusInteractionCondition : InteractionCondition
    {
        [SerializeField] private bool entityIsStunned = true;

        bool Check(InteractionContext ctx)
        {
            return ctx.Target.Stunned ==  entityIsStunned;
        }
        public override void Check(InteractionContext ctx, UnityAction<InteractionState> callback)
        {
            callback.Invoke(Check(ctx) ? InteractionState.Success : InteractionState.Failure);
        }

    }
}