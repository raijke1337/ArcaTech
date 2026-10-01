using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    [CreateAssetMenu(fileName = "projectileBehavior_", menuName = "Projectiles/Behavior/Bouncing")]
    public class SerializedBouncyProjectileBehavior : SerializedBasicProjectileBehavior
    {
        [Min(1)] public float targetSearchRadius;

        [Min(0), Tooltip("Сколько раз снаряд может отскочить (от цели или стены). После этого следующее столкновение его уничтожает.")]
        public int maxBounces = 3;

        [Header("Homing (Optional)")]
        [Range(0f, 10f)]
        [Tooltip("How strongly projectile tracks toward target between bounces. 0 = straight line, higher = tighter tracking")]
        public float homingStrength;
        public override ProjectileBehavior Deserialize(BaseGameEntityComponent owner)
        {
            return new BouncyProjectileBehavior(this, baseProjectileSettings, owner);
        }
    }

    /// <summary>
    /// Отскоки (вид "g" из диздока): после столкновения с целью летит к ближайшей валидной цели,
    /// если целей нет - отражается зеркально. Стена - всегда зеркальное отражение.
    /// </summary>
    public class BouncyProjectileBehavior : BaseProjectileBehavior
    {
        private const float BounceNudge = 1f; // отступ от точки удара, чтобы не задеть ту же поверхность

        private readonly float _rad;
        private readonly int _maxBounces;
        private readonly float _homingStrength;
        private BaseGameEntityComponent _currentTarget;
        private int _bounces;

        private readonly List<BaseGameEntityComponent> _targets;
        private readonly Collider[] _searchBuffer = new Collider[32];

        public BouncyProjectileBehavior(SerializedBouncyProjectileBehavior b, BaseProjectileSettings settings,BaseGameEntityComponent owner) : base(settings,owner)
        {
            _rad = b.targetSearchRadius;
            _maxBounces = Mathf.Max(0, b.maxBounces);
            _homingStrength = b.homingStrength;
            _targets  = new List<BaseGameEntityComponent>();
        }

        protected override void RotateProjectile(float distanceThisFrame, Transform projectileTransform, float deltaTime)
        {
            if (_currentTarget && !_currentTarget.gameObject.activeInHierarchy) _currentTarget = null;

            if (_currentTarget && _homingStrength > 0f)
            {
                Vector3 directionToTarget = (_currentTarget.transform.position - projectileTransform.position).normalized;
                Vector3 currentForward = projectileTransform.forward;

                // Smoothly interpolate toward target
                Vector3 newDirection = Vector3.Slerp(currentForward, directionToTarget, _homingStrength * deltaTime);
                projectileTransform.rotation = Quaternion.LookRotation(newDirection);
            }
        }

        public override ProjectileCollisionResult OnCollision(in ProjectileCollision c)
        {
            if (c.IsOwner) return ProjectileCollisionResult.Continue;

            if (c.IsEnvironment)
                return Bounce(c, null);

            // повторное касание уже поражённой цели / лимит исчерпан ранее - не отскакиваем
            if (!c.Accepted) return ProjectileCollisionResult.Continue;

            _targets.Add(c.Target);

            // последнее из разрешённых правилами попадание
            if (c.BudgetExhausted) return ProjectileCollisionResult.Finish;

            return Bounce(c, c.Target);
        }

        private ProjectileCollisionResult Bounce(in ProjectileCollision c, BaseGameEntityComponent hitTarget)
        {
            if (_bounces >= _maxBounces) return ProjectileCollisionResult.Finish;
            _bounces++;

            Transform t = c.Projectile;

            if (hitTarget != null)
            {
                // отскок от цели: к ближайшей валидной цели
                BaseGameEntityComponent next = FindNearestTarget(c.Hit.Position, t);
                if (next)
                {
                    _currentTarget = next;
                    Vector3 toNext = (next.transform.position - c.Hit.Position).normalized;
                    if (toNext.sqrMagnitude > 0.0001f)
                    {
                        t.rotation = Quaternion.LookRotation(toNext);
                        return ProjectileCollisionResult.Continue;
                    }
                }
            }

            // нет цели (или стена): зеркальное отражение
            _currentTarget = null;
            Vector3 reflected = Vector3.Reflect(c.Hit.ImpactDirection, c.Hit.Normal);

            if (hitTarget == null)
            {
                // небольшой разброс на стенах, чтобы рикошет выглядел естественнее
                reflected = Quaternion.AngleAxis(Random.Range(-5f, 5f), Vector3.up) * reflected;
            }

            if (reflected.sqrMagnitude < 0.0001f) reflected = -t.forward;
            reflected.Normalize();

            t.rotation = Quaternion.LookRotation(reflected);
            t.position = c.Hit.Position + reflected * BounceNudge;

            return ProjectileCollisionResult.Continue;
        }

        private BaseGameEntityComponent FindNearestTarget(Vector3 searchPosition, Transform projectileTransform)
        {
            int count = Physics.OverlapSphereNonAlloc(searchPosition, _rad, _searchBuffer);

            BaseGameEntityComponent nearestEntity = null;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                BaseGameEntityComponent entity = _searchBuffer[i].GetComponent<BaseGameEntityComponent>();

                // Skip invalid targets
                if (entity == null) continue;
                if (entity.transform == projectileTransform) continue;
                if (Owner != null && entity == Owner) continue;
                if (_targets.Contains(entity)) continue; // already hit this (в том числе только что поражённая цель)

                // faction filtering
                if (entity.GetEntitySide == Side.Unassigned) continue;
                if (Owner != null && entity.GetEntitySide == Owner.GetEntitySide) continue;

                float distance = Vector3.Distance(searchPosition, entity.transform.position);

                if (!(distance < nearestDistance)) continue;
                nearestDistance = distance;
                nearestEntity = entity;
            }
            return nearestEntity;
        }

        public override void Reset()
        {
            _currentTarget = null;
            _bounces = 0;
            _targets.Clear();
            base.Reset();
        }
    }

}
