using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    [CreateAssetMenu(fileName = "New Basic Projectile Behavior", menuName = "Projectiles/Behavior/Basic", order = 0)]
    public class SerializedBasicProjectileBehavior : SerializedProjectileBehavior
    {
        public BaseProjectileSettings baseProjectileSettings;

        public override ProjectileBehavior Deserialize(BaseGameEntityComponent owner)
        {
            return new BaseProjectileBehavior(baseProjectileSettings, owner);
        }
    }

    /// <summary>
    /// Летит по прямой до столкновения (вид "a" из диздока). Разбивается о стену
    /// (см. environmentResponse) и исчезает, когда исчерпан лимит поражаемых целей.
    /// </summary>
    public class BaseProjectileBehavior : ProjectileBehavior
    {
        private const float DistanceEpsilon = 1e-4f;

        protected readonly BaseProjectileSettings _settings;
        private readonly float _flightTime;      // реальное время жизни с учётом кривой скорости
        float _distanceTraveled;
        protected bool init = false;
        private float _timeElapsed;

        protected float DistanceTraveled => _distanceTraveled;

        public BaseProjectileBehavior(BaseProjectileSettings settings, BaseGameEntityComponent owner)
        {
            _settings = settings;
            Owner = owner;
            _flightTime = ComputeFlightTime(settings);
        }

        /// <summary>
        /// Время, за которое интеграл (скорость * кривая) от 0 до T равен maxFlightDistance.
        /// Раньше время считалось как distance / baseSpeed, что верно только для константной
        /// кривой. Если кривая падает к нулю, снаряд не успевал долететь до maxFlightDistance
        /// и "зависал" в воздухе на нулевой скорости.
        /// </summary>
        private static float ComputeFlightTime(in BaseProjectileSettings s)
        {
            if (s.baseSpeed <= 0f) return 0f; // не движется -> сразу завершаем

            float average = 1f;
            if (s.speedCurve != null && s.speedCurve.length > 0)
            {
                const int steps = 64;
                float sum = 0f;
                for (int i = 0; i < steps; i++)
                    sum += Mathf.Max(0f, s.speedCurve.Evaluate((i + 0.5f) / steps));
                average = sum / steps;
            }

            average = Mathf.Max(average, 0.05f); // защита от кривой, равной нулю почти везде
            return s.maxFlightDistance / (s.baseSpeed * average);
        }

        public override ProjectileCollisionResult OnCollision(in ProjectileCollision c)
        {
            if (c.IsOwner) return ProjectileCollisionResult.Continue;

            if (c.IsEnvironment)
            {
                return _settings.environmentResponse == EnvironmentResponse.Destroy
                    ? ProjectileCollisionResult.Finish
                    : ProjectileCollisionResult.Continue;
            }

            // попадание по цели: снаряд живёт, пока не исчерпан лимит из ProjectileHitRules
            return c.BudgetExhausted ? ProjectileCollisionResult.Finish : ProjectileCollisionResult.Continue;
        }

        public sealed override void UpdatePosition(float delta, Transform projectileTransform)
        {
            if (BehaviorCompleted) return;
            if (!init) Init(projectileTransform);

            float distanceThisFrame = CalculateDistanceThisFrame(delta);

            RotateProjectile(distanceThisFrame, projectileTransform, delta);
            MoveForward(distanceThisFrame, projectileTransform);

            _timeElapsed += delta;
            CheckExpiry(distanceThisFrame);
        }

        private float CalculateDistanceThisFrame(float delta)
        {
            float normalizedTime = _flightTime > 0f ? Mathf.Clamp01(_timeElapsed / _flightTime) : 1f;
            float speedMultiplier = _settings.speedCurve != null && _settings.speedCurve.length > 0
                ? Mathf.Max(0f, _settings.speedCurve.Evaluate(normalizedTime))
                : 1f;

            float distance = _settings.baseSpeed * speedMultiplier * delta;

            // не пролетать дальше maxFlightDistance на последнем кадре
            float remaining = Mathf.Max(0f, _settings.maxFlightDistance - _distanceTraveled);
            return Mathf.Min(distance, remaining);
        }

        protected virtual void RotateProjectile(float distanceThisFrame, Transform projectileTransform, float deltaTime)
        {
            // basic projectile doesn't rotate
        }

        private void MoveForward(float distanceThisFrame, Transform projectileTransform)
        {
            projectileTransform.position += projectileTransform.forward * distanceThisFrame;
        }

        private void CheckExpiry(float distanceThisFrame)
        {
            _distanceTraveled += distanceThisFrame;

            // Два независимых условия: дистанция ИЛИ время. Второе гарантирует,
            // что снаряд не останется висеть, даже если кривая скорости "съела" путь.
            if (_distanceTraveled >= _settings.maxFlightDistance - DistanceEpsilon
                || _timeElapsed >= _flightTime)
            {
                OnDistanceExpiry();
            }
        }

        protected virtual void OnDistanceExpiry()
        {
            BehaviorCompleted = true;
        }

        protected virtual void Init(Transform projectileTransform)
        {
            init = true;
            _timeElapsed = 0f;
        }

        public override void Reset()
        {
            _distanceTraveled = 0f;
            _timeElapsed = 0f;
            init = false;
            BehaviorCompleted = false;
        }
    }
}
