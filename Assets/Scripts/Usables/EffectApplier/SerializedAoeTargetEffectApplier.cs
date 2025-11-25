using System.Collections.Generic;
using Arcatech.Actions;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "New aoe hit applier", menuName = "Usables/Hit Applier/AoE")]
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

    public class AoeTargetEffectApplier : IEffectApplier
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
        public void ApplyEffects(BaseGameEntityComponent user, TriggerHitInfo hit, List<ActionResult> effects, Vector3 origin)
        {
            
            if (!hit.IsValidHit) // "invalid" hit
            {
                Debug.Log("On Invalid Hit[]");
            }
            
            if (Physics.OverlapSphereNonAlloc(hit.Position, _radius, _hits, _targetLayer) ==0 ) return;
            foreach (var h in _hits)
            {
                if (!h.TryGetComponent<BaseGameEntityComponent>(out var unit)) continue;
                foreach (var effect in effects)
                {
                    effect.ProduceResult(user, unit, unit.transform.position, unit.transform.rotation);
                }
            }
        }
    }
}