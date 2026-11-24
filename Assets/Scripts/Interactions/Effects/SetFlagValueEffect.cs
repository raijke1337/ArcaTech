// Interactions/Interface/SetFlagEffect.cs
using UnityEngine;

namespace Arcatech.Interactions
{
    public class SetFlagValueEffect : InteractionEffect
    {
        [SerializeField] private SaveSystem.ObjectiveFlagSO flag;
        [SerializeField] private InteractionState onState = InteractionState.Success;

        public override void Play(InteractionContext ctx)
        {
            if (ctx.State == onState) flag.Set();
        }
    }
}