
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    [CreateAssetMenu(fileName = "projectileBehavior_", menuName = "Projectiles/Behavior/Bouncing")]
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

        protected override void RotateProjectile(float distanceThisFrame, Transform projectileTransform, float deltaTime)
        {
            if (_currentTarget && _homingStrength > 0f)
            {
                Vector3 directionToTarget = (_currentTarget.transform.position - projectileTransform.position).normalized;
                Vector3 currentForward = projectileTransform.forward;
            
                // Smoothly interpolate toward target
                Vector3 newDirection = Vector3.Slerp(currentForward, directionToTarget, _homingStrength * deltaTime);
                projectileTransform.rotation = Quaternion.LookRotation(newDirection);
            }
        }

        public override void NotifyCollision(TriggerHitInfo hit)
        {
            if (hit.Target == Owner) return;

            if (!hit.IsValidHit)
            {
                // --- Ricochet Logic 
                Vector3 incomingDirection = hit.ImpactDirection; // Use the precise impact direction
                Vector3 surfaceNormal = hit.Normal; // Use the accurate surface normal

                // Calculate the reflected direction
                Vector3 reflectedDirection = Vector3.Reflect(incomingDirection, surfaceNormal);

                // Apply a small random spread to the reflection for more natural bounces (optional)
                // Example: apply +/- 5 degrees on Y axis (for mostly horizontal surfaces)
                // You might want to adjust based on the normal for more generalized spread.
                float randomAngle = Random.Range(-5f, 5f);
                reflectedDirection = Quaternion.AngleAxis(randomAngle, Vector3.up) * reflectedDirection;
                // Or rotate around an axis perpendicular to both normal and incoming direction for more complex spread if needed.

                // Update projectile's rotation to the new reflected direction
                _cachedTransform.rotation = Quaternion.LookRotation(reflectedDirection);

                // Move the projectile slightly away from the collision point to prevent immediate re-collision
                _cachedTransform.position = hit.Position + reflectedDirection;

                Debug.Log(
                    $"Ricochet! Initial dir: {incomingDirection}, Normal: {surfaceNormal}, Reflected: {reflectedDirection}");

                // IMPORTANT: If you want the normalized speed curve to restart after a ricochet,
                // or if ricochet should reset its "distance traveled", you need to handle that here.
                // Resetting `_distanceTraveled` might be appropriate depending on game design.
                // _distanceTraveled = 0f; // Reset distance for this new "phase" of flight.
                // This would make the speed curve restart from 0, potentially making the projectile fast again right after a bounce.
                // Or, keep tracking total distance but evaluate the curve differently.
                // It depends on your desired gameplay.
            }
            else
            {
                _targets.Add(hit.Target);
                BaseGameEntityComponent nextTarget = FindNearestTarget(hit.Position);
                if (nextTarget)
                {
                    _currentTarget = nextTarget;
                    Vector3 directionToTarget = (nextTarget.transform.position - hit.Position).normalized;
                    _cachedTransform.rotation = Quaternion.LookRotation(directionToTarget);
                }
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
                if (entity.transform == _cachedTransform) continue;
                if (Owner != null && entity == Owner) continue;
                if (entity == _currentTarget) continue; // Don't bounce back to same target
                if (_targets.Contains(entity)) continue; // already hit this
            
                // faction filtering 
                if (entity.GetEntitySide == Owner.GetEntitySide || entity.GetEntitySide == Side.Unassigned) continue;

                float distance = Vector3.Distance(searchPosition, entity.transform.position);

                if (!(distance < nearestDistance)) continue;
                nearestDistance = distance;
                nearestEntity = entity;
            }
            return nearestEntity;
        }

        protected override void Init(Transform projectileTransform)
        {
            base.Init(projectileTransform);
            _cachedTransform =  projectileTransform;
        }

        public override void Reset()
        {
            _cachedTransform = null;
            _currentTarget = null;
            _targets.Clear();
            base.Reset();
        }
    }
    
}