using Arcatech.Stats;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class StatsInteractionCondition : InteractionCondition
    {
        
        [SerializeField] private ConditionGroup condtForTarget;
        public override bool Check(InteractionContext ctx)
        {
            if (!ctx.Target.TryGetComponent(out EntityStatsComponent stats)) return false;
            return stats.CheckStatsConditionGroup(condtForTarget);
        }
    }
}