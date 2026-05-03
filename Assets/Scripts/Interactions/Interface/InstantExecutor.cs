using System;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
    public class InstantExecutor : InteractionExecutor
    {
        [SerializeField] private bool willComplete = true;
        public override void Execute(InteractionContext ctx, UnityAction<InteractionState> onComplete)
        {
            if (!willComplete) onComplete?.Invoke(InteractionState.Failure);
            onComplete?.Invoke(InteractionState.Success);
        }
    }
}