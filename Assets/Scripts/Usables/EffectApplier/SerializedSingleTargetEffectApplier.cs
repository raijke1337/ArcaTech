using System;
using System.Collections.Generic;
using Arcatech.Actions;
using Arcatech.Effects;
using Arcatech.EventBus;
using CartoonFX;
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
        private ParticlesEvent _particles;
        private bool _setup;
        protected override void DoApplyLogic(BaseGameEntityComponent user, TriggerHitInfo hit, List<ActionResult> effects, Vector3 origin,  CFXR_Effect onValidApply)
        {
            if (!_setup)
            {
                _particles = new ParticlesEvent(new[] { onValidApply });
                _setup = true;
            }
            foreach (var effect in effects)
            {
                effect.ProduceResult(user, hit.Target, hit.Position,hit.Target.transform.rotation);
            }
            _particles.Place = hit.Target.EffectSpawn.position;
            EventBus<ParticlesEvent>.Raise(_particles);
        }
    }
}