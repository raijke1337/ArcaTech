using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Triggers
{
    [RequireComponent(typeof(Collider))]
    public class TriggerTrackerComponent : ValidatedMonoBehaviour, ITriggerNotificationProvider
    {
        [SerializeField, Self] Collider _collider;
        protected override void OnValidate()
        {
            base.OnValidate();
            _collider.includeLayers = LayerMask.GetMask("Entities");
        }
        private List<ITriggerNotificationReceiver> receivers;


        public bool Active { get; set; }
        
        private void OnEnable()
        { 
            _collider.isTrigger = true;
            var r = GetComponentsInChildren<ITriggerNotificationReceiver>();
            foreach (var r2 in r) RegisterReceiver(r2);
        }

        public void RegisterReceiver(ITriggerNotificationReceiver receiver)
        {
            receivers ??= new List<ITriggerNotificationReceiver>();
            
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
            if (toRemove.Count <= 0) return;
            //Debug.Log($"Cleaning up {toRemove.Count} receivers");
            receivers = receivers.Except(toRemove).ToList();
            toRemove.Clear();
        }

        public void RecheckCollisions() => StartCoroutine(ColliderRefresh());

        private IEnumerator ColliderRefresh()
        {
            _collider.enabled = false;
            yield return new WaitForEndOfFrame();
            _collider.enabled = true;
        }
        
        protected void OnTriggerEnter(Collider other)
        {
            if (!Active || receivers == null || !receivers.Any()) return;
            if (other.TryGetComponent<BaseGameEntityComponent>(out var component))
            {
                Debug.Log($"Bonk {component.GetName}. Notify {receivers.Count} receivers");
                
                foreach (var receiver in receivers)
                {
                    receiver.TriggerEntered(component, this);
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
                    receiver.TriggerExited(component, this);
                }
            }

            CleanUpReceivers();
        }
    }
}