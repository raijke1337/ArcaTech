using System.Collections.Generic;
using Arcatech.Actions;
using Arcatech.Effects;
using Arcatech.EventBus;
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
        private ParticlesEvent _onApplication;
        public EffectApplier(CFXR_Effect applicationEffect)
        {
            _onApplication = new ParticlesEvent (applicationEffect);
        }

        public abstract void ApplyEffects(BaseGameEntityComponent user, TriggerHitInfo hit, List<ActionResult> effects,
            Vector3 origin);

        protected void PlayApplicationParticles(Vector3 position)
        {
            _onApplication.Place = position;
            EventBus<ParticlesEvent>.Raise(_onApplication);
        }
    }
}