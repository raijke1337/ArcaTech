using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "projectileBehavior_", menuName = "Projectiles/Behavior/Homing")]
    public class SerializedHomingProjectileBehavior : SerializedBasicProjectileBehavior
    {
        [Min(0.1f)] public float scanRadius = 5f;
        [Range(0.01f, 2f)] public float scanInterval = 0.25f;

        [Tooltip("How strongly the projectile rotates toward the locked target each frame.")]
        public float homingStrength = 10f;

        public LayerMask targetLayers;

        public override ProjectileBehavior Deserialize(BaseGameEntityComponent owner)
        {
            return new HomingProjectileBehavior(this, baseProjectileSettings, owner);
        }
    }

    public class HomingProjectileBehavior : BaseProjectileBehavior
    {
        private readonly float _scanRadius;
        private readonly float _scanInterval;
        private readonly float _homingStrength;
        private readonly LayerMask _targetLayers;

        private float _timeSinceLastScan;
        private BaseGameEntityComponent _currentTarget;

        public HomingProjectileBehavior(SerializedHomingProjectileBehavior serialized, BaseProjectileSettings settings,
            BaseGameEntityComponent owner)
            : base(settings, owner)
        {
            _scanRadius = serialized.scanRadius;
            _scanInterval = Mathf.Max(0.01f, serialized.scanInterval);
            _homingStrength = Mathf.Max(0f, serialized.homingStrength);
            _targetLayers = serialized.targetLayers;
        }

        protected override void RotateProjectile(float distanceThisFrame, Transform projectileTransform, float deltaTime)
        {

           if (_currentTarget == null || !_currentTarget.gameObject.activeInHierarchy)
           {
               TryAcquireTarget(projectileTransform);
           }
           
           else if (_homingStrength > 0f)
           {
               Vector3 directionToTarget =
                   (_currentTarget.transform.position - projectileTransform.position).normalized;
               Vector3 newDirection =
                   Vector3.Slerp(projectileTransform.forward, directionToTarget, _homingStrength * deltaTime);
               projectileTransform.rotation = Quaternion.LookRotation(newDirection);
           }
        }

        protected override void Init(Transform projectileTransform)
        {
            base.Init(projectileTransform);
            _timeSinceLastScan = _scanInterval;
        }

        private void TryAcquireTarget(Transform projectileTransform)
        {
            if (_timeSinceLastScan < _scanInterval)
                return;

            _timeSinceLastScan = 0f;
            Collider[] colliders = Physics.OverlapSphere(projectileTransform.position, _scanRadius, _targetLayers);

            BaseGameEntityComponent bestTarget = null;
            float bestDistance = float.MaxValue;

            foreach (var collider in colliders)
            {
                BaseGameEntityComponent candidate = collider.GetComponent<BaseGameEntityComponent>();
                if (candidate == null || candidate == Owner)
                    continue;

                if (Owner != null && candidate.GetEntitySide == Owner.GetEntitySide || candidate.GetEntitySide == Side.Unassigned)
                    continue;

                if (candidate.transform == projectileTransform) continue;

                float distance = Vector3.Distance(projectileTransform.position, candidate.transform.position);
                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                bestTarget = candidate;
            }

            if (bestTarget != null)
            {
                _currentTarget = bestTarget;
            }
        }

        public override void NotifyCollision(TriggerHitInfo hit)
        {
            if (hit.Target == null)
                return;

            if (hit.Target == Owner)
            {
                BehaviorCompleted = true;
                return;
            }

            if (_currentTarget == hit.Target)
            {
                _currentTarget = null;
            }
        }

        public override void Reset()
        {
            _currentTarget = null;
            _timeSinceLastScan = 0f;
            base.Reset();
        }
    }
}