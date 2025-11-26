using System;
using System.Collections.Generic;
using Arcatech.Actions;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "New single target hit applier", menuName = "Usables/Hit Applier/Single target")]
    public class SerializedSingleTargetEffectApplier : SerializedEffectApplier
    {
        public override IEffectApplier Deserialize()
        {
            return new SingleTargetEffectApplier();
        }
    }
    
    

    public class SingleTargetEffectApplier : EffectApplier
    {
        protected override void DoApplyLogic(BaseGameEntityComponent user, TriggerHitInfo hit, List<ActionResult> effects, Vector3 origin)
        {
            foreach (var effect in effects)
            {
                effect.ProduceResult(user, hit.Target, hit.Position,hit.Target.transform.rotation);
            }
        }
    }
}