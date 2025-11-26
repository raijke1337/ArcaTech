using System;
using DG.Tweening;
using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    [CreateAssetMenu(fileName = "New Basic Projectile Behavior", menuName = "Projectiles/Behavior/Basic", order = 0)]
    public class SerializedBasicProjectileBehavior : SerializedProjectileBehavior
    {
        public BaseProjectileSettings baseProjectileSettings;
        public override ProjectileBehavior Deserialize()
        {
            return new BaseProjectileBehavior(baseProjectileSettings);
        }
    }

    public class BaseProjectileBehavior : ProjectileBehavior
    {
        readonly BaseProjectileSettings _settings;

        private float _distanceTraveled;
        private Vector3 _startPosition;
        bool init = false;

        public BaseProjectileBehavior(BaseProjectileSettings settings)
        {
            _settings = settings;
        }

        public override void NotifyCollision(Collider collider)
        {
            //
        }
        public override void UpdatePosition(float delta, Transform projectileTransform)
        {
            if (!init)
            {
                init = true;
                _startPosition = projectileTransform.position;
            }
            // Calculate distance to travel this frame
            float distanceThisFrame = _settings.speedPerSecond * delta;

            // Move the projectile forward along its current direction
            projectileTransform.position += projectileTransform.forward * distanceThisFrame;

            // Track total distance traveled
            _distanceTraveled += distanceThisFrame;

            // Check if projectile has exceeded max flight distance
            if (_distanceTraveled >= _settings.maxFlightDistance)
            {
                IsExpired = true;
            }
        }

        public override void Reset()
        {
            _distanceTraveled = 0f;
            _startPosition = Vector3.zero;
            init = false;
            IsExpired = false;
        }
    }

    [Serializable]
    public struct BaseProjectileSettings
    {
        public float maxFlightDistance;
        public float speedPerSecond;
    }
    
}