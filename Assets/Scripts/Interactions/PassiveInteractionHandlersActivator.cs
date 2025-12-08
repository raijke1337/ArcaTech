using System.Collections.Generic;
using System.Linq;
using Arcatech.Triggers;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(TriggerTrackerComponent))]
    public class PassiveInteractionHandlersActivator : ValidatedMonoBehaviour, ITriggerNotificationReceiver
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


        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            if (Completed) return;
            if (!ValidateComponent(triggerHitInfo.Target, out var interactor)) return;
            foreach (var handler in handlers)
            {
                handler.OnPlayerEnter();
            }
        }

        public void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            
            if (Completed) return;
            if (!ValidateComponent(triggerExitInfo.Target, out var interactor)) return;
            
            if (!allowMultipleActivations)
            {
                Completed = true;
                triggerExitInfo.Source.Active = false;
            }
            
            foreach (var handler in handlers)
            {
                handler.OnPlayerExit();
            }
        }
    }
}