using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    [CreateAssetMenu(fileName = "projectileBehavior_", menuName = "Projectiles/Behavior/Orbiting")]
    public class SerializedOrbitingProjectileBehavior : SerializedBasicProjectileBehavior
    {


        [Min(0.1f)] public float orbitRadius = 3f;

        [Tooltip("Degrees per second the projectile sweeps around its owner.")]
        public float angularSpeed = 120f;

        public Vector3 orbitAxis = Vector3.up;
        public bool clockwise = true;


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

        private Vector3 _orbitOffset;
        public OrbitingProjectileBehavior(SerializedOrbitingProjectileBehavior serialized,
            BaseProjectileSettings settings, BaseGameEntityComponent owner)
            : base(settings, owner)
        {
            _orbitRadius = serialized.orbitRadius;
            _orbitAxis = serialized.orbitAxis.sqrMagnitude <= 0f ? Vector3.up : serialized.orbitAxis.normalized;
            _clockwise = serialized.clockwise;
        }

        protected override void Init(Transform projectileTransform)
        {
            base.Init(projectileTransform);
            _orbitOffset = projectileTransform.position - Owner.transform.position;
            _orbitOffset = Vector3.ProjectOnPlane(_orbitOffset, _orbitAxis);

            if (_orbitOffset == Vector3.zero)
            {
                _orbitOffset = Vector3.forward;
            }

            _orbitOffset = _orbitOffset.normalized * _orbitRadius;
        }

        protected override void RotateProjectile(float distanceThisFrame, Transform projectileTransform, float deltaTime)
        {
            
            // rotate 
            float linearSpeed = _settings.baseSpeed;
            float angularSpeedRad = linearSpeed / _orbitRadius;
            float deltaAngleDegrees = angularSpeedRad * deltaTime * Mathf.Rad2Deg;
            float directionMultiplier = _clockwise ? -1f : 1f;

            Quaternion rotation = Quaternion.AngleAxis(deltaAngleDegrees * directionMultiplier, _orbitAxis);
            _orbitOffset = rotation * _orbitOffset;
            
            projectileTransform.position = Owner.EffectSpawn.transform.position + _orbitOffset;

            Vector3 tangent = Vector3.Cross(_orbitAxis, _orbitOffset).normalized;
            if (tangent != Vector3.zero)
            {
                projectileTransform.rotation = Quaternion.LookRotation(tangent, _orbitAxis);
            }
        }
        
        public override void Reset()
        {
            _orbitOffset = Vector3.zero;
            base.Reset();
        }
    }
}