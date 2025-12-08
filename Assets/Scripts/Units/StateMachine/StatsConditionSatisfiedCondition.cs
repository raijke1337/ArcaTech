using Arcatech.Stats;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/Stats")]
    public class StatsConditionSatisfiedCondition : SerializedStateTransitionCondition
    {
        public ConditionGroup conditionsToCheck;
        public override string ConditionName => ($"a group of {conditionsToCheck.statConditions.Count} stat conditions");
        public override bool CanTransition(StateMachineContext ctx)
        {
            if (!ctx.Stats)
            {
                Debug.Log("Tried to validate stats without stats component"); return false;
            }
            
            return ctx.Stats.CheckStatsConditionGroup(conditionsToCheck);
        }
    }
}