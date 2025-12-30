using System.Collections.Generic;
using System.Linq;
using Arcatech.Managers;
using KBCore.Refs;
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
        
        private const float ImpactDirectionEpsilon = 1e-6f;
        private const float RayOriginOffset = 0.05f;
        private const float RaycastRange = RayOriginOffset * 3f;
        private Rigidbody _cachedRigidbody;
        
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

            _hittingColor = component? Color.green : Color.red;
            CleanUpReceivers();
        }

        protected void OnTriggerExit(Collider other)
        {
            if (!Active || _receivers == null || !_receivers.Any()) return;
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

            _hitting = false;
            CleanUpReceivers();
        }

        private (Vector3 position, Vector3 direction, Vector3 normal) CalculateHitGeometry(Collider other)
        {
            Vector3 hitPosition = transform.position;
            Vector3 impactDirection = ResolveImpactDirection(Vector3.zero);
            Vector3 hitNormal = -impactDirection;

            if (other == null)
            {
                return (hitPosition, impactDirection, hitNormal);
            }

            hitPosition = other.ClosestPoint(transform.position);
            impactDirection = ResolveImpactDirection(hitPosition - transform.position);
            hitNormal = -impactDirection;

            if (impactDirection != Vector3.zero)
            {
                var mask = _valid | _invalid;
                if (mask == 0) mask = Physics.AllLayers;

                var directionOffset = impactDirection * RayOriginOffset;
                var rayOrigin = hitPosition - directionOffset;

                if (Physics.Raycast(rayOrigin, impactDirection, out var hit, RaycastRange, mask, QueryTriggerInteraction.Ignore))
                {
                    return (hit.point, impactDirection, hit.normal);
                }

                var reverseOrigin = hitPosition + directionOffset;
                if (Physics.Raycast(reverseOrigin, -impactDirection, out hit, RaycastRange, mask, QueryTriggerInteraction.Ignore))
                {
                    return (hit.point, impactDirection, hit.normal);
                }
            }

            return (hitPosition, impactDirection, hitNormal);
        }

        private Vector3 ResolveImpactDirection(Vector3 candidate)
        {
            if (candidate.sqrMagnitude > ImpactDirectionEpsilon)
            {
                return candidate.normalized;
            }

            _cachedRigidbody ??= GetComponent<Rigidbody>();
            if (_cachedRigidbody != null && _cachedRigidbody.linearVelocity.sqrMagnitude > ImpactDirectionEpsilon)
            {
                return _cachedRigidbody.linearVelocity.normalized;
            }

            if (transform.forward.sqrMagnitude > ImpactDirectionEpsilon)
            {
                return transform.forward.normalized;
            }

            if (transform.up.sqrMagnitude > ImpactDirectionEpsilon)
            {
                return transform.up.normalized;
            }

            if (transform.right.sqrMagnitude > ImpactDirectionEpsilon)
            {
                return transform.right.normalized;
            }
            Debug.Log("Fail to calculate impact direction, returning forward");
            return Vector3.forward;
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