using System;
using Arcatech.Actions;
using UnityEngine;

namespace Arcatech.Units
{
    public class StateTransition
    {
        public int TransitionPriority { get; }
        public UnitState NextState { get; }
        public ActionResult[] OnTransition { get; }
        public float ExitNormalizedTime { get; }
        private SerializedStateTransitionCondition[] Conditions { get; }
        public bool CanOverrideMinimumStateTime { get; }
      //  public bool IsCommandTransition { get; }

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
          //  IsCommandTransition =  isCommand;
        }

        public bool CanTransition(StateMachineContext ctx)
        {
            if (Conditions == null || Conditions.Length == 0) return true;
            foreach (var c in Conditions)
                if (c != null && !c.CanTransition(ctx))
                    return false;
            return true;
        }
    }
}