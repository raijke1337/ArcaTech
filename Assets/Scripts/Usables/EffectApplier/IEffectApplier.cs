using System.Collections.Generic;
using Arcatech.Actions;
using Arcatech.Effects;
using Arcatech.EventBus;
using Arcatech.Usables.Effects;
using ArcaTech.Usables.Effects;
using CartoonFX;
using UnityEngine;

namespace Arcatech.Usables
{
    // apply directly, in aoe or to self maybe?
    public interface IEffectApplier
    {
        void ApplyEffects(BaseGameEntityComponent user, TriggerHitInfo hit, List<ActionResult> effects, Vector3 origin);
    }

    public abstract class SerializedEffectApplier : ScriptableObject
    {
        public abstract IEffectApplier Deserialize(CFXR_Effect applicationEffect);
    }
    

    public abstract class EffectApplier : IEffectApplier
    {
        private CFXR_Effect effect;
        public EffectApplier(CFXR_Effect applicationEffect) => effect = applicationEffect;
    
        public abstract void ApplyEffects(BaseGameEntityComponent user, TriggerHitInfo hit, List<ActionResult> effects,
            Vector3 origin);

        protected void ApplyToTarget(BaseGameEntityComponent user, BaseGameEntityComponent target,
            TriggerHitInfo hit, List<ActionResult> effects, bool applyAtHitPosition)
        {
            target.TryGetComponent<DestructibleComponent>(out var destructible);
            float? previousDamage = destructible != null ? destructible.BeginHit() : null;
            float damage = 0f;
            try
            {
                foreach (var effect in effects)
                {
                    var place = applyAtHitPosition ? hit.Position : target.transform.position;
                    var rotation = applyAtHitPosition ? Quaternion.identity : target.transform.rotation;
                    effect.ProduceResult(user, target, place, rotation);
                }
            }
            finally
            {
                if (destructible != null) damage = destructible.EndHit(previousDamage);
            }

            PlayApplicationParticles(applyAtHitPosition ? hit.Position : target.EffectSpawn.position);
            if (destructible != null) destructible.CompleteHit(damage, hit);
        }
    
        protected void PlayApplicationParticles(Vector3 position)
        {
            EventBus<ParticlesEvent>.Raise(new  ParticlesEvent(effect, position));
        }
    }
}
