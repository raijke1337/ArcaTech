using Arcatech.Effects;
using Arcatech.EventBus;
using CartoonFX;
using UnityEngine;

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