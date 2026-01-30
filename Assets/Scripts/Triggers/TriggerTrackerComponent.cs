using System.Collections.Generic;
using Arcatech.Managers;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Triggers
{
    [RequireComponent(typeof(Collider))]
    public class TriggerTrackerComponent : ValidatedMonoBehaviour, ITriggerNotificationProvider
    {
        #region ITriggerNotificationProvider
        
        private readonly HashSet<ITriggerNotificationReceiver> _receivers = new();
        public bool Active { get; set; } = true;
        public void RegisterReceiver(ITriggerNotificationReceiver r) => _receivers.Add(r);
        public void UnregisterReceiver(ITriggerNotificationReceiver r) => _receivers.Remove(r);
      
        #endregion
        
        [SerializeField, Self] private Collider triggerCollider;

        private LayerMask _valid;
        private LayerMask _invalid;
        
        
        // todo: maybe move into a game rule config
        
        private const float ImpactDirectionEpsilon = 1e-6f;
        private const float RayOriginOffset = 0.05f;
        private const float RaycastRange = RayOriginOffset * 3f;
        private Rigidbody _cachedRigidbody;
        

        private void OnEnable()
        { 
            triggerCollider.isTrigger = true;
            _valid = LayerMask.GetMask(DataManager.GameRules.ValidHitsLayer);
            _invalid = LayerMask.GetMask(DataManager.GameRules.InvalidHitsLayer);

            triggerCollider.includeLayers = (_valid| _invalid);
            
            var r = GetComponentsInChildren<ITriggerNotificationReceiver>();
            foreach (var r2 in r) RegisterReceiver(r2);
        }
        private bool CanNotify() =>
            Active && _receivers.Count > 0;

        protected void OnTriggerEnter(Collider other)
        {
            if (!CanNotify() || other.isTrigger) return;

            other.TryGetComponent<BaseGameEntityComponent>(out var component);

            var hitGeometry = CalculateHitGeometry(other);
            foreach (var receiver in _receivers)
            {
                receiver.TriggerEntered(new TriggerHitInfo(
                    this,
                    component,
                    hitGeometry.position,
                    hitGeometry.direction,
                    hitGeometry.normal,
                    Time.time));
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            if (!CanNotify()|| other.isTrigger) return;
            
            if (other.TryGetComponent<BaseGameEntityComponent>(out var component))
            {
                var hitGeometry = CalculateHitGeometry(other);
                foreach (var receiver in _receivers)
                {
                    receiver.TriggerExited(new TriggerHitInfo(
                        this,
                        component,
                        hitGeometry.position,
                        hitGeometry.direction,
                        hitGeometry.normal,
                        Time.time));
                }
            }

        }


        private void OnDisable()
        {
            _receivers.Clear();
            _cachedRigidbody = null;
        }

        
        // Simplified: Assume impactDirection provides a reasonable normal for triggers
        private (Vector3 position, Vector3 direction, Vector3 normal) CalculateHitGeometry(Collider other)
        {
            if (other == null) 
                return (transform.position, Vector3.zero, Vector3.zero);

            var rb = other.attachedRigidbody;
            var hitPosition = other.ClosestPoint(transform.position);
            var rawDirection = hitPosition - transform.position;
            var impactDirection = ResolveImpactDirection(rawDirection,rb);
            var hitNormal = -impactDirection;
            
            return (hitPosition, impactDirection, hitNormal);
        }

       // updated for better precision
       private Vector3 ResolveImpactDirection(Vector3 candidate, Rigidbody otherRigidbody = null)
       {
           _cachedRigidbody ??= GetComponent<Rigidbody>();

           // 1. Try relative velocity if both have rigidbodies
           // hit on a moving enemy
           if (_cachedRigidbody != null && otherRigidbody != null)
           {
               Vector3 relativeVelocity = otherRigidbody.linearVelocity - _cachedRigidbody.linearVelocity;
               if (relativeVelocity.sqrMagnitude > ImpactDirectionEpsilon)
               {
                   return relativeVelocity.normalized;
               }
           }

           // 2. Try 'other' object's velocity if it has one and trigger doesn't move much 
           // Static trigger (trap, box activation area, etc)
           if (otherRigidbody != null && otherRigidbody.linearVelocity.sqrMagnitude > ImpactDirectionEpsilon)
           {
               return otherRigidbody.linearVelocity.normalized;
           }

           // 3. Try trigger's velocity if it has one
           // other object has no rigidbody (wall or floor)
           if (_cachedRigidbody != null && _cachedRigidbody.linearVelocity.sqrMagnitude > ImpactDirectionEpsilon)
           {
               return _cachedRigidbody.linearVelocity.normalized;
           }

           // 4. Fallback to candidate (from closest point) IF it's not based on *center* to point
           if (candidate.sqrMagnitude > ImpactDirectionEpsilon)
           {
               Debug.LogWarning($"Fallback to candidate (from closest point)" +
                                $"{candidate} is problematic, check origin point");
               
               return -candidate.normalized; // Invert candidate if it's from current transform.position to hitPosition
           }
    
           // 5. Fallback to object's forward direction
           if (transform.forward.sqrMagnitude > ImpactDirectionEpsilon)
           {
               Debug.LogWarning($"Fallback to {this.name} forward direction");
               
               return transform.forward.normalized;
           }
           Debug.LogWarning("Failed to calculate impact direction, returning Vector3.forward on " + gameObject.name);
           return Vector3.forward;
       }
    }
}