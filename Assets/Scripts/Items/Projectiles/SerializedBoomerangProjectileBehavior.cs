using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    [CreateAssetMenu(fileName = "New Boomerang Projectile Behavior", menuName = "Projectiles/Behavior/Boomerang")]
    public class SerializedBoomerangProjectileBehavior : SerializedBasicProjectileBehavior
    {
        [Tooltip("How aggressively the projectile turns back toward the owner once returning.")]
        public float returnHomingStrength = 8f;
        public float curveAmplitude = 1f;
        public float curveFrequency = 2f * Mathf.PI;
        
        public override ProjectileBehavior Deserialize(BaseGameEntityComponent owner)
        {
            return new BoomerangProjectileBehavior(this, baseProjectileSettings, owner);
        }
    }

    public class BoomerangProjectileBehavior : BaseProjectileBehavior
    {
        private readonly float _returnHomingStrength;
        private readonly float _curveAmplitude;
        private readonly float _curveFrequency;
        private Transform _cachedTransform;
        private bool _returning;
        private bool _initDirection;
        private Vector3 _initialDirection;
        private Vector3 _curvePerpendicular; // Perpendicular vector for oscillation (e.g., up in world space)

        public BoomerangProjectileBehavior(SerializedBoomerangProjectileBehavior serialized,
            BaseProjectileSettings settings, BaseGameEntityComponent owner)
            : base(settings, owner)
        {
            _returnHomingStrength = serialized.returnHomingStrength;
            _curveAmplitude = serialized.curveAmplitude;
            _curveFrequency = serialized.curveFrequency;
        }


        private Vector3 GetLaunchDirection()
        {
            if (Owner == null) return Vector3.forward;

            Vector3 launchDir = Vector3.ProjectOnPlane(Owner.transform.forward, Vector3.up);
            if (launchDir.sqrMagnitude < 0.001f)
            {
                launchDir = Owner.transform.forward;
            }

            return launchDir.normalized;
        }

        public override void UpdatePosition(float delta, Transform projectileTransform)
        {
            if (!init)
            {
                init = true;
                _cachedTransform = projectileTransform;
                _returning = false;
                _initDirection = false;
                // NEW: Set curve direction perpendicular to launch (use world up for simplicity)
                _curvePerpendicular = Vector3.Cross(GetLaunchDirection(), Vector3.up).normalized;
                // Fallback if cross product is zero (rare)
                if (_curvePerpendicular.sqrMagnitude < 0.001f)
                    _curvePerpendicular = Vector3.right;
            }

            if (!_initDirection)
            {
                Vector3 launchDirection = GetLaunchDirection();
                projectileTransform.rotation = Quaternion.LookRotation(launchDirection);
                _initDirection = true;
            }

            float distanceThisFrame = _settings.speedPerSecond * delta;

            if (!_returning)
            {
                _distanceTraveled += distanceThisFrame;
                float halfDistance = _settings.maxFlightDistance * 0.5f;

                if (_distanceTraveled >= halfDistance)
                {
                    BeginReturnPhase(projectileTransform);
                }
                else
                {
                    // MOVEMENT: Calculate straight forward displacement
                    Vector3 forwardDisplacement = projectileTransform.forward * distanceThisFrame;

                    // NEW: Add oscillating curve (sine wave based on distance traveled)
                    float curveOffset = Mathf.Sin(_distanceTraveled * _curveFrequency) * _curveAmplitude;
                    Vector3 curveDisplacement = _curvePerpendicular * curveOffset * delta; // Scaled for smoothness

                    // Apply total displacement
                    projectileTransform.position += forwardDisplacement + curveDisplacement;
                    
                    return;
                }
            }

            if (_returning && Owner)
            {
                // HOMING: Calculate direction to owner with homing strength
                Vector3 directionToOwner = (Owner.transform.position - projectileTransform.position).normalized;
                Vector3 newDirection = Vector3.Slerp(projectileTransform.forward, directionToOwner,
                    _returnHomingStrength * delta);
                projectileTransform.rotation = Quaternion.LookRotation(newDirection);

                // MOVEMENT: Forward displacement along new direction
                Vector3 forwardDisplacement = projectileTransform.forward * distanceThisFrame;

                // NEW: Add oscillating curve (continue from outward, reset on begin return if desired)
                float curveOffset = Mathf.Sin(_distanceTraveled * _curveFrequency) * _curveAmplitude;
                Vector3 curveDisplacement = _curvePerpendicular * curveOffset * delta;

                // Apply total displacement
                projectileTransform.position += forwardDisplacement + curveDisplacement;

                _distanceTraveled += distanceThisFrame;
            }

            if (_distanceTraveled >= _settings.maxFlightDistance && !_returning)
            {
                BeginReturnPhase(projectileTransform);
            }

            if (_distanceTraveled >= _settings.maxFlightDistance * 2f)
            {
                BehaviorCompleted = true;
            }
        }

        private void BeginReturnPhase(Transform projectileTransform)
        {
            if (_returning) return;

            _returning = true;
            // _distanceTraveled = 0f;  // Keep for curve continuity, or reset if you want a fresh wave
            if (Owner)
            {
                Vector3 directionToOwner = (Owner.transform.position - projectileTransform.position).normalized;
                projectileTransform.rotation = Quaternion.LookRotation(directionToOwner);
            }
        }
    

        public override void NotifyCollision(TriggerHitInfo hit)
        {
            if (!hit.IsValidHit) return;
            if (hit.Target == Owner && _returning)
            {
                BehaviorCompleted = true;
                return;
            }
            if (hit.IsValidHit && hit.Target != Owner)
                BeginReturnPhase(_cachedTransform);
        }
        

        public override void Reset()
        {
            _distanceTraveled = 0f;
            _returning = false;
            _initDirection = false;
            init = false;
            BehaviorCompleted = false;
        }
    }
}