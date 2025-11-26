using UnityEngine;

namespace Arcatech.Units
{
    public abstract class SerializedStateTransitionCondition : ScriptableObject
    {
        // Return true when this condition allows the transition
        public abstract string ConditionName { get; }
        public abstract bool CanTransition(StateMachineContext ctx);
    }
}