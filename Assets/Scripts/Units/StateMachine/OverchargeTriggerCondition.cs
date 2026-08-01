using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Overcharge")]
    public class OverchargeTriggerCondition : SerializedStateTransitionCondition
    {
        [SerializeField] public bool OverChargeStateIs = false;

        public override bool CanTransition(StateMachineContext ctx)
        {
            throw new System.NotImplementedException();
        }
    }
}