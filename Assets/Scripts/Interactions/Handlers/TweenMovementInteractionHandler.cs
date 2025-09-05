using DG.Tweening;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class TweenMovementInteractionHandler : InteractionHandlerBase
    {
        [SerializeField] SerializedDOTweener tween;
        Tween cached;
        private void OnEnable()
        {
            cached = tween.GetTween(transform).Pause(); 
        }
        public override void DoInteraction(IInteractor interactor, IInteractive item, IInteractionContext context)
        {
            cached.Play();
        }
    }

}
