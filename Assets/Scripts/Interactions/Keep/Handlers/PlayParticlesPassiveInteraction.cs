using Arcatech.Effects;
using CartoonFX;
using UnityEngine;

using Arcatech.EventBus;

namespace Arcatech.Interactions
{
    public class PlayParticlesPassiveInteraction : PassiveInteractionHandlerBase
    {
        [SerializeField] private CFXR_Effect particleEffect;

        public override void OnInteractorEnter(IInteractor interactor)
        {
            EventBus<ParticlesEvent>.Raise(new ParticlesEvent(particleEffect,baseGameEntityComponent.EffectSpawn.position));
        }

        public override void OnInteractorExit(IInteractor interactor)
        {
        }
    }
}