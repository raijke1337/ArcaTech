using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Triggers
{
    [RequireComponent(typeof(Collider), typeof(BaseGameEntityComponent))]
    public class TriggerTrackerComponent : ValidatedMonoBehaviour
    {
        public Collider Collider => collider;
        [SerializeField, Self] Collider collider;
        [SerializeField, Self] BaseGameEntityComponent entity;
        
        private List<ITriggerNotificationReceiver> receivers;

        public bool Active { get; set; } = true;
        
        
        private void OnEnable()
        { 
            collider.isTrigger = true;
            receivers =  new List<ITriggerNotificationReceiver>(GetComponentsInChildren<ITriggerNotificationReceiver>());
        }

        public void RegisterReceiver(ITriggerNotificationReceiver receiver)
        {
            receivers ??= new List<ITriggerNotificationReceiver>(GetComponentsInChildren<ITriggerNotificationReceiver>());
            if (receivers.Contains(receiver)) return;
            receivers.Add(receiver);
        }

        private List<ITriggerNotificationReceiver> toRemove = new List<ITriggerNotificationReceiver>();
        public void UnregisterReceiver(ITriggerNotificationReceiver receiver)
        {
            if (receivers.Contains(receiver)) toRemove.Add(receiver);
        }

        private void CleanUpReceivers()
        {
            receivers.Where(t=> toRemove.Contains(t)).ToList().ForEach(t => receivers.Remove(t));
            toRemove.Clear();
        }

        public void RecheckCollisions() => StartCoroutine(ColliderRefresh());

        private IEnumerator ColliderRefresh()
        {
            collider.enabled = false;
            yield return new WaitForEndOfFrame();
            collider.enabled = true;
        }
        
        protected void OnTriggerEnter(Collider other)
        {
            if (!Active || receivers == null || !receivers.Any()) return;
            if (other.TryGetComponent<BaseGameEntityComponent>(out var component))
            {
                Debug.Log($"Bonk {component.GetName}. Notify {receivers.Count} receivers");
                
                foreach (var receiver in receivers)
                {
                    receiver.TriggerEntered(component, entity);
                }
            }
            CleanUpReceivers();
        }

        protected void OnTriggerExit(Collider other)
        {
            if (!Active || receivers == null || !receivers.Any()) return;
            if (other.TryGetComponent<BaseGameEntityComponent>(out var component))
            {
                foreach (var receiver in receivers)
                {
                    receiver.TriggerExited(component, entity);
                }
            }

            CleanUpReceivers();
        }
    }
}