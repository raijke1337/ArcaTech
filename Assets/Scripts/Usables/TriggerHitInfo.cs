using System;
using Arcatech.Triggers;
using JetBrains.Annotations;
using UnityEngine;

namespace Arcatech
{
    [Serializable]
    public struct TriggerHitInfo
    {
        public TriggerHitInfo(ITriggerNotificationProvider triggerNotificationProvider,
            Collider hit,
            Vector3 hitPosition, Vector3 impactDirection, Vector3 hitNormal,
            float time) // Added impactDir and hitNormal
        {
            Source = triggerNotificationProvider;
            TargetCollider = hit;
            Position = hitPosition;
            ImpactDirection = impactDirection; // From the incoming projectile
            Normal = hitNormal; // Normal of the surface hit
            Time = time;
            TargetCollider.TryGetComponent(out _targetEntity);
        }

        private BaseGameEntityComponent _targetEntity;

        /// <summary>
        /// helper method to avoid endless TryGetComponent()s
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool TryGetEntityTarget(out BaseGameEntityComponent entity)
        {
            entity = _targetEntity;
            return entity != null;
        }
        public ITriggerNotificationProvider Source { get; }

        /// <summary>
        /// The hit collider. 
        /// </summary>
        public Collider TargetCollider { get; }

        /// <summary>
        /// The  point in world space where the hit occurred.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// The direction of the incoming projectile at the moment of impact.
        /// </summary>
        public Vector3 ImpactDirection { get; }

        /// <summary>
        /// The normal vector of the surface that was hit.
        /// </summary>
        public Vector3 Normal { get; }

        /// <summary>
        /// The Unity Time.time when the hit occurred.
        /// </summary>
        public float Time { get; }
    }
}