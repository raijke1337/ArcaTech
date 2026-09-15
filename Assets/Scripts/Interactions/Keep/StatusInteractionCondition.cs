using UnityEngine;

namespace Arcatech.Interactions
{
    public class StatusInteractionCondition : InteractionCondition
    {
        [SerializeField] private bool entityIsStunned = true;


        public override bool Check(InteractionContext ctx)
        {
            return ctx.Target.Stunned ==  entityIsStunned;
        }
    }
}