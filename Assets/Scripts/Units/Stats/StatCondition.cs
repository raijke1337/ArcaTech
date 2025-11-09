using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Stats
{
    [Serializable]
    public struct StatCondition
    {
        public ResourceStatType stat;
        public StatTarget target;       // Usually Current for threshold checks
        public bool usePercentOfMax;    // If true, compares normalized ratio (0..1) instead of absolute units
        public ConditionOp op;
        public float a;
        public float b;                 // Used when op == Between
    }

    [Serializable]
    public struct ConditionGroup
    {
        [Tooltip("If true: all conditions must pass (AND). If false: any condition may pass (OR).")]
        public bool requireAll;

        [Tooltip("Optional inversion of the group result.")]
        public bool invert;

        public List<StatCondition> statConditions;

        public bool IsEmpty => statConditions == null || statConditions.Count == 0;
    }
    
    public enum ConditionOp
    {
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual,
        Between, // inclusive [a, b]
        Equal,
        NotEqual
    }
    
}