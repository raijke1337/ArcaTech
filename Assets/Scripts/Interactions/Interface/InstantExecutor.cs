using System;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
    public class InstantExecutor : InteractionExecutor
    {
        public override void Execute(InteractionContext ctx, UnityAction<InteractionState> onComplete)
        {
            onComplete?.Invoke(InteractionState.Success);
        }
    }
}