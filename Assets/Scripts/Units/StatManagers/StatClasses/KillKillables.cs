using Arcatech.Units;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "New kill", menuName = "Units/Base Stats/Handle/Kill Killables")]
    public class KillKillables : StatsUpdateStrategy
    {
        public override IOnStatsChangeStrategy BuildStrategy(ActiveGameUnitComponent unit)
        {
            return new IKillablesTrigger(unit);
        }
    }

    public class IKillablesTrigger : StatsChangeHandle
    {
        List<IKillableComponent> components;
        public IKillablesTrigger(ActiveGameUnitComponent component) : base(component)
        {
            components = component.GetComponentsInChildren<IKillableComponent>().ToList();
        }

        public override void HandleStats(IDictionary<BaseStatType, StatValueContainer> stats)
        {
            if (stats[BaseStatType.Health].Initialized && stats[BaseStatType.Health].GetCurrent == 0)
            {
                foreach (var component in components) { component.Kill(); }
            }
        }
    }
}
