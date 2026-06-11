using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Time elapsed in current state")]
    public class TimeInCurrentStateElapsedCondition : SerializedStateTransitionCondition
    {
        [SerializeField] private float requiredTimeInStateSeconds = 1f;
        public override bool CanTransition(StateMachineContext ctx)
        {
            return ctx.CurrentState.TimeInState>=requiredTimeInStateSeconds;
        }
    }
}