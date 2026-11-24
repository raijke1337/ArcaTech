// Interactions/Interface/FlagsSetCondition.cs
using System.Collections.Generic;
using Arcatech.SaveSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
    public class FlagsSetCondition : InteractionCondition
    {
        [SerializeField] private List<ObjectiveFlagSO> flags;

        public override void Check(InteractionContext ctx, UnityAction<InteractionState> callback)
        {
            bool allSet = true;
            foreach (var f in flags)
                if (f == null || !f.IsSet) { allSet = false; break; }

            callback.Invoke(allSet ? InteractionState.Success : InteractionState.Failure);
        }
    }
}