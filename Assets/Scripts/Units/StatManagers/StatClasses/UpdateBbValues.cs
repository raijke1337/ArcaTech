using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "New update bb", menuName = "Units/Base Stats/Handle/NPC/Update values in blackboard")]
    public class UpdateBbValues : StatsUpdateStrategy
    {
        public override IOnStatsChangeStrategy BuildStrategy(ActiveGameUnitComponent unit)
        {
            return new UpdateValuesHandle(unit);
        }
    }

    public class UpdateValuesHandle : IOnStatsChangeStrategy
    {
        ActiveGameUnitComponent unit;
        private BlackboardReference bb;

        public UpdateValuesHandle(ActiveGameUnitComponent unit)
        { 
            this.unit = unit;
            if (unit.TryGetComponent<BehaviorGraphAgent>(out BehaviorGraphAgent agent))
            {
                bb = agent.BlackboardReference;
            }
        }

        public void HandleStats(IDictionary<BaseStatType, StatValueContainer> stats)
        {
            if (bb != null)
            {
                var hp = stats[BaseStatType.Health];
                bb.SetVariableValue("hpPercent", hp.GetPercent);
            }
        }
    }
}