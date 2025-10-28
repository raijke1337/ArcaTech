using Arcatech.EventBus;
using System.Collections.Generic;
using Arcatech.Managers;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "New draw damage", menuName = "Units/Base Stats/Handle/Draw damages")]
    public class DrawDamage : StatsUpdateStrategy
    {
        [SerializeField, Range(0, 99999)] int minValueToDraw = 20;
        [SerializeField] private DamageTextContainer prefab;
        [SerializeField, Range(1,10)] private int maxInstances = 3;
        
        public override IOnStatsChangeStrategy BuildStrategy(EntityStatsComponent unit)
        {
            return new DrawDamageTrigger(prefab, minValueToDraw, maxInstances, unit);
        }
    }
    public class DrawDamageTrigger : StatsChangeHandle
    {
        readonly int triggerTreschold;
        private DamageTextContainer[] prefabs;
        private int index = 0;
        
        private DamageTextContainer prefab;
        
        public DrawDamageTrigger(DamageTextContainer p, int instances, int drawAt, EntityStatsComponent component) : base(component)
        {
            this.prefab = p; 
            triggerTreschold = drawAt;
            prefabs = new DamageTextContainer[instances];
            for (int i = 0; i < prefabs.Length; i++)
            {
                prefabs[i] = Object.Instantiate(prefab, component.transform);
            }
        }

        public override void HandleStats(IDictionary<BaseStatType, StatValueContainer> stats)
        {
            float value = Mathf.Abs(stats[BaseStatType.Health].GetFrameDeltaValue);
            if (value > triggerTreschold)
            {
                // EventBus<DrawDamageEvent>.Raise(new DrawDamageEvent(unit, hp.GetFrameDeltaValue));
                //ForceUnitAction(_damageAction);
                if (index == prefabs.Length)
                {
                    index = 0;
                }
                prefabs[index].PlayNumbers(value);
            }

        }
    }
}
