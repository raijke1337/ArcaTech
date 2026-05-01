using System;
using UnityEngine;

namespace Arcatech.Interactions
{
    /// <summary>
    ///the execution part of interaction
    /// </summary>
    public abstract class InteractionExecutor : MonoBehaviour
    {
        public abstract void Execute(InteractionContext ctx, Action<InteractionState> onComplete);
        public virtual void Cancel(InteractionContext ctx) { }
        public virtual bool CanCancel => false;
    }
}