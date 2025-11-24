using System;
using System.Collections.Generic;
using Arcatech.Actions;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "New Usables Item", menuName = "Usables/Hit Applier/Single target")]
    public class SerializedSingleTargetEffectApplier : SerializedEffectApplier
    {
        public override IEffectApplier Deserialize()
        {
            return new SingleTargetEffectApplier();
        }
    }
    
    

    public class SingleTargetEffectApplier : IEffectApplier
    {
        public void ApplyEffects(BaseGameEntityComponent user, TriggerHitInfo hit, List<ActionResult> effects, Vector3 origin)
        {
            if (!hit.Target) // "invalid" hit
            {
                Debug.Log("Invalid hit");
                return;
            }

            foreach (var effect in effects)
            {
                effect.ProduceResult(user, hit.Target, hit.Position,hit.Target.transform.rotation);
            }
        }
    }
}