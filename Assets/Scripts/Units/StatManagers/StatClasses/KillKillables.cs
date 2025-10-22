using Arcatech.Units;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "New kill", menuName = "Units/Base Stats/Handle/Kill Killables")]
    public class KillKillables : StatsUpdateStrategy
    {
        public override IOnStatsChangeStrategy BuildStrategy(EntityStatsComponent unit)
        {
            return new IKillablesTrigger(unit);
        }
    }

    public class IKillablesTrigger : StatsChangeHandle
    {
        List<IKillableComponent> components;
        public IKillablesTrigger(EntityStatsComponent component) : base(component)
        {
            components = component.GetComponentsInChildren<IKillableComponent>().ToList();
        }

        public override void HandleStats(IDictionary<BaseStatType, StatValueContainer> stats)
        {
            
            if (stats[BaseStatType.Health].Initialized && stats[BaseStatType.Health].GetCurrent <= 0)
            {
                foreach (var component in components) { component.Killed = true; }
            }
        }
    }
}
