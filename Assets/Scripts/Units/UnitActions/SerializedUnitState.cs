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
        [Range(0,1)]public float minTimeInStateNormalized = 0f;

        [Header("Gameplay locks")] public bool allowsMovement = true;
        public bool allowsAiming = true;
        public bool invulnerable = false;

        [Header("Root motion enabled override disables aiming and movement")]
        public bool rootMotionEnabled = false;

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

        UnitState Build(Dictionary<SerializedUnitState, UnitState> cache)
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
                minTime: minTimeInStateNormalized,
                isRootMotionState:rootMotionEnabled,
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

                var ove = t.overrideMinTime;
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
                    transitionPriority: t.Priority,
                    ove);

                runtimeTransitions.Add(rt);
            }

            // Wire transitions into the placeholder now that they are built
            placeholder.InternalSetTransitions(runtimeTransitions.ToArray());

            return placeholder;
        }
    

    }
}