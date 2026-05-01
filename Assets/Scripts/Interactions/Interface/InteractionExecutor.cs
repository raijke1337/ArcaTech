using System;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
    /// <summary>
    ///the execution part of interaction
    /// </summary>
    public abstract class InteractionExecutor : MonoBehaviour
    {
        public abstract void Execute(InteractionContext ctx, UnityAction<InteractionState> onComplete);
        public virtual void Cancel(InteractionContext ctx) { }
        public virtual bool CanCancel => false;
    }
}