using Arcatech.EventBus;
using Arcatech.Stat;
using Arcatech.Triggers;
using System.Collections.Generic;
using UnityEngine;
namespace Arcatech.Managers
{
    public class TriggersManager : MonoBehaviour, IManagedController
    {
        EventBinding<StatsEffectTriggerEvent> _triggersBinding;

        private void OnEnable()
        {
            if (_triggersBinding == null) _triggersBinding = new EventBinding<StatsEffectTriggerEvent>(HandleStatsEffectEvent);
            EventBus<StatsEffectTriggerEvent>.Register(_triggersBinding);
        }
        private void OnDisable()
        {
            EventBus<StatsEffectTriggerEvent>.Deregister(_triggersBinding);
        }


        #region LoadedManager
        public virtual void StartController()
        {

            Debug.Log($"starting triggers on {gameObject}");

            _alreadyAppliedTO = new Dictionary<StatsEffect, List<EntityStatsComponent>>();

        }
        public virtual void ControllerUpdate(float delta)
        {

        }
        public virtual void FixedControllerUpdate(float fixedDelta)
        {

        }

        public virtual void StopController()
        {
            _alreadyAppliedTO.Clear();

        }
        #endregion

        #region triggers

        private Dictionary<StatsEffect, List<EntityStatsComponent>> _alreadyAppliedTO;
        private void HandleStatsEffectEvent(StatsEffectTriggerEvent obj)
        {

            if (obj.Target.TryGetComponent<EntityStatsComponent>(out var stats)) // check if the hit entity has some stats that can be changed
            {
                if (_alreadyAppliedTO.TryGetValue(obj.Applied, out var listOfAffectedEntities))
                {
                    // effect in list

                    if (listOfAffectedEntities.Contains(stats)) return; // already applied to to target
                    else
                    {
                        // target not in list
                        stats.ApplyStatsEffect(obj.Applied);
                        listOfAffectedEntities.Add(stats);

                        if (obj.Applied.OnApply != null)
                        {
                            obj.Applied.OnApply.BuildActionResult().ProduceResult(null, obj.Target, obj.Place); // play particles or maybe something else if needed
                        }
                    }
                }
                // effect not in list just do normally and create a new entry
                else
                {
                    stats.ApplyStatsEffect(obj.Applied);
                    _alreadyAppliedTO[obj.Applied] = new List<EntityStatsComponent>() { stats };

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

