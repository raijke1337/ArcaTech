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

    public class BaseProjectileBehavior : ProjectileBehavior
    {
        protected readonly BaseProjectileSettings _settings;
        float _distanceTraveled;
        protected bool init = false;
        private float _timeElapsed;

        protected float DistanceTraveled => _distanceTraveled;
        public BaseProjectileBehavior(BaseProjectileSettings settings, BaseGameEntityComponent owner)
        {
            _settings = settings;
            Owner = owner;
        }

        public override void NotifyCollision(TriggerHitInfo hit)
        {
            // regular projectile does nothing
            // it is killed externally
        }

        public sealed override void UpdatePosition(float delta, Transform projectileTransform)
        {
            if (!init) Init(projectileTransform);
            
            float distanceThisFrame = CalculateDistanceThisFrame(delta);
            
            RotateProjectile(distanceThisFrame, projectileTransform, delta);
            MoveForward(distanceThisFrame, projectileTransform);
            CheckDistanceExpiry(distanceThisFrame);

            _timeElapsed += delta;
        }

        private float CalculateDistanceThisFrame(float delta)
        {

            float normalizedTime = Mathf.Clamp01(_timeElapsed / _settings.MaxFlightTime);
            float speedMultiplier = _settings.speedCurve.Evaluate(normalizedTime);
            float currentSpeed = _settings.baseSpeed * speedMultiplier;

            return currentSpeed * delta;
        }
        protected virtual void RotateProjectile(float distanceThisFrame, Transform projectileTransform, float deltaTime)
        {
            // basic projectile doesn't rotate
        }

        private void MoveForward(float distanceThisFrame, Transform projectileTransform)
        {
            projectileTransform.position += projectileTransform.forward * distanceThisFrame;
        }

        private void CheckDistanceExpiry(float distanceThisFrame)
        {
            // Track total distance traveled
            _distanceTraveled += distanceThisFrame;

            // Check if projectile has exceeded max flight distance
            if (_distanceTraveled >= _settings.maxFlightDistance)
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