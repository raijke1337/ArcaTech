
using Arcatech.EventBus;
using Arcatech.Stats;
using Arcatech.Triggers;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Arcatech.Managers
{
    public class TriggersManager : MonoBehaviour
    {
        EventBinding<StatsEffectTriggerEvent> _triggersBinding;

        private void OnEnable()
        {
            _alreadyAppliedTO = new Dictionary<StatsEffect, List<BaseGameEntityComponent>>();
            if (_triggersBinding == null) _triggersBinding = new EventBinding<StatsEffectTriggerEvent>(HandleStatsEffectEvent);
            EventBus<StatsEffectTriggerEvent>.Register(_triggersBinding);
        }
        private void OnDisable()
        {
            EventBus<StatsEffectTriggerEvent>.Deregister(_triggersBinding);
            _alreadyAppliedTO.Clear();
        }


        #region triggers

        private Dictionary<StatsEffect, List<BaseGameEntityComponent>> _alreadyAppliedTO;
        private void HandleStatsEffectEvent(StatsEffectTriggerEvent obj)
        {
            
            var target = obj.Target;

            //if (obj.Target.TryGetComponent<EntityStatsComponent>(out var stats)) // check if the hit entity has some stats that can be changed
            {
                if (_alreadyAppliedTO.TryGetValue(obj.Applied, out var listOfAffectedEntities))
                {
                    // effect in list

                    if (listOfAffectedEntities.Contains(target)) return; // already applied  to target
                    else
                    {
                        // target not in list
                        target.ApplyStatsEffect(obj.Applied,obj.Source);
                        listOfAffectedEntities.Add(target);

                        if (obj.Applied.OnApply != null)
                        {
                            obj.Applied.OnApply.BuildActionResult().ProduceResult(null, obj.Target, obj.Place); // play particles or maybe something else if needed
                        }
                    }
                }
                // effect not in list just do normally and create a new entry
                else
                {
                    target.ApplyStatsEffect(obj.Applied,obj.Source);
                    _alreadyAppliedTO[obj.Applied] = new List<BaseGameEntityComponent> { target };

                    if (obj.Applied.OnApply != null)
                    {
                        obj.Applied.OnApply.BuildActionResult().ProduceResult(null, obj.Target, obj.Place);
                    }
                }
            }
        }
    }
    #endregion
}

