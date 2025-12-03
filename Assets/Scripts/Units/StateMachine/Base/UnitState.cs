using Arcatech.Actions;
using System;
using System.Linq;
using Unity.AppUI.UI;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Units
{
    public class UnitState
    {
        public override string ToString() => StateName;
        private string StateName { get; }
        public float TimeInState => _stateTimer.GetTime;
        private readonly StopwatchTimer _stateTimer;
        private bool AllowsAiming { get; }
        private bool AllowsMovement { get; }
        private bool Invulnerable { get; }
        private bool IsRootMotionState { get; }
        public StateTransition[] Transitions { get; private set; }
        private ActionResult[] OnEnterState { get; }
        private ActionResult[] OnExitState { get; }

        private int _animatorHash;
        private int _animatorLayer;
        readonly float _crossfadeTime;

        /// <summary>
        /// until this time is reached, state will not exit (no transition is valid)
        /// </summary>
        private float _minNormalizedTimeInState;

        private Animator _animator;
        public UnitState(
            string name,
            int animatorHash = 0,
            float crossfadeTime = 0.1f,
            float minNormalizedTime = 0f,
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
                AllowsMovement = allowsMove;
                AllowsAiming = allowsAim;
            }
            Invulnerable = invulnerable;
            Transitions = transitions ?? Array.Empty<StateTransition>();
            
            OnEnterState = onEnter?.Length > 0
                ? onEnter.Select(a => a.BuildActionResult()).ToArray()
                : Array.Empty<ActionResult>();
            
            OnExitState = onExit?.Length > 0
                ? onExit.Select(a => a.BuildActionResult()).ToArray()
                : Array.Empty<ActionResult>();

            _minNormalizedTimeInState = minNormalizedTime;
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
            _animator =  animator;

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

            foreach (var i in context.Invulnerables)
            {
                i.Invulnerable = Invulnerable;
            }

            // Execute on-enter actions

            if (OnEnterState == null || OnEnterState.Length <= 0) return;
            foreach (var a in OnEnterState)
                a?.ProduceResult(context.Owner, null, context.Spawn.position, context.Spawn.rotation);
        }

        public void UpdateState(float delta)
        {
            _stateTimer?.Tick(delta);
        }


        public void ExitState(StateMachineContext context, Animator animator)
        {
            if (OnExitState == null || OnExitState.Length == 0) return;
            foreach (var a in OnExitState)
                a?.ProduceResult(context.Owner, null, context.Spawn.position, context.Spawn.rotation);
            _stateTimer.Stop();

        }
        public bool CanExitState(Animator animator)
        {
            if (_minNormalizedTimeInState <= 0f) return true;

            if (TryGetAnimatorProgress(animator, out float normalized))
                return normalized >= _minNormalizedTimeInState;

            // Fallback: use real time since enter so we don't get stuck forever.
            return _stateTimer.GetTime >= (_minNormalizedTimeInState * 0.1f); // or some small grace
        }

        public bool TransitionMinTimeInStateSatisfied(Animator animator, float timeNormalized)
        {
            timeNormalized = Mathf.Clamp01(timeNormalized);
            if (timeNormalized <= 0f) return true;

            if (TryGetAnimatorProgress(animator, out float normalized))
                return normalized >= timeNormalized;

            // Animator hasn’t entered this state yet, so don’t allow transitions that require time.
            return false;
        }
        
        private bool TryGetAnimatorProgress(Animator animator, out float normalized)
        {
            normalized = 0f;
            if (animator == null) return false;

            var layer = _animatorLayer;

            // If we're in the intended state, use current info.
            var current = animator.GetCurrentAnimatorStateInfo(layer);
            if (current.shortNameHash == _animatorHash)
            {
                normalized = current.normalizedTime % 1f;
                return true;
            }

            // If we're blending toward it, use next info.
            if (animator.IsInTransition(layer))
            {
                var next = animator.GetNextAnimatorStateInfo(layer);
                if (next.shortNameHash == _animatorHash)
                {
                    normalized = next.normalizedTime % 1f;
                    return true;
                }
            }

            return false; // Animator isn’t on our clip yet.
        }
    }
}