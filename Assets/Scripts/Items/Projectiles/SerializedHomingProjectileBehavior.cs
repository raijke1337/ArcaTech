namespace Arcatech.Items.Projectiles
{
    using System.Collections.Generic;
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

    /// <summary>
    /// Слабое наведение (вид "d" из диздока): летит вперёд и доворачивает к цели в радиусе захвата.
    /// </summary>
    public class HomingProjectileBehavior : BaseProjectileBehavior
    {
        private readonly float _scanRadius;
        private readonly float _scanInterval;
        private readonly float _homingStrength;
        private readonly LayerMask _targetLayers;

        private float _timeSinceLastScan;
        private BaseGameEntityComponent _currentTarget;

        // уже поражённые цели - не захватываем повторно, иначе снаряд будет кружить вокруг них
        private readonly List<BaseGameEntityComponent> _hitTargets = new();
        private readonly Collider[] _scanBuffer = new Collider[32];

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
            // FIX: раньше таймер сканирования никогда не увеличивался, и если первый скан
            // не находил цель, повторного скана не происходило вообще.
            _timeSinceLastScan += deltaTime;

            if (_currentTarget == null || !_currentTarget.gameObject.activeInHierarchy)
            {
                _currentTarget = null;
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
            int count = Physics.OverlapSphereNonAlloc(projectileTransform.position, _scanRadius, _scanBuffer, _targetLayers);

            BaseGameEntityComponent bestTarget = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                BaseGameEntityComponent candidate = _scanBuffer[i].GetComponent<BaseGameEntityComponent>();
                if (candidate == null || candidate == Owner)
                    continue;

                if (candidate.GetEntitySide == Side.Unassigned)
                    continue;
                if (Owner != null && candidate.GetEntitySide == Owner.GetEntitySide)
                    continue;

                if (candidate.transform == projectileTransform) continue;
                if (_hitTargets.Contains(candidate)) continue;

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

        public override ProjectileCollisionResult OnCollision(in ProjectileCollision c)
        {
            // FIX: раньше здесь сравнивался Collider с BaseGameEntityComponent (всегда false),
            // поэтому реакция на цели и владельца не работала.
            if (c.IsOwner) return ProjectileCollisionResult.Continue;

            if (c.IsEnvironment)
            {
                return _settings.environmentResponse == EnvironmentResponse.Destroy
                    ? ProjectileCollisionResult.Finish
                    : ProjectileCollisionResult.Continue;
            }

            if (c.Target != null)
            {
                if (!_hitTargets.Contains(c.Target)) _hitTargets.Add(c.Target);
                if (_currentTarget == c.Target) _currentTarget = null;
            }

            return c.BudgetExhausted ? ProjectileCollisionResult.Finish : ProjectileCollisionResult.Continue;
        }

        public override void Reset()
        {
            _currentTarget = null;
            _timeSinceLastScan = 0f;
            _hitTargets.Clear();
            base.Reset();
        }
    }
}
