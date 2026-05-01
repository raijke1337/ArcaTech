using System;
using Arcatech.Triggers;
using KBCore.Refs;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
    /// <summary>
    /// defines how the interactible component activates
    /// </summary>
    [RequireComponent(typeof(InteractableComponent))]
    public abstract class InteractionTrigger : ValidatedMonoBehaviour, ITriggerNotificationReceiver
    {
        public abstract void TriggerEntered(TriggerHitInfo triggerHitInfo);
        public abstract void TriggerExited(TriggerHitInfo triggerExitInfo);
        
        [SerializeField] TriggerTrackerComponent triggerTrackerComponent;
        [SerializeField,Self] protected InteractableComponent interactableComponent;

        protected override void OnValidate()
        {
            base.OnValidate();
            Assert.IsNotNull(triggerTrackerComponent);
        }
        private void OnEnable()
        {
            triggerTrackerComponent.RegisterReceiver(this);
        }

        private void OnDisable()
        {
            triggerTrackerComponent.UnregisterReceiver(this);
        }
    }
}