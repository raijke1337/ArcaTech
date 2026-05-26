using System.Collections.Generic;
using System.Linq;
using Arcatech.Stats;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "applyStat_", menuName = "Actions/Action Result/Apply Stat Change", order = 3)]
    public class SerializedApplyUsableEffectsResult : SerializedActionResult
    {

        [SerializeField] SerializedDictionary<TargetingType, AppliedStatsDeltaEffect[]> StatChanges;
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
        Dictionary <TargetingType, AppliedStatsDeltaEffect[]> _effs; 
        public ApplyStatChangeEffectResult(SerializedDictionary<TargetingType, AppliedStatsDeltaEffect[]> cfg)
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
                    if (target.GetEntitySide != source.GetEntitySide && source.GetEntitySide != Side.Unassigned)
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
                    break;
            }
            if (finalTarget == null) return false;
            return true;
        }

        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Vector3 place,
            Quaternion placeRot)
        {
            bool result = true;
            foreach (var type in _effs.Keys)
            {
                if (TryPickEffectTarget(type, user, target, out var final))
                {
                    if (final==null) return false;

                    foreach (var effect in _effs[type])
                    {
                        if (!final.ApplyStatsEffect(effect, user))
                        {
                            result = false;
                        }
                    }
                }
            }
            return result;
        }
    }
}