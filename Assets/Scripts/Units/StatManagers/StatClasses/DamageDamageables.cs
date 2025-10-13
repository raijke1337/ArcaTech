using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "New damage", menuName = "Units/Base Stats/Handle/Damage damageables")]
    public class DamageDamageables : StatsUpdateStrategy
    {
        public override IOnStatsChangeStrategy BuildStrategy(ActiveGameUnitComponent unit)
        {
            return new IDamageableTrigger(unit);
        }
    }

    public class IDamageableTrigger : StatsChangeHandle
    {
        public IDamageableTrigger(ActiveGameUnitComponent component) : base(component)
        {
        }

        public override void HandleStats(IDictionary<BaseStatType, StatValueContainer> stats)
        {
            throw new System.NotImplementedException();
        }
    }
}
