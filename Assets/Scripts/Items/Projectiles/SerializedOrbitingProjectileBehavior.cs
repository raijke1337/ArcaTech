using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    [CreateAssetMenu(fileName = "New Orbiting Projectile Behavior", menuName = "Projectiles/Behavior/Orbiting")]
    public class SerializedOrbitingProjectileBehavior : SerializedBasicProjectileBehavior
    {


        [Min(0.1f)] public float orbitRadius = 3f;

        [Tooltip("Degrees per second the projectile sweeps around its owner.")]
        public float angularSpeed = 120f;

        public Vector3 orbitAxis = Vector3.up;
        public bool clockwise = true;

        [Tooltip("If true, keep the projectile level with the owner instead of preserving its height.If false, the Effects Spawn transform will be used")]
        public bool matchOwnerHeight = true;

        public override ProjectileBehavior Deserialize(BaseGameEntityComponent owner)
        {
            return new OrbitingProjectileBehavior(this, baseProjectileSettings, owner);
        }
    }


    public class OrbitingProjectileBehavior : BaseProjectileBehavior
    {
        private readonly float _orbitRadius;
        private readonly Vector3 _orbitAxis;
        private readonly bool _clockwise;
        private readonly bool _matchOwnerHeight;

        private Transform _cachedTransform;
        private Vector3 _orbitOffset;
        private bool _initialized;

        public OrbitingProjectileBehavior(SerializedOrbitingProjectileBehavior serialized,
            BaseProjectileSettings settings, BaseGameEntityComponent owner)
            : base(settings, owner)
        {
            _orbitRadius = serialized.orbitRadius;
            _orbitAxis = serialized.orbitAxis.sqrMagnitude <= 0f ? Vector3.up : serialized.orbitAxis.normalized;
            _clockwise = serialized.clockwise;
            _matchOwnerHeight = serialized.matchOwnerHeight;
        }

        public override void UpdatePosition(float delta, Transform projectileTransform)
        {
            if (!init)
            {
                init = true;
                _cachedTransform = projectileTransform;
                _initialized = false;
            }

            Vector3 ownerPosition = Owner ? Owner.transform.position : projectileTransform.position;

            if (!_initialized)
            {
                _orbitOffset = projectileTransform.position - ownerPosition;
                _orbitOffset = Vector3.ProjectOnPlane(_orbitOffset, _orbitAxis);

                if (_orbitOffset == Vector3.zero)
                {
                    _orbitOffset = Vector3.forward;
                }

                _orbitOffset = _orbitOffset.normalized * _orbitRadius;
                _initialized = true;
            }

            float linearSpeed = _settings.speedPerSecond;
            float angularSpeedRad = linearSpeed / _orbitRadius;
            float deltaAngleDegrees = angularSpeedRad * delta * Mathf.Rad2Deg;
            float directionMultiplier = _clockwise ? -1f : 1f;

            Quaternion rotation = Quaternion.AngleAxis(deltaAngleDegrees * directionMultiplier, _orbitAxis);
            _orbitOffset = rotation * _orbitOffset;

            if (_matchOwnerHeight && Owner)
            {
                ownerPosition.y = Owner.transform.position.y;
                
            }
            else
            {
                ownerPosition.y = Owner.EffectSpawn.transform.position.y;
            }

            projectileTransform.position = ownerPosition + _orbitOffset;

            Vector3 tangent = Vector3.Cross(_orbitAxis, _orbitOffset).normalized;
            if (tangent != Vector3.zero)
            {
                projectileTransform.rotation = Quaternion.LookRotation(tangent, _orbitAxis);
            }

            _distanceTraveled += linearSpeed * delta;

            if (_distanceTraveled >= _settings.maxFlightDistance)
            {
                BehaviorCompleted = true;
            }
        }

        public override void Reset()
        {
            _distanceTraveled = 0f;
            _initialized = false;
            init = false;
            BehaviorCompleted = false;
            _orbitOffset = Vector3.zero;
        }
    }
}