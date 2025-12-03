using System;
using System.Linq;
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
            if (NextState == null)
            {
                throw new ArgumentNullException(nameof(NextState));
            }
            OnTransition = onTransition?.Length > 0
                ? onTransition.Select(a => a.BuildActionResult()).ToArray()
                : Array.Empty<ActionResult>();

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