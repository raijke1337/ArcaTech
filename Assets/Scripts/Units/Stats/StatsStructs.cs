using System;
using Arcatech.Actions;
using UnityEngine;

namespace Arcatech.Stats
{
// Equipment and Effect data types


    public enum StatOpKind { Add, Mult } // Mult is interpreted as (1 + value) multiplier
    public enum StatTarget { Current, Max }

    [Serializable]
    public struct StatModifier
    {
        public ResourceStatType stat;
        public StatTarget target;
        public StatOpKind op;
        /// <summary>
        /// Add: +X; Mult: +m (use 0.10 for +10%)
        /// </summary>
        public float value;

        [Tooltip("Optional condition for this modifier to be active.")]
        public ConditionGroup condition;
    }
    [Serializable]
    public struct PeriodicDelta
    {
        public StatDelta delta;
        public float intervalSeconds; // e.g., 1.0 for +1 per second
        [Tooltip("Optional condition to apply this delta when it ticks.")]
        public ConditionGroup condition;
    }
    [Serializable]
    public struct StatDelta // an instantaneous or per-tick delta to Current or Max
    {
        public ResourceStatType stat;
        public StatTarget target;
        public float amount; // Negative for damage, positive for healing/gain
        public SerializedProduceFXResult onApply;
    }
}