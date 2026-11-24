using Arcatech.SaveSystem;
using UnityEngine;

namespace Arcatech.Interactions
{
    public sealed class CheckpointActivationEffect : InteractionEffect
    {

        [SerializeField] private CheckpointTrigger checkpoint;

        public override void Play(InteractionContext ctx)
        {
            if (ctx.State == InteractionState.Success) checkpoint.Reach();
        }
    }
}