using Arcatech.EventBus;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "New draw damage", menuName = "Units/Base Stats/Handle/Draw damages")]
    public class DrawDamage : StatsUpdateStrategy
    {
        [SerializeField, Range(0, 99999)] int minValueToDraw = 20;
        public override IOnStatsChangeStrategy BuildStrategy(ActiveGameUnitComponent unit)
        {
            return new DrawDamageTrigger(minValueToDraw, unit);
        }
    }
    public class DrawDamageTrigger : StatsChangeHandle
    {
        int triggerTreschold;
        public DrawDamageTrigger(int drawAt, ActiveGameUnitComponent component) : base(component)
        {
            triggerTreschold = drawAt;
        }

        public override void HandleStats(IDictionary<BaseStatType, StatValueContainer> stats)
        {
            var hp = stats[BaseStatType.Health];
            if (hp != null && hp.Initialized)
            {

                //  Debug.Log($"HP delta {hp.GetFrameDeltaValue}");
                if (Mathf.Abs(hp.GetFrameDeltaValue) > triggerTreschold)
                {
                    EventBus<DrawDamageEvent>.Raise(new DrawDamageEvent(unit.GetMainEntity, hp.GetFrameDeltaValue));
                    //ForceUnitAction(_damageAction);
                }
            }
        }
    }

}
