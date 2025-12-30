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
        public LayerMask targetLayer;
        public override IEffectApplier Deserialize()
        {
            return new AoeTargetEffectApplier(this);
        }
    }

    public class AoeTargetEffectApplier : EffectApplier
    {
        private readonly float _radius;
        private readonly Collider[] _hits;
        private readonly int _targetLayer;
        public AoeTargetEffectApplier(SerializedAoeTargetEffectApplier cfg)
        {
            _radius = cfg.radius;
            _hits = new  Collider[cfg.maxHits];
            _targetLayer = cfg.targetLayer;
        }

        private ParticlesEvent _particles;
        private bool _setup;

        protected override void DoApplyLogic(BaseGameEntityComponent user, TriggerHitInfo hit, List<ActionResult> effects, Vector3 origin, CFXR_Effect onValid)
        {
            
            if (!_setup)
            {
                _particles = new ParticlesEvent(new[] { onValid });
                _setup = true;
            }
            
            if (Physics.OverlapSphereNonAlloc(hit.Position, _radius, _hits, _targetLayer) ==0 ) return;
            foreach (var h in _hits)
            {
                if (!h) continue;
                if (!h.TryGetComponent<BaseGameEntityComponent>(out var unit)) continue;
                foreach (var effect in effects)
                {
                    effect.ProduceResult(user, unit, unit.transform.position, unit.transform.rotation);
                }
                _particles.Place = hit.Target.EffectSpawn.position;
                EventBus<ParticlesEvent>.Raise(_particles);
            }
        }
    }
}