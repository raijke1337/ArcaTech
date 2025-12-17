using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Managers;
using KBCore.Refs;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Arcatech.Triggers
{
    [RequireComponent(typeof(Collider))]
    public class TriggerTrackerComponent : ValidatedMonoBehaviour, ITriggerNotificationProvider
    {
        [SerializeField, Self] private new Collider collider;

        private List<ITriggerNotificationReceiver> _receivers;
        public bool Active { get; set; } = true;

        private LayerMask _valid;
        private LayerMask _invalid;
        
        private void OnEnable()
        { 
            collider.isTrigger = true;
            _valid = LayerMask.GetMask(DataManager.GameRules.ValidHitsLayer);
            _invalid = LayerMask.GetMask(DataManager.GameRules.InvalidHitsLayer);

         collider.includeLayers = (_valid| _invalid);
            
            var r = GetComponentsInChildren<ITriggerNotificationReceiver>();
            foreach (var r2 in r) RegisterReceiver(r2);
        }
        

        public void RegisterReceiver(ITriggerNotificationReceiver receiver)
        {
            _receivers ??= new List<ITriggerNotificationReceiver>();
            
            if (_receivers.Contains(receiver)) return;
            _receivers.Add(receiver);
        }

        private List<ITriggerNotificationReceiver> toRemove = new List<ITriggerNotificationReceiver>();
        public void UnregisterReceiver(ITriggerNotificationReceiver receiver)
        {
            if (_receivers.Contains(receiver)) toRemove.Add(receiver);
        }

        private void CleanUpReceivers()
        {
            if (toRemove.Count <= 0) return;
            //Debug.Log($"Cleaning up {toRemove.Count} receivers");
            _receivers = _receivers.Except(toRemove).ToList();
            toRemove.Clear();
        }
        
        protected void OnTriggerEnter(Collider other)
        {
            if (!Active || _receivers == null || !_receivers.Any()) return;
            if (other.isTrigger) return;
            Debug.Log($"{this.name}: Hitting {other.gameObject.name}");
            
            _hitting = true;
            other.TryGetComponent<BaseGameEntityComponent>(out var component);
            
            foreach (var receiver in _receivers)
            {
                receiver.TriggerEntered(new TriggerHitInfo(this, component,transform.position,Time.time));
            }              
            
            _hittingColor = component? Color.green : Color.red;
            CleanUpReceivers();
        }

        protected void OnTriggerExit(Collider other)
        {
            if (!Active || _receivers == null || !_receivers.Any()) return;
            if (other.TryGetComponent<BaseGameEntityComponent>(out var component))
            {
                foreach (var receiver in _receivers)
                {
                    receiver.TriggerExited(new TriggerHitInfo(this,component,transform.position,Time.time));
                }
            }

            _hitting = false;
            CleanUpReceivers();
        }


        private bool _hitting = false;
        private Color _hittingColor = Color.red;
        
        private void OnDrawGizmos()
        {
            if (_receivers == null || !_receivers.Any())
            {
                Gizmos.color = Color.gray;
            }
            else
            {
                Gizmos.color = Color.yellow;
            }

            if (_hitting)
            {
                Gizmos.color = _hittingColor;
            }

            if (collider is BoxCollider box)
            {
                var colliderTransform = box.transform;
                var center = box.center;
                var size = box.size;
                
                var matrix = Matrix4x4.TRS(
                    colliderTransform.TransformPoint(center),
                    colliderTransform.rotation,
                    Vector3.Scale(colliderTransform.lossyScale, size)
                );
                
                var inverseScale = new Vector3(
                    1f / colliderTransform.lossyScale.x,
                    1f / colliderTransform.lossyScale.y,
                    1f / colliderTransform.lossyScale.z
                );
                
                Gizmos.matrix = matrix;
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                Gizmos.matrix = Matrix4x4.identity;
            }

            
        }
    }
}