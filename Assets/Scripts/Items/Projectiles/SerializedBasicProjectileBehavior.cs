using System;
using DG.Tweening;
using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    [CreateAssetMenu(fileName = "New Basic Projectile Behavior", menuName = "Projectiles/Behavior/Basic", order = 0)]
    public class SerializedBasicProjectileBehavior : SerializedProjectileBehavior
    {
        public BaseProjectileSettings baseProjectileSettings;

        public override ProjectileBehavior Deserialize(BaseGameEntityComponent owner)
        {
            return new BaseProjectileBehavior(baseProjectileSettings,owner);
        }
    }

    public class BaseProjectileBehavior : ProjectileBehavior
    {
        protected readonly BaseProjectileSettings _settings;
        protected float _distanceTraveled;
        protected Vector3 _startPosition;
        protected bool init = false;
        

        public BaseProjectileBehavior(BaseProjectileSettings settings,  BaseGameEntityComponent owner)
        {
            _settings = settings;
            Owner = owner;
        }

        public override void NotifyCollision(TriggerHitInfo hit)
        {
            // regular projectile does nothing
            // it is killed externally
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
                BehaviorCompleted = true;
            }
        }

        public override void Reset()
        {
            _distanceTraveled = 0f;
            _startPosition = Vector3.zero;
            init = false;
            BehaviorCompleted = false;
        }
    }

    [Serializable]
    public struct BaseProjectileSettings
    {
        public float maxFlightDistance;
        public float speedPerSecond;
    }
    
}