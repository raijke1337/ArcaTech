using System.Collections.Generic;
using System.Linq;
using Arcatech.Triggers;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(TriggerTrackerComponent))]
    public class InteractionHandlersActivator : ValidatedMonoBehaviour, ITriggerNotificationReceiver
    {
        [SerializeField,Self] TriggerTrackerComponent triggerTracker;
        [SerializeField] private bool allowMultipleActivations = false;
        [SerializeField] InteractionCondition condition;
        [SerializeField] private List<InteractionHandlerBase> handlers;
        public bool Completed { get; private set; } = false;
    
        public IReadOnlyList<InteractionHandlerBase> Handlers => handlers;
        private void Start()
        {
            var onThis = GetComponentsInChildren<InteractionHandlerBase>(true);
            handlers.AddRange(onThis.Except(handlers));
            
            triggerTracker.RegisterReceiver(this);
        }

        private void OnDisable()
        {
            triggerTracker.UnregisterReceiver(this);
        }

        bool ValidateComponent(BaseGameEntityComponent comp, out IInteractor interactor )
        {
            interactor = null;
            return comp.CompareTag("Player") && comp.TryGetComponent(out interactor) &&
                   condition.CheckCondition(interactor, null);
        }

        public void TriggerEntered(BaseGameEntityComponent enterComponent, TriggerTrackerComponent trigger)
        {
            if (Completed) return;
            if (!ValidateComponent(enterComponent, out var interactor)) return;
            foreach (var handler in handlers)
            {
                handler.DoInteraction(interactor);
            }
        }

        public void TriggerExited(BaseGameEntityComponent exitComponent, TriggerTrackerComponent trigger)
        {
            
            if (Completed) return;
            if (!ValidateComponent(exitComponent, out var interactor)) return;
            
            if (!allowMultipleActivations)
            {
                Completed = true;
                trigger.Active = false;
            }
            
            foreach (var handler in handlers)
            {
                handler.EndInteraction(interactor);
            }
        }
    }
}