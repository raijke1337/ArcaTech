using System.Collections.Generic;
using Arcatech.Triggers;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public abstract class EventsTrigger : ValidatedMonoBehaviour,ITriggerNotificationReceiver, IKillerComponent
    {
        [Space,SerializeField, Self] protected BaseGameEntityComponent baseComp;
        [SerializeField,Child] protected TriggerTrackerComponent activationArea;
        
        private List<IKillableComponent> _killableComponents;
        [SerializeField] protected bool disappearWhenTriggered;
        
        [SerializeField] protected bool allowMultipleActivations = false;
        
        public abstract void TriggerEntered(TriggerHitInfo triggerHitInfo);
        public abstract void TriggerExited(TriggerHitInfo triggerExitInfo);
        
        protected virtual void Start()
        {
            _killableComponents =  new  List<IKillableComponent>(GetComponentsInChildren<IKillableComponent>());
            activationArea.Active = true;
            activationArea.RegisterReceiver(this);
        }
        protected virtual void OnDisable()
        {
            activationArea.UnregisterReceiver(this);
        }

        protected void StartDisable()
        {
            foreach (var k in _killableComponents)
            {
                k.SetKilled(this,true);
            }
        }

        public string KilledBy  => "Event fired successfully";
    }
    
    
}