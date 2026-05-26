using Arcatech.SaveSystem;
using Arcatech.Triggers;
using KBCore.Refs;
using NUnit.Framework;
using UnityEngine;

namespace Arcatech.Interactions
{
    /// <summary>
    /// defines how the interactable component activates
    /// </summary>
    [RequireComponent(typeof(InteractableComponent))]
    public abstract class InteractionTrigger : ValidatedMonoBehaviour, ITriggerNotificationReceiver
    {
        [SerializeField] protected TriggerTrackerComponent triggerTrackerComponent;
        [SerializeField,Self] protected InteractableComponent interactableComponent;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            Assert.IsNotNull(triggerTrackerComponent);
        }
        public abstract void TriggerEntered(TriggerHitInfo triggerHitInfo);
        public abstract void TriggerExited(TriggerHitInfo triggerExitInfo);

        protected virtual void Start()
        {
            triggerTrackerComponent.RegisterReceiver(this);
        }

        protected virtual void OnDisable()
        {
            triggerTrackerComponent.UnregisterReceiver(this);
        }
        
                
        public void ResetTrigger()
        {
            HasTriggered = false;
            LastTriggerTime = -999f;
        }
        
        protected float LastTriggerTime = -999f;
        protected bool HasTriggered;
        [SerializeField] protected bool triggerOnce = true;
        [SerializeField] protected float cooldown;
    }
}