﻿
using Arcatech.Stats;
using UnityEngine;
namespace Arcatech.Triggers
{
    [CreateAssetMenu(fileName = "New Serialized Stat Mod Condition", menuName = "Items/Stats/Stat mod condition/Check current value", order = 2)]
    public class CheckCurrentValuePercent : SerializedStatModCondition
    {
        [SerializeField] ConditionComparer Comparison;
        [SerializeField, Range (0,100)] float PercentCutoff;
        public override bool CheckCondition(StatValueContainer c)
        {
            switch (Comparison)
            {
                case ConditionComparer.Greater:
                    return (c.GetPercent > PercentCutoff);
                case ConditionComparer.Less:
                    return (c.GetPercent < PercentCutoff);
            }
            return false;
        }
    }

}