using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    [CreateAssetMenu(fileName = "New Boomerang Projectile Behavior", menuName = "Projectiles/Behavior/Boomerang")]
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

        public override void UpdatePosition(float delta, Transform projectileTransform)
        {
            if (!init)
            {
                init = true;
                _cachedTransform = projectileTransform;
                _returning = false;
                _initDirection = false;
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
                    projectileTransform.position += projectileTransform.forward * distanceThisFrame;
                    return;
                }
            }

            if (_returning && Owner)
            {
                Vector3 directionToOwner = (Owner.transform.position - projectileTransform.position).normalized;
                Vector3 newDirection = Vector3.Slerp(projectileTransform.forward, directionToOwner,
                    _returnHomingStrength * delta);
                projectileTransform.rotation = Quaternion.LookRotation(newDirection);
            }

            projectileTransform.position += projectileTransform.forward * distanceThisFrame;
            _distanceTraveled += distanceThisFrame;

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
            _distanceTraveled = 0f;
            if (Owner)
            {
                Vector3 directionToOwner = (Owner.transform.position - projectileTransform.position).normalized;
                projectileTransform.rotation = Quaternion.LookRotation(directionToOwner);
            }
        }

        public override void NotifyCollision(TriggerHitInfo hit)
        {
            if (hit.Target == null) return;

            if (hit.Target == Owner)
            {
                BehaviorCompleted = true;
                return;
            }

            if (!IsEnemy(hit.Target))
            {
                return;
            }

            BeginReturnPhase(_cachedTransform);
        }

        private bool IsEnemy(BaseGameEntityComponent target)
        {
            if (Owner == null || target == null) return false;
            return target.GetEntitySide != Owner.GetEntitySide;
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