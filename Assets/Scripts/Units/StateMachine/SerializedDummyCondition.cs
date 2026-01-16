using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Dummy")]
    public class SerializedDummyCondition : SerializedStateTransitionCondition
    {
        [SerializeField] private bool result;
        public override bool CanTransition(StateMachineContext ctx) => result;
    }
}