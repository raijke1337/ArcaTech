using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "New update bb", menuName = "Units/Base Stats/Handle/NPC/Update values in blackboard")]
    public class UpdateBbValues : StatsUpdateStrategy
    {
        public override IOnStatsChangeStrategy BuildStrategy(EntityStatsComponent unit)
        {
            return new UpdateValuesHandle(unit);
        }
    }

    public class UpdateValuesHandle : IOnStatsChangeStrategy
    {
        EntityStatsComponent unit;
        private BlackboardReference bb;

        public UpdateValuesHandle(EntityStatsComponent unit)
        { 
            this.unit = unit;
            if (unit.TryGetComponent<BehaviorGraphAgent>(out BehaviorGraphAgent agent))
            {
                bb = agent.BlackboardReference;
            }
        }

        public void HandleStats(IDictionary<BaseStatType, StatValueContainer> stats)
        {
            if (bb == null)
            {
                throw new NullReferenceException($"no blackboard reference in {unit}");
            }
            var hp = stats[BaseStatType.Health];
            if (!hp.Initialized) return;
            bb.SetVariableValue("hpPercent", hp.GetPercent);
        }
    }
}