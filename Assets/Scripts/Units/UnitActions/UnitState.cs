using Arcatech.Actions;
using System;
using System.Linq;
using Arcatech.Units.Control;
using Unity.AppUI.UI;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Units
{
    public enum UnitActionState
    {
        None,
        Started,
        ExitTime,
        Completed
    }

    public class StateMachineContext
    {
        public UnitActionType PendingCommand;
        public Transform Spawn;
        public UnitState CurrentState;
        public BaseGameEntityComponent Owner;
        
        public IMove[] Movers;
        public IAim[] Aimers;
        public IInvulnerability[] Invulnerabiles;
        public void ClearCommand ()=> PendingCommand = UnitActionType.None;
    }



    public class UnitState
    {
        public override string ToString()
        {
            return StateName;
        }

        public string StateName { get; }
        public float TimeInState => _stateTimer.GetTime;
        private readonly StopwatchTimer _stateTimer;
        public bool AllowsAiming { get; }
        public bool AllowsMovement { get; }
        public bool Invulnerable { get; }
        public bool IsRootMotionState { get; }
        public StateTransition[] Transitions { get; private set; }
        public ActionResult[] OnEnterState { get; }
        public ActionResult[] OnExitState { get; }

        private int _animatorHash;
        private int _animatorLayer;
        readonly float _crossfadeTime;

        /// <summary>
        /// until this time is reached, state will not exit (no transition is valid)
        /// </summary>
        private float _minTimeInState;
        
        public UnitState(
            string name,
            int animatorHash = 0,
            float crossfadeTime = 0.1f,
            float minTime = 0f,
            int animatorLayer = 0,
            bool allowsMove = true,
            bool allowsAim = true,
            bool invulnerable = false,
            bool isRootMotionState = false,
            StateTransition[] transitions = null,
            SerializedActionResult[] onEnter = null,
            SerializedActionResult[] onExit = null)
        {
            StateName = name;
            _animatorHash = animatorHash;
            _crossfadeTime = crossfadeTime;
            _animatorLayer = animatorLayer;
            IsRootMotionState = isRootMotionState;
            if (isRootMotionState)
            {
                AllowsMovement = false;
                AllowsAiming = false;
            }
            else
            {
                AllowsMovement = allowsAim;
                AllowsAiming = allowsAim;
            }
            Invulnerable = invulnerable;
            Transitions = transitions ?? Array.Empty<StateTransition>();
            if (onEnter != null && onEnter.Length > 0)
            {
                OnEnterState = new ActionResult[onEnter.Length];
                for (int i = 0; i < onEnter.Length; i++)
                {
                    OnEnterState[i] = onEnter[i].BuildActionResult();
                }
            }

            if (onExit != null && onExit.Length > 0)
            {
                OnExitState = new ActionResult[onExit.Length];
                for (int i = 0; i < onExit.Length; i++)
                {
                    OnExitState[i] = onExit[i].BuildActionResult();
                }
            }

            _minTimeInState = minTime;
            _stateTimer = new StopwatchTimer();
        }

        internal void InternalSetTransitions(StateTransition[] transitions)
        {
            Transitions = transitions ?? Array.Empty<StateTransition>();
        }

        public void EnterState(StateMachineContext context, Animator animator)
        {
            _stateTimer.Reset();
            _stateTimer.Start();


            // Apply animator crossfade if an animation name/hash was provided
            if (animator != null && _animatorHash != 0)
                animator.CrossFadeInFixedTime(_animatorHash, _crossfadeTime, _animatorLayer);


            foreach (var m in context.Movers)
            {
                m.CanMove = AllowsMovement;
                m.UseRootMotion = IsRootMotionState;
            }

            foreach (var m in context.Aimers)
            {
                m.CanAim = AllowsAiming;
            }

            foreach (var i in context.Invulnerabiles)
            {
                i.Invulnerable = Invulnerable;
            }

            // Execute on-enter actions

            if (OnEnterState == null || OnEnterState.Length <= 0) return;
            foreach (var a in OnEnterState)
                a?.ProduceResult(context.Owner, null, context.Spawn);
        }

        public void UpdateState(float delta)
        {
            _stateTimer?.Tick(delta);
        }


        public void ExitState(StateMachineContext context, Animator animator)
        {
            if (OnExitState == null || OnExitState.Length == 0) return;
            foreach (var a in OnExitState)
                a?.ProduceResult(context.Owner, null, context.Spawn);
            _stateTimer.Stop();

        }
        public bool CanExitState(Animator animator)
        {
            // No minimum -> can exit immediately
            if (_minTimeInState <= 0f) return true;

            // If animator is not available, *fallback* to TimeInState comparing against
            // MinimumTimeInStateNormalized interpreted as seconds fallback (documented caveat)
            if (animator == null)
            {
                // Fallback heuristic: treat MinimumTimeInStateNormalized as seconds if animator missing.
                return TimeInState >= _minTimeInState;
            }

            // Get the current animator state info for this state's layer/hash if available
            // assuming UnitState stores animatorHash and animatorLayer fields:
            var layer = _animatorLayer;
            var info = animator.GetCurrentAnimatorStateInfo(layer);

            // If we're actually in the intended animator state, use normalizedTime
            if (info.shortNameHash == _animatorHash) // if you store AnimatorStateHash
            {
                // normalizedTime can grow > 1.0 for looping states; we use fraction
                float normalized = info.normalizedTime % 1.0f;
                return normalized >= _minTimeInState;
            }

            // If animator is not in the state's expected animator state, treat it as passed
            // (we're already in a different anim state, so allow exit)
            return true;
        }

        public bool TransitionMinTimeInStateSatisfied(Animator animator, float timeNormalized)
        {
            timeNormalized = Mathf.Clamp01(timeNormalized);
            if (timeNormalized <= 0f) return true;
            if (animator == null) return _stateTimer.GetTime > 0.01f; // can't check animator, allow tiny progress
            var info = animator.GetCurrentAnimatorStateInfo(_animatorLayer);
            // If there's a transition in progress, check next state's normalized time too (helps blends)
            if (animator.IsInTransition(_animatorLayer))
            {
                var next = animator.GetNextAnimatorStateInfo(_animatorLayer);
                if (next.IsName("") == false)
                    return next.normalizedTime >= timeNormalized;
            }

            return info.normalizedTime >= timeNormalized;
            
        }
    }
}