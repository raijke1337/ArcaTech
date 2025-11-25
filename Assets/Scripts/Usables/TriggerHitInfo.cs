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
            Vector3 hitPosition, float time)
        {
            Source = triggerNotificationProvider;
            Target = baseGameEntityComponent;
            Position = hitPosition;
            Timestamp = time;
        }

        public bool IsValidHit => Target;
        public ITriggerNotificationProvider Source { get; }
        public BaseGameEntityComponent Target { get; } // The hit target (can be null for environmental effects)
        public Vector3 Position { get; } // Where the hit occurred
        public float Timestamp { get; } // Time of hit (for sequencing)
    }
}