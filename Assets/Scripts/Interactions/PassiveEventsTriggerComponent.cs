using System.Collections.Generic;
using System.Linq;
using Arcatech.Triggers;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(TriggerTrackerComponent))]
    public class PassiveEventsTriggerComponent : ValidatedMonoBehaviour, ITriggerNotificationReceiver
    {
        [SerializeField,Self] TriggerTrackerComponent activationArea;
        [SerializeField] private bool allowMultipleActivations = false;
        [SerializeField] InteractionCondition condition;
        [SerializeField] private List<PassiveInteractionHandlerBase> handlers;
        public bool Completed { get; private set; } = false;
    
        public IReadOnlyList<PassiveInteractionHandlerBase> Handlers => handlers;
        private void Start()
        {
            var onThis = GetComponentsInChildren<PassiveInteractionHandlerBase>(true);
            handlers.AddRange(onThis.Except(handlers));
            
            activationArea.RegisterReceiver(this);
        }

        private void OnDisable()
        {
            activationArea.UnregisterReceiver(this);
        }

        bool ValidateComponent(Collider comp, out IInteractor interactor )
        {
            interactor = null;
            return comp.CompareTag("Player") && comp.TryGetComponent(out interactor) &&
                   condition.CheckCondition(interactor, null);
        }


        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            if (Completed) return;
            if (!ValidateComponent(triggerHitInfo.TargetCollider, out var interactor)) return;
            foreach (var handler in handlers)
            {
                handler.OnInteractorEnter(interactor);
            }
        }

        public void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            
            if (Completed) return;
            if (!ValidateComponent(triggerExitInfo.TargetCollider, out var interactor)) return;
            
            if (!allowMultipleActivations)
            {
                Completed = true;
                triggerExitInfo.Source.Active = false;
            }
            
            foreach (var handler in handlers)
            {
                handler.OnInteractorExit(interactor);
            }
        }
    }
}