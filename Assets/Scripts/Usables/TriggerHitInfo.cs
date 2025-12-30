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
            [CanBeNull] BaseGameEntityComponent baseGameEntityComponent,
            Vector3 hitPosition, Vector3 impactDirection, Vector3 hitNormal,
            float time) // Added impactDir and hitNormal
        {
            Source = triggerNotificationProvider;
            Target = baseGameEntityComponent;
            Position = hitPosition;
            ImpactDirection = impactDirection; // From the incoming projectile
            Normal = hitNormal; // Normal of the surface hit
            Time = time;
        }

        public bool IsValidHit => Target != null; // Changed to null check for clarity
        public ITriggerNotificationProvider Source { get; }

        /// <summary>
        /// The hit entity, can be null for environment hits
        /// </summary>
        [CanBeNull]
        public BaseGameEntityComponent Target { get; }

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