using System.Collections.Generic;
using System.Linq;
using Arcatech.Triggers;
using UnityEngine;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(TriggerTrackerComponent))]
    public class PassiveEventsTriggerComponent : EventsTrigger
    {
        [SerializeField] private float pausePlayerTime = 0f;
        [SerializeField] InteractionCondition condition;
        [Header("Assign extras here")]
        [SerializeField] private List<PassiveInteractionHandlerBase> handlers;
        public bool Completed { get; private set; } = false;
    
        public IReadOnlyList<PassiveInteractionHandlerBase> Handlers => handlers;
        protected override void Start()
        {
            var onThis = GetComponentsInChildren<PassiveInteractionHandlerBase>(true);
            handlers.AddRange(onThis.Except(handlers));
            base.Start();
        }
        
        bool ValidateComponent(Collider comp, out IInteractor interactor )
        {
            interactor = null;
            return comp.CompareTag("Player") && comp.TryGetComponent(out interactor) &&
                   condition.CheckCondition(interactor, null);
        }


        public override void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            if (Completed) return;
            if (!ValidateComponent(triggerHitInfo.TargetCollider, out var interactor)) return;
            foreach (var handler in handlers)
            {
                handler.OnInteractorEnter(interactor);
            }
            if (pausePlayerTime > 0f && triggerHitInfo.TryGetEntityTarget(out var p))
            {
                p.Pauser.Pause(pausePlayerTime);
            }
            Completed = true;
            if (disappearWhenTriggered)
            {
                StartDisable();
            }
        }

        public override void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            
            if (Completed) return;
            if (!ValidateComponent(triggerExitInfo.TargetCollider, out var interactor)) return;

            
            foreach (var handler in handlers)
            {
                handler.OnInteractorExit(interactor);
            }
                        
            if (!allowMultipleActivations)
            {
                Completed = true;
                triggerExitInfo.Source.Active = false;
            }
        }

    }
}