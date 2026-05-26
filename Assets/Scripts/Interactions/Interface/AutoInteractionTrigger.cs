using UnityEngine;

namespace Arcatech.Interactions
{
    public class AutoInteractionTrigger : InteractionTrigger
    {
        
        [SerializeField,Header("Unassigned = apply to all")] Side affectedSide = Side.Unassigned;


        
        
        public override void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            if (interactableComponent == null) return;
            if (triggerOnce && HasTriggered) return;
            if (Time.time < LastTriggerTime + cooldown) return;
            if (!interactableComponent.IsAvailable) return;
            if (!triggerHitInfo.TryGetEntityTarget(out var initiator)) return;
            if (affectedSide!=Side.Unassigned && initiator.GetEntitySide != affectedSide) return;
            

            initiator.TryGetComponent(out InteractionComponent component);
            var ctx = new InteractionContext
            {
                Interactor = component,
                State = InteractionState.Starting
            };
            
            interactableComponent.StartInteraction(ctx);
            HasTriggered = true;
            LastTriggerTime = Time.time;
        }

        public override void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            
        }

        private void Update()
        {
            // todo add multiple activations
        }
    }
}