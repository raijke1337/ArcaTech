using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    [CreateAssetMenu(fileName = "projectileBehavior_", menuName = "Projectiles/Behavior/Boomerang")]
    public class SerializedBoomerangProjectileBehavior : SerializedBasicProjectileBehavior
    {
        [Tooltip("How aggressively the projectile turns back toward the owner once returning.")]
        public float returnHomingStrength = 8f;
        
        public override ProjectileBehavior Deserialize(BaseGameEntityComponent owner)
        {
            return new BoomerangProjectileBehavior(this, baseProjectileSettings, owner);
        }
    }

    public class BoomerangProjectileBehavior : BaseProjectileBehavior
    {
        private readonly float _returnHomingStrength;
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

        protected override void RotateProjectile(float distanceThisFrame, Transform projectileTransform, float deltaTime)
        {
            if (!_initDirection)
            {
                Vector3 launchDirection = GetLaunchDirection();
                projectileTransform.rotation = Quaternion.LookRotation(launchDirection);
                _initDirection = true;
                _cachedTransform = projectileTransform;
            }

            if (_returning)
            {
                if (DistanceTraveled >= _settings.maxFlightDistance * 2f)
                {
                    BehaviorCompleted = true;
                }
                if (Owner)
                {
                    // HOMING: Calculate direction to owner with homing strength
                    Vector3 directionToOwner = (Owner.EffectSpawn.position - projectileTransform.position).normalized;
                    Vector3 newDirection = Vector3.Slerp(projectileTransform.forward, directionToOwner,
                        _returnHomingStrength * deltaTime);
                    projectileTransform.rotation = Quaternion.LookRotation(newDirection);
                }
            }

            else
            {
                float halfDistance = _settings.maxFlightDistance * 0.5f;
                if (DistanceTraveled >= halfDistance)
                {
                    BeginReturnPhase(projectileTransform);
                }
            }
        }

        protected override void Init(Transform projectileTransform)
        {
            base.Init(projectileTransform);
            _returning = false;
            _initDirection = false;
            // NEW: Set curve direction perpendicular to launch (use world up for simplicity)
            _curvePerpendicular = Vector3.Cross(GetLaunchDirection(), Vector3.up).normalized;
            // Fallback if cross product is zero (rare)
            if (_curvePerpendicular.sqrMagnitude < 0.001f)
                _curvePerpendicular = Vector3.right;
        }
        
        private void BeginReturnPhase(Transform projectileTransform)
        {
            if (_returning) return;
            Debug.Log(projectileTransform);
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
            if (!hit.TargetCollider.TryGetComponent(out BaseGameEntityComponent entity)) return;
            if (entity == Owner && _returning)
            {
                BehaviorCompleted = true;
                return;
            }
            if (entity != Owner)
                BeginReturnPhase(_cachedTransform);
        }
        

        public override void Reset()
        {
            _returning = false;
            _initDirection = false;
            base.Reset();
        }
    }
}