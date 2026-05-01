using System;

namespace Arcatech.Interactions
{
    public class InstantExecutor : InteractionExecutor
    {
        public override void Execute(InteractionContext ctx, Action<InteractionStatus> onComplete)
        {
            onComplete?.Invoke(InteractionStatus.Success);
        }
    }
}