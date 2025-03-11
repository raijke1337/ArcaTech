using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;
using Arcatech.EventBus;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System.Linq;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "New apply stat change result", menuName = "Actions/Action Result/Apply Stat Change", order = 3)]
    public class SerializedApplyStatChangeResult : SerializedActionResult
    {

        [SerializeField] SerializedDictionary<TargetingType, SerializedStatsEffectConfig[]> StatChanges;
        public override IActionResult BuildActionResult()
        {
            return new ApplyStatChangeEffectResult(StatChanges);
        }

        private void OnValidate()
        {
            Assert.IsNotNull(StatChanges);
            Assert.IsTrue(StatChanges.Count > 0);
            var firstKey = StatChanges.Keys.FirstOrDefault();
            Assert.IsNotNull(StatChanges[firstKey]);
            Assert.IsTrue(StatChanges[firstKey].Length>0);
        }

        public override string ToString()
        {
            return $"apply effects result total {StatChanges.Count}";
        }
    }
    public class ApplyStatChangeEffectResult : ActionResult
    {
        Dictionary <TargetingType, SerializedStatsEffectConfig[]> _effs; 
        public ApplyStatChangeEffectResult(SerializedDictionary<TargetingType, SerializedStatsEffectConfig[]> cfg)
        {
            _effs = cfg;

        }

        public override void ProduceResult(BaseEntity user, BaseEntity target,Transform place)
        {
            foreach (var type in _effs.Keys)
            {
                switch (type)
                {
                    case TargetingType.None:
                        foreach (var e in _effs[type])
                        {
                            Debug.LogWarning($"Target type not set for effect {e}");
                        }
                        break;
                    case TargetingType.OnlyUser:
                        foreach (var e in _effs[type])
                        {
                            EventBus<StatsEffectTriggerEvent>.Raise(new StatsEffectTriggerEvent(user, new StatsEffect(e), place));
                        }
                        break;
                    case TargetingType.AnyUnit:
                        foreach (var e in _effs[type])
                        {
                            EventBus<StatsEffectTriggerEvent>.Raise(new StatsEffectTriggerEvent(target, new StatsEffect(e), place));
                        }
                        break;
                    case TargetingType.AnyEnemy:
                        if (target.Side == user.Side) return;
                        foreach (var e in _effs[type])
                        {
                            EventBus<StatsEffectTriggerEvent>.Raise(new StatsEffectTriggerEvent(target, new StatsEffect(e), place));
                        }
                        break;
                    case TargetingType.AnyAlly:
                        if (target.Side != user.Side) return;
                        foreach (var e in _effs[type])
                        {
                            EventBus<StatsEffectTriggerEvent>.Raise(new StatsEffectTriggerEvent(target, new StatsEffect(e), place));
                        }
                        break;
                }
            }
        }
    }

}