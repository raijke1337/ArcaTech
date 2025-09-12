using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "New stun", menuName = "Units/Base Stats/Handle/Stun stunnables")]
    public class StunStunnables : StatsUpdateStrategy
    {
        public override IOnStatsChangeStrategy BuildStrategy(ActiveGameUnitComponent unit)
        {
            throw new System.NotImplementedException();
        }
    }

    public class IStunnablesTrigger : StatsChangeHandle
    {
        public IStunnablesTrigger(ActiveGameUnitComponent component) : base(component)
        {
        }

        public override void HandleStats(IDictionary<BaseStatType, StatValueContainer> stats)
        {
            throw new System.NotImplementedException();
        }
    }
}
