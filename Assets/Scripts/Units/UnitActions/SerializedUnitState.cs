using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Actions;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Units
{
    [CreateAssetMenu(fileName = "Unit state ", menuName = "States/Unit state")]
    public class SerializedUnitState : ScriptableObject
    {
        [Header("Identity")] public string stateDisplayName = "NewState";

        [Header("Animator")]
        [Tooltip("Animator state name (use the state name in the Animator). If empty no animator call will be made.")]
        public string animatorStateName = "";

        public int animatorLayer = 0;
        public float crossfadeTime = 0.1f;

        [Header("Gameplay locks")] public bool allowsMovement = true;
        public bool allowsAiming = true;
        public bool invulnerable = false;

        [Header("State data")] public SerializedStateTransition[] transitions = new SerializedStateTransition[0];
        public SerializedActionResult[] onEnterActions = new SerializedActionResult[0];
        public SerializedActionResult[] onExitActions = new SerializedActionResult[0];

        // Build a runtime UnitState instance from this ScriptableObject.
        // The created UnitState is purely a data + behavior object (not a UnityEngine.Object).

        public UnitState Build()
        {
            // convenience wrapper that creates a new cache for top-level calls
            return Build(new Dictionary<SerializedUnitState, UnitState>());
        }

        public UnitState Build(Dictionary<SerializedUnitState, UnitState> cache)
        {
            // Return cached runtime state if already built (prevents recursion)
            if (cache.TryGetValue(this, out var cached)) return cached;

            // Create placeholder runtime UnitState with NO transitions yet and put into cache
            int animHash = string.IsNullOrEmpty(animatorStateName) ? 0 : Animator.StringToHash(animatorStateName);

            // Construct a placeholder UnitState with empty transitions (we'll fill them next).
            var placeholder = new UnitState(
                name: stateDisplayName,
                animatorHash: animHash,
                crossfadeTime: crossfadeTime,
                animatorLayer: animatorLayer,
                allowsMove: allowsMovement,
                allowsAim: allowsAiming,
                invulnerable: invulnerable,
                transitions: Array.Empty<StateTransition>(),
                onEnter: onEnterActions ?? Array.Empty<SerializedActionResult>(),
                onExit: onExitActions ?? Array.Empty<SerializedActionResult>());

            cache[this] = placeholder; // important: add before recursing to break cycles

            // Now build transitions and resolve nextState using the same cache
            var runtimeTransitions = new List<StateTransition>();
            var transArray = transitions ?? Array.Empty<SerializedStateTransition>();
            foreach (var t in transArray)
            {
                if (t == null) continue;

                // Resolve next state using the SAME cache -> avoids infinite recursion
                var next = t.nextState != null ? t.nextState.Build(cache) : null;

                var onTransitionArray = t.onTransition != null
                    ? t.onTransition.ToArray()
                    : Array.Empty<SerializedActionResult>();

                var conditionsArray = t.conditions != null
                    ? t.conditions.ToArray()
                    : Array.Empty<SerializedStateTransitionCondition>();

                var rt = new StateTransition(
                    nextState: next,
                    onTransition: onTransitionArray,
                    exitNormalizedTime: t.requireExitNormalizedTime,
                    conditions: conditionsArray,
                    transitionPriority: t.Priority);

                runtimeTransitions.Add(rt);
            }

            // Wire transitions into the placeholder now that they are built
            placeholder.InternalSetTransitions(runtimeTransitions.ToArray());

            return placeholder;
        }
    


    // public UnitState Build()
        // {
        //     int animHash = string.IsNullOrEmpty(animatorStateName) ? 0 : Animator.StringToHash(animatorStateName);
        //
        //     // Convert transitions into runtime StateTransition instances
        //     var runtimeList = new List<StateTransition>();
        //     
        //     var runtimeTransitions = new StateTransition[transitions.Length];
        //     if (transitions != null)
        //     {
        //         
        //         
        //         foreach (var t in transitions)
        //         {
        //             if (t == null) continue;
        //
        //             var nextState = t.nextState != null ? t.nextState.Build() : null;
        //            // overflow here in cases move-idle-move
        //             SerializedActionResult[] onTransitionArray;
        //             if (t.onTransition != null)
        //                 onTransitionArray = t.onTransition.Select(a => a as SerializedActionResult).ToArray();
        //             else
        //                 onTransitionArray = Array.Empty<SerializedActionResult>();
        //
        //             var conditionsArray = t.conditions != null
        //                 ? t.conditions.ToArray()
        //                 : Array.Empty<SerializedStateTransitionCondition>();
        //
        //             var rt = new StateTransition(
        //                 nextState,
        //                 onTransitionArray,
        //                 t.requireExitNormalizedTime,
        //                 conditionsArray,
        //                 t.Priority
        //             );
        //
        //             runtimeList.Add(rt);
        //         }
        //         runtimeTransitions =  runtimeList.ToArray();
        //     }
        //     // Convert actions (we store the SOs and call them via Execute on the SO)
        //     var enterActions = (onEnterActions ?? Array.Empty<SerializedActionResult>()).Select(a => a).ToArray();
        //     var exitActions = (onExitActions ?? Array.Empty<SerializedActionResult>()).Select(a => a).ToArray();
        //
        //     // Construct the runtime UnitState
        //     return new UnitState(
        //         stateDisplayName,
        //         animatorHash: animHash,
        //         crossfadeTime: crossfadeTime,
        //         animatorLayer: animatorLayer,
        //         allowsMove: allowsMovement,
        //         allowsAim: allowsAiming,
        //         invulnerable: invulnerable,
        //         transitions: runtimeTransitions,
        //         onEnter: enterActions,
        //         onExit: exitActions
        //     );
        // }
    }
}