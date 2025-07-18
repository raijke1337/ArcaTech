using Arcatech.Level.Conditions;
using AYellowpaper.SerializedCollections;
using KBCore.Refs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Level
{
    [CreateAssetMenu(fileName = "new Light changes color behavior", menuName = "Level/Event Condition Behavior/Light changes color")]
    public class ToggledLightColorBehavior : ConditionBehaviorStrategy
    {
        [SerializeField] SerializedDictionary<ConditionCheckResult, Color> _colors;
        public override IConditionControlledStrat Build(ConditionControlledItemComponent item)
        {
            return new ToggledLightStrat(item,_colors);
        }
        private void OnValidate()
        {
            Assert.IsNotNull( _colors);
        }
    }
    public class ToggledLightStrat : IConditionControlledStrat
    {
        List<Light> lights;
        Dictionary<ConditionCheckResult, Color> colors;
        public ToggledLightStrat (ConditionControlledItemComponent item, SerializedDictionary<ConditionCheckResult, Color> c)
        {
            lights =  new List<Light>();
            colors = new Dictionary<ConditionCheckResult, Color>(c);
        }
        public void SetState(ConditionCheckResult newstate)
        {
            foreach (Light light in lights)
            {
                light.color = colors[newstate];
            }
        }

    }

}