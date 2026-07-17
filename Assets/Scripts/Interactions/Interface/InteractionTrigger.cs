using System;
using Arcatech.SaveSystem;
using Arcatech.Triggers;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Assertions;

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

        [Space, Header("Timer")] [SerializeField]
        [Tooltip("0 means doesn't expire"),Range(0,60)]private float expirationTime = 0f;
        [SerializeField] bool destroyAfterExpiry = false;
        private float _startTime;
        private float _expireAt;
        protected override void OnValidate()
        {
            base.OnValidate();
            Assert.IsNotNull(triggerTrackerComponent);
        }
        public abstract void TriggerEntered(TriggerHitInfo triggerHitInfo);
        public abstract void TriggerExited(TriggerHitInfo triggerExitInfo);

        protected virtual void Start()
        {
            _startTime = Time.time;
            if (expirationTime > 0) _expireAt =  _startTime + expirationTime;
            triggerTrackerComponent.RegisterReceiver(this);
        }

        protected virtual void OnDisable()
        {
            triggerTrackerComponent.UnregisterReceiver(this);
        }

        private void Update()
        {
            if (Time.time > _expireAt)
            {
                triggerTrackerComponent.Active = false;
                if (destroyAfterExpiry)
                    gameObject.SetActive(false);
            }
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