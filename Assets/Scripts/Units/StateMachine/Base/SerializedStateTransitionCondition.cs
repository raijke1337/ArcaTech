using UnityEngine;

namespace Arcatech.Units
{
    public abstract class SerializedStateTransitionCondition : ScriptableObject
    {
        public abstract bool CanTransition(StateMachineContext ctx);
    }
    
    
}