using System;
using Arcatech.Actions;
using UnityEditorInternal;
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
        

        [Header("The transition will not be valid unless this time has passed in source state")]
        [Range(0f, 1f)]
        public float minTimeInSourceStateNormalized = 1f;
        [Header("Override source state's minimum required time")]
        public bool overrideMinTime = false;


        public StateTransition Build()
        {
            return new StateTransition(nextState.Build(), 
                onTransition, 
                minTimeInSourceStateNormalized, 
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
                result += $"Required normalized time in source state was: {ExitNormalizedTime}. Override: {CanOverrideMinimumStateTime}";
                return result;
            }
        }
    }
}