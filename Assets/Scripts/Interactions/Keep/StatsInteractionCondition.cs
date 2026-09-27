using Arcatech.Stats;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
    public class StatsInteractionCondition : InteractionCondition
    {
        
        [SerializeField] private ConditionGroup condtForTarget;
        bool Check(InteractionContext ctx)
        {
            if (!ctx.Target.TryGetComponent(out EntityStatsComponent stats)) return false;
            return stats.CheckStatsConditionGroup(condtForTarget);
        }
        public override void Check(InteractionContext ctx, UnityAction<InteractionState> callback)
        {
            callback.Invoke(Check(ctx) ? InteractionState.Success : InteractionState.Failure);
        }
    }
}