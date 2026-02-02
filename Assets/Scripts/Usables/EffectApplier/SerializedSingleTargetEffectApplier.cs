using System;
using System.Collections.Generic;
using Arcatech.Actions;
using Arcatech.Effects;
using Arcatech.EventBus;
using CartoonFX;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "usable_applier_single_", menuName = "Usables/Hit Applier/Single target")]
    public class SerializedSingleTargetEffectApplier : SerializedEffectApplier
    {
        public override IEffectApplier Deserialize(CFXR_Effect applicationEffect)
        {
            return new SingleTargetEffectApplier(applicationEffect);
        }
    }
    
    

    public class SingleTargetEffectApplier : EffectApplier
    {
        public SingleTargetEffectApplier(CFXR_Effect applicationEffect) : base(applicationEffect)
        {
        }

        public override void ApplyEffects(BaseGameEntityComponent user, TriggerHitInfo hit, List<ActionResult> effects, Vector3 origin)
        {
            if (hit.TryGetEntityTarget(out BaseGameEntityComponent component))
            {
                foreach (var e in effects)
                {
                    e.ProduceResult(user, component, hit.Position, Quaternion.identity);
                }
                PlayApplicationParticles(hit.Position);
            }
        }
    }
}