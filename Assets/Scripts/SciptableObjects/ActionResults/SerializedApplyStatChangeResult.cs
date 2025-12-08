using System;
using Arcatech.Triggers;
using UnityEngine;
using Arcatech.EventBus;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System.Linq;
using Arcatech.Stats;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "New apply stat change result", menuName = "Actions/Action Result/Apply Stat Change", order = 3)]
    public class SerializedApplyStatChangeResult : SerializedActionResult
    {

        [SerializeField] SerializedDictionary<TargetingType, StatsEffect[]> StatChanges;
        public override ActionResult Deserialize()
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
        Dictionary <TargetingType, StatsEffect[]> _effs; 
        public ApplyStatChangeEffectResult(SerializedDictionary<TargetingType, StatsEffect[]> cfg)
        {
            _effs = cfg; 
        }
        private bool TryPickEffectTarget(TargetingType targetType, BaseGameEntityComponent source, BaseGameEntityComponent target, out BaseGameEntityComponent finalTarget)
        {
            finalTarget = null;
            switch (targetType)
            {
                case TargetingType.None:
                    break;
                case TargetingType.ApplyToSource:
                    finalTarget = source;
                    return true;
                case TargetingType.ApplyToEnemyTarget:
                    if (target == source) return false;
                    if (target.GetEntitySide != source.GetEntitySide)
                        finalTarget = target;
                    return true;
                case TargetingType.ApplyToAlliedTarget:
                    if (target == source) return false;
                    if (target.GetEntitySide == source.GetEntitySide)
                        finalTarget = target;
                    break;
                case TargetingType.ApplyToAnyTargetExceptSource:
                    if (target == source) return false;
                    finalTarget = target;
                    break;
                case TargetingType.ApplyToAnyTarget:
                    finalTarget = target;
                    return true;
            }

            return finalTarget;
        }

        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Vector3 place,
            Quaternion placeRot)
        {
            foreach (var type in _effs.Keys)
            {
                if (TryPickEffectTarget(type, user, target, out var final))
                {
                    foreach (var effect in _effs[type])
                    {
                        final.ApplyStatsEffect(effect,user);
                    }
                }
            }

            return false;
        }
    }
}