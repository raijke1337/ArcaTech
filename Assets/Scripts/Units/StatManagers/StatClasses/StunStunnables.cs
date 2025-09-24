using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "New stun", menuName = "Units/Base Stats/Handle/Stun stunnables")]
    public class StunStunnables : StatsUpdateStrategy
    {
        public override IOnStatsChangeStrategy BuildStrategy(ActiveGameUnitComponent unit)
        {
            return new IStunnablesTrigger(unit);
        }
    }

    public class IStunnablesTrigger : StatsChangeHandle
    {
        public IStunnablesTrigger(ActiveGameUnitComponent component) : base(component)
        {
        }

        public override void HandleStats(IDictionary<BaseStatType, StatValueContainer> stats)
        {
            if (stats.TryGetValue(BaseStatType.Stamina, out var st) && st.GetCurrent <= 0)
            {
                Debug.Log("Stun");
            }
        }
    }
}
