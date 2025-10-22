using System.Collections.Generic;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "New damage", menuName = "Units/Base Stats/Handle/Damage damageables")]
    public class DamageDamageables : StatsUpdateStrategy
    {
        public override IOnStatsChangeStrategy BuildStrategy(EntityStatsComponent unit)
        {
            return new IDamageableTrigger(unit);
        }
    }

    public class IDamageableTrigger : StatsChangeHandle
    {
        
        List<IDamageableComponent> _damageables;
        public IDamageableTrigger(EntityStatsComponent component) : base(component)
        {
            _damageables =  new List<IDamageableComponent>(component.GetComponentsInChildren<IDamageableComponent>());
        }

        public override void HandleStats(IDictionary<BaseStatType, StatValueContainer> stats)
        {
            foreach (var stat in stats)
            {
                var d = stat.Value.GetFrameDeltaValue;
                if (d < 0) // damage
                {
                    foreach (var damageable in _damageables)
                    {
                        damageable.Damage(d,stat.Key);
                    }
                }
            }
        }
    }
}
