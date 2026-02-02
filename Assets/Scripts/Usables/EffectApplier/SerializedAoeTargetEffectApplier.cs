using System;
using System.Collections.Generic;
using Arcatech.Actions;
using Arcatech.Effects;
using Arcatech.EventBus;
using CartoonFX;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "usable_applier_aoe_", menuName = "Usables/Hit Applier/AoE")]
    public class SerializedAoeTargetEffectApplier : SerializedEffectApplier
    {
        public float radius;
        [Min(0)] public int maxHits = 16;
        public LayerMask sphereCollisionMask;
        
        public bool affectInitialTarget = true;// apply to the target which triggered the aoe sphere overlap
        public bool affectUser = true; // apply to source
        public override IEffectApplier Deserialize(CFXR_Effect effect)
        {
            return new AoeTargetEffectApplier(this,effect);
        }
    }

    public class AoeTargetEffectApplier : EffectApplier
    {
        private readonly float _radius;
        private readonly Collider[] _hits;
        private readonly bool _affectInitialTarget;
        private readonly bool _affectUser;
        private readonly int _collisionMask;
        public AoeTargetEffectApplier(SerializedAoeTargetEffectApplier cfg,CFXR_Effect appl) : base(appl)
        {
            _radius = cfg.radius;
            _hits = new  Collider[cfg.maxHits];
            _affectInitialTarget = cfg.affectInitialTarget;
            _affectUser = cfg.affectUser;
            _collisionMask = cfg.sphereCollisionMask;
        }
        

        public override void ApplyEffects(BaseGameEntityComponent user, TriggerHitInfo hit, List<ActionResult> effects, Vector3 origin)
        {
            if (Physics.OverlapSphereNonAlloc(hit.Position, _radius, _hits,_collisionMask) ==0 ) return;
            hit.TryGetEntityTarget(out var initTarget);
            
            foreach (var h in _hits)
            {
                if (!h) continue;
                if (!h.TryGetComponent<BaseGameEntityComponent>(out var unit)) continue;

                if (unit == user && !_affectUser) continue;
                if (unit == initTarget && !_affectInitialTarget) continue;
                
                
                foreach (var effect in effects)
                {
                    effect.ProduceResult(user, unit, unit.transform.position, unit.transform.rotation);
                }
                PlayApplicationParticles(unit.EffectSpawn.position);
            }
        }
    }
}