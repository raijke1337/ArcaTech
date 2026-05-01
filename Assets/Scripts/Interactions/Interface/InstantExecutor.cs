using System;

namespace Arcatech.Interactions
{
    public class InstantExecutor : InteractionExecutor
    {
        public override void Execute(InteractionContext ctx, Action<InteractionState> onComplete)
        {
            onComplete?.Invoke(InteractionState.Success);
        }
    }
}