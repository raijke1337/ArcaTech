using Arcatech.Stats;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Lewding
{
    [CreateAssetMenu(menuName = "States/State Transition Condition/H/LewdnessStage")]
    public class LewdnessStageCondition : SerializedStateTransitionCondition
    {
        const float eps = 0.0001f;
        public ConditionOp Comparison;
        public float CompareTo;
        public override string ConditionName => $"Lewdness is {Comparison} than {CompareTo}";
        public override bool CanTransition(StateMachineContext ctx)
        {
            var val = ctx.EcchiContext.LewdStage;
            switch (Comparison)
            {
                case ConditionOp.Greater: return val > CompareTo;
                case ConditionOp.GreaterOrEqual: return val >= CompareTo;
                case ConditionOp.Less: return val < CompareTo;
                case ConditionOp.LessOrEqual: return val <= CompareTo;
                case ConditionOp.Equal: return Mathf.Abs(val - CompareTo) <= eps;
                case ConditionOp.NotEqual: return Mathf.Abs(val - CompareTo) > eps;
                case ConditionOp.Between:
                    Debug.Log("Not inplemented");
                    return false;
                default: return true;
            }
        }
    }
}