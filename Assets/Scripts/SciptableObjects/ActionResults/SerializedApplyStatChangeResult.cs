﻿using Arcatech.Triggers;
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

        [SerializeField] SerializedDictionary<TriggerTargetType, SerializedStatsEffectConfig[]> StatChanges;
        public override IActionResult GetActionResult()
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
        Dictionary <TriggerTargetType, SerializedStatsEffectConfig[]> _effs; 
        public ApplyStatChangeEffectResult(SerializedDictionary<TriggerTargetType, SerializedStatsEffectConfig[]> cfg)
        {
            _effs = cfg;

        }

        public override void ProduceResult(BaseEntity user, BaseEntity target,Transform place)
        {
            foreach (var type in _effs.Keys)
            {
                switch (type)
                {
                    case TriggerTargetType.None:
                        foreach (var e in _effs[type])
                        {
                            Debug.LogWarning($"Target type not set for effect {e}");
                        }
                        break;
                    case TriggerTargetType.OnlyUser:
                        foreach (var e in _effs[type])
                        {
                            EventBus<StatsEffectTriggerEvent>.Raise(new StatsEffectTriggerEvent(user, new StatsEffect(e), place));
                        }
                        break;
                    case TriggerTargetType.AnyUnit:
                        foreach (var e in _effs[type])
                        {
                            EventBus<StatsEffectTriggerEvent>.Raise(new StatsEffectTriggerEvent(target, new StatsEffect(e), place));
                        }
                        break;
                    case TriggerTargetType.AnyEnemy:
                        if (target.Side == user.Side) return;
                        foreach (var e in _effs[type])
                        {
                            EventBus<StatsEffectTriggerEvent>.Raise(new StatsEffectTriggerEvent(target, new StatsEffect(e), place));
                        }
                        break;
                    case TriggerTargetType.AnyAlly:
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