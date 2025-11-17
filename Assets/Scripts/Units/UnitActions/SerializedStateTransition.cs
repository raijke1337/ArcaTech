using System;
using Arcatech.Actions;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/State Transition")]
    public class SerializedStateTransition : ScriptableObject
    {
        public int Priority;
        public SerializedStateTransitionCondition[] conditions = new SerializedStateTransitionCondition[0];
        public SerializedUnitState nextState;
        public SerializedActionResult[] onTransition = new SerializedActionResult[0];
        

        [Tooltip("If true, require the source state's animation to reach this normalized time (0..1) before allowing the transition.")]
        [Range(0f, 1f)]
        public float requireExitNormalizedTime = 1f;

        public bool overrideMinTime = false;

        public StateTransition Build()
        {
            return new StateTransition(nextState.Build(), 
                onTransition, 
                requireExitNormalizedTime, 
                conditions,Priority,
                overrideMinTime);
        }
        
    }
    
    public class StateTransition
    {
        public int TransitionPriority { get; }
        public UnitState NextState { get; }
        public ActionResult[] OnTransition { get; }
        public float ExitNormalizedTime { get; }
        public SerializedStateTransitionCondition[] Conditions { get; }
        public bool CanOverrideMinimumStateTime { get; }

        public StateTransition(UnitState nextState,
            SerializedActionResult[] onTransition,
            float exitNormalizedTime,
            SerializedStateTransitionCondition[] conditions,
            int transitionPriority,
            bool isOverride)
        {
            NextState = nextState;
            if (onTransition != null && onTransition.Length > 0)
            {
                OnTransition = new ActionResult[onTransition.Length];
                for (int i = 0; i < onTransition.Length; i++)
                {
                    OnTransition[i] = onTransition[i].BuildActionResult();
                }
            }
            ExitNormalizedTime = Mathf.Clamp01(exitNormalizedTime);
            Conditions = conditions ?? Array.Empty<SerializedStateTransitionCondition>();
            TransitionPriority = transitionPriority;  
            CanOverrideMinimumStateTime = isOverride;
        }

        public bool CanTransition(StateMachineContext ctx)
        {
            if (Conditions == null || Conditions.Length == 0) return true;
            foreach (var c in Conditions)
                if (c != null && !c.CanTransition(ctx))
                    return false;
            return true;
        }

        public string DebugConditions
        {
            get
            {
                string result = string.Empty;
                foreach (var c in Conditions)
                {
                    result += $"{c.ConditionName} \n";
                }
                return result;
            }
        }
    }
}