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
        Dictionary <TargetingType, StatsEffect[]> _effs; 
        public ApplyStatChangeEffectResult(SerializedDictionary<TargetingType, StatsEffect[]> cfg)
        {
            _effs = cfg; 
        }


        private bool ValidateEffectTarget(TargetingType targetType, BaseGameEntityComponent source, BaseGameEntityComponent target, out BaseGameEntityComponent finalTarget)
        {
            finalTarget = null;
            switch (targetType)
            {
                case TargetingType.AnyUnit:
                    finalTarget = target; return true;
                case TargetingType.AnyAlly:
                    if (source.GetEntitySide == target.GetEntitySide)
                    {
                        finalTarget = target; 
                        return true;
                    }
                    break;
                case TargetingType.AnyEnemy:
                    if (source.GetEntitySide != target.GetEntitySide)
                    {
                        finalTarget = target; 
                        return true;
                    }

                    break;
                case TargetingType.OnlyUser:
                    if (source == target)
                    {
                        finalTarget = source;
                        return true;
                    }
                    break;
            }
            return false;
        }

        public override void ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target,Transform place)
        {
            foreach (var type in _effs.Keys)
            {
                if (ValidateEffectTarget(type, user, target, out BaseGameEntityComponent final))
                {
                    foreach (var effect in _effs[type])
                    {
                        final.ApplyStatsEffect(effect,user);
                    }
                }
            }
        }
    }
}