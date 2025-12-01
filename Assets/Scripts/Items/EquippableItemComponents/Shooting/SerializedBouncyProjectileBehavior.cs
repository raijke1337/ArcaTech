using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    [CreateAssetMenu(fileName = "New Basic Projectile Behavior", menuName = "Projectiles/Behavior/Bouncing")]
    public class SerializedBouncyProjectileBehavior : SerializedBasicProjectileBehavior
    {
        [Min(1)] public float targetSearchRadius;
        
        [Header("Homing (Optional)")]
        [Range(0f, 10f)] 
        [Tooltip("How strongly projectile tracks toward target between bounces. 0 = straight line, higher = tighter tracking")]
        public float homingStrength;
        public override ProjectileBehavior Deserialize(BaseGameEntityComponent owner)
        {
            return new BouncyProjectileBehavior(this, baseProjectileSettings, owner);
        }
    }

    public class BouncyProjectileBehavior : BaseProjectileBehavior
    {
        private readonly float _rad;
        private BaseGameEntityComponent _currentTarget;
        private float _homingStrength;
        private readonly int _layer;
        private Transform _cachedTransform;
        
        private List <BaseGameEntityComponent> _targets;
        
        public BouncyProjectileBehavior(SerializedBouncyProjectileBehavior b, BaseProjectileSettings settings,BaseGameEntityComponent owner) : base(settings,owner)
        {
            _rad = b.targetSearchRadius;
            _layer = LayerMask.NameToLayer("Entities");
            _homingStrength = b.homingStrength;
            _targets  = new List<BaseGameEntityComponent>();
        }
        
        public override void UpdatePosition(float delta, Transform projectileTransform)
        {
            if (!init)
            {
                init = true;
                _cachedTransform =  projectileTransform;
            }

            // Calculate distance to travel this frame
            float distanceThisFrame = _settings.speedPerSecond * delta;

            // Optional: Add homing behavior between bounces
            if (_currentTarget && _homingStrength > 0f)
            {
                Vector3 directionToTarget = (_currentTarget.transform.position - projectileTransform.position).normalized;
                Vector3 currentForward = projectileTransform.forward;
            
                // Smoothly interpolate toward target
                Vector3 newDirection = Vector3.Slerp(currentForward, directionToTarget, _homingStrength * delta);
                projectileTransform.rotation = Quaternion.LookRotation(newDirection);
            }

            // Move projectile forward
            projectileTransform.position += projectileTransform.forward * distanceThisFrame;

            // Track distance traveled in current bounce segment
            _distanceTraveled += distanceThisFrame;

            // Check if exceeded max flight distance
            if (_distanceTraveled >= _settings.maxFlightDistance)
            {
                BehaviorCompleted = true;
            }
        }

        public override void NotifyCollision(TriggerHitInfo hit)
        {
            _targets.Add(hit.Target);
            BaseGameEntityComponent nextTarget = FindNearestTarget(hit.Position);
        
            if (nextTarget)
            {
                _currentTarget = nextTarget;
                _distanceTraveled = 0f; 
                Vector3 directionToTarget = (hit.Position-nextTarget.transform.position).normalized;
                _cachedTransform.rotation = Quaternion.LookRotation(directionToTarget);
            }
            else
            {
                // No valid target found, end behavior
                BehaviorCompleted = true;
            }
        }
        
        
        private BaseGameEntityComponent FindNearestTarget(Vector3 searchPosition)
        {
            Collider[] hitColliders = Physics.OverlapSphere(searchPosition, _rad);
        
            BaseGameEntityComponent nearestEntity = null;
            float nearestDistance = float.MaxValue;

            foreach (var hitCollider in hitColliders)
            {
                BaseGameEntityComponent entity = hitCollider.GetComponent<BaseGameEntityComponent>();
            
                // Skip invalid targets
                if (entity == null) continue;
                if (Owner != null && entity == Owner) continue;
                if (entity == _currentTarget) continue; // Don't bounce back to same target
                if (_targets.Contains(entity)) continue; // already hit this
            
                // Optional: Add team/faction filtering here
                if (entity.GetEntitySide == Owner.GetEntitySide) continue;

                float distance = Vector3.Distance(searchPosition, entity.transform.position);

                if (!(distance < nearestDistance)) continue;
                nearestDistance = distance;
                nearestEntity = entity;
            }
            return nearestEntity;
        }
        
        
        public override void Reset()
        {
            _distanceTraveled = 0f;
            _currentTarget = null;
            init = false;
            BehaviorCompleted = false;
            _targets.Clear();
        }
    }
    
}