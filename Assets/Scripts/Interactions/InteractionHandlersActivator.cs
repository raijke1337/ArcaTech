using System.Collections.Generic;
using Arcatech.Triggers;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(TriggerTrackerComponent))]
    public class InteractionHandlersActivator : ValidatedMonoBehaviour, ITriggerNotificationReceiver
    {
        [SerializeField,Self] TriggerTrackerComponent triggerTracker;
        [SerializeField] InteractionCondition condition;
        [SerializeField] private List<InteractionHandlerBase> handlers;

        public bool Completed { get; private set; } = false;
    
        public IReadOnlyList<InteractionHandlerBase> Handlers => handlers;
        private void Start()
        {
            triggerTracker.RegisterReceiver(this);
        }

        private void OnDisable()
        {
            triggerTracker.UnregisterReceiver(this);
        }

        public void TriggerEntered(BaseGameEntityComponent enterComponent, TriggerTrackerComponent trigger)
        {
            if (Completed) return;
            if (!enterComponent.CompareTag("Player")) return;
            if (!enterComponent.TryGetComponent(out IInteractor interactor)) return;
            if (!condition.CheckCondition(interactor, null)) return;
           
            foreach (var handler in handlers)
            {
                handler.DoInteraction(interactor);
            }

            Completed = true;
            trigger.Active = false;
        }

        public void TriggerExited(BaseGameEntityComponent exitComponent, TriggerTrackerComponent trigger)
        {
            
        }
    }
}