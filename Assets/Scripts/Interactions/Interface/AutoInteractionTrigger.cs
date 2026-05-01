using Arcatech.Triggers;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class AutoInteractionTrigger : InteractionTrigger
    {
        [SerializeField,Self] TriggerTrackerComponent activationArea;
        [SerializeField] protected bool triggerOnce = true;
        [SerializeField] protected float cooldown;
        [SerializeField] protected bool useInitiatorAsInteractor = true;

        private float _lastTriggerTime = -999f;
        private bool _hasTriggered;


        public void ResetTrigger()
        {
            _hasTriggered = false;
            _lastTriggerTime = -999f;
        }

        public override void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            if (interactableComponent == null) return;
            if (triggerOnce && _hasTriggered) return;
            if (Time.time < _lastTriggerTime + cooldown) return;
            if (!interactableComponent.IsAvailable) return;
            if (!triggerHitInfo.TryGetEntityTarget(out var initiator)) return;
            // TODO: extend activation to npcs
            if (initiator.CompareTag("Player") &&
                initiator.TryGetComponent(out InteractionComponent component))
            {
                var ctx = new InteractionContext
                {
                    Interactor = useInitiatorAsInteractor ? component : null,
                   // InteractionPoint = target.InteractionPoint 
                    //    ? target.InteractionPoint.position 
                    //    : transform.position
                };
                interactableComponent.StartInteraction(ctx);
                _hasTriggered = true;
                _lastTriggerTime = Time.time;
            }
        }

        public override void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
        }
    }
}