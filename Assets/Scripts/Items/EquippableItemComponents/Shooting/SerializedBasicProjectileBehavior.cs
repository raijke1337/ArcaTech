using System;
using DG.Tweening;
using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    [CreateAssetMenu(fileName = "New Homing Projectile Behavior", menuName = "Projectiles/Projectile/Behavior/Basic", order = 0)]
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
        
        protected Sequence _motionSequence;  // DOTween sequence for all behaviors (chains moves, easing, etc.)
        protected float _timeElapsed = 0f;  // Tracks time for manual control
        protected Vector3 _startPosition;  // Cache for distance calculations
        protected float _totalDist = 0f;  // Tracking moved distance
        
        bool intialized = false;
        public BaseProjectileBehavior(BaseProjectileSettings settings)
        {
            _settings = settings;
        }

        public override void UpdatePosition(float delta, Transform projectileTransform)
        {
            if (!intialized)
            {
                _startPosition =  projectileTransform.position;
                
                // Default: Simple linear move with falloff (for direct flight); subclasses can append/override
                Vector3 targetPos = projectileTransform.position + projectileTransform.forward * _settings.maxFlightDistance;
                _motionSequence.Append(
                    projectileTransform.DOMove(targetPos, _settings.maxFlightDistance / _settings.speedPerSecond)
                        .SetEase(Ease.Linear) // Basic; customizable in subclasses
                        .OnUpdate(() => UpdateDistAndFalloff(projectileTransform, _settings.speedPerSecond))); // Handle falloff if added
                
                intialized = true;
            }
            
            if (IsExpired) return;  // Skip if paused or done
        
            _timeElapsed += delta;
            // Manually advance tween by delta (DOTween handles easing/manifestation under the hood)
            _motionSequence.Goto(_timeElapsed);

            // Check expiry (sequence complete or max distance reached)
            if (_motionSequence.IsComplete() || _totalDist >= _settings.maxFlightDistance)
            {
                IsExpired = true;
                _motionSequence.Kill();  // Cleanup
            }
        }
        
        protected virtual void UpdateDistAndFalloff(Transform projectileTransform, float currentSpeed)
        {
            float distMoved = Vector3.Distance(_startPosition, projectileTransform.position);
            _totalDist = Mathf.Max(_totalDist, distMoved);

        }

        public override void Reset()
        {
            IsExpired = false;
            _timeElapsed = 0f;
            _totalDist = 0f;
            _motionSequence?.Rewind();  // Reset sequence
            _motionSequence?.Play();  // Ready for start
        }
    }

    [Serializable]
    public struct BaseProjectileSettings
    {
        public float maxFlightDistance;
        public float speedPerSecond;
    }
    
}