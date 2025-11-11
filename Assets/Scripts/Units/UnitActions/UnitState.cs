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
        public string StateName { get; }
        public float TimeInState => _stateTimer.GetTime;
        private readonly StopwatchTimer _stateTimer;
        public bool AllowsAiming { get; }
        public bool AllowsMovement { get; }
        public bool Invulnerable { get; }
        public StateTransition[] Transitions { get; private set; }
        public ActionResult[] OnEnterState { get; }
        public ActionResult[] OnExitState { get; }

        private int _animatorHash;
        private int _animatorLayer;
        readonly float _crossfadeTime;

        public UnitState(
            string name,
            int animatorHash = 0,
            float crossfadeTime = 0.1f,
            int animatorLayer = 0,
            bool allowsMove = true,
            bool allowsAim = true,
            bool invulnerable = false,
            StateTransition[] transitions = null,
            SerializedActionResult[] onEnter = null,
            SerializedActionResult[] onExit = null)
        {
            StateName = name;
            _animatorHash = animatorHash;
            _crossfadeTime = crossfadeTime;
            _animatorLayer = animatorLayer;
            AllowsMovement = allowsMove;
            AllowsAiming = allowsAim;
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

            _stateTimer = new StopwatchTimer();
        }

        internal void InternalSetTransitions(StateTransition[] transitions)
        {
            Transitions = transitions ?? Array.Empty<StateTransition>();
        }

        public void EnterState(StateMachineContext context, Animator animator)
        {
            Debug.Log($"EnterState {this.StateName}");
            _stateTimer.Reset();
            _stateTimer.Start();

            // if (_actor.GetMainEntity.ShowingDebugs) Debug.Log($"Entering state {this}");

            // Apply animator crossfade if an animation name/hash was provided
            if (animator != null && _animatorHash != 0)
                animator.CrossFadeInFixedTime(_animatorHash, _crossfadeTime, _animatorLayer);


            foreach (var m in context.Movers)
            {
                Debug.Log($"{m} can move: {AllowsMovement}");
                m.CanMove = AllowsMovement;
            }

            foreach (var m in context.Aimers)
            {
                Debug.Log($"{m} can aim: {AllowsAiming}");
                m.CanAim = AllowsAiming;
            }

            foreach (var i in context.Invulnerabiles)
            {
                Debug.Log($"{i} is FUCKING INVINCIBLE: {Invulnerable}");
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
            Debug.Log($"Exit {StateName} after {_stateTimer.GetTime}");
            if (OnExitState == null || OnExitState.Length == 0) return;
            foreach (var a in OnExitState)
                a?.ProduceResult(context.Owner, null, context.Spawn);
            _stateTimer.Stop();

        }

        // Choose a valid transition based on conditions and optional exit-time requirement
        public StateTransition ChooseTransition(StateMachineContext ctx, Animator animator)
        {
            StateTransition highestPriority = null;
            // Iterate transitions in the order provided; you can sort by priority if you add such a field
            foreach (var t in Transitions)
            {
                if (t == null) continue;

                // If this transition requires the animation to reach a normalized time before firing, check that
                if (!HasPassedExitTime(animator, t.ExitNormalizedTime)) continue;

                if (!t.CanTransition(ctx)) continue;
                if (highestPriority == null) highestPriority = t;
                else
                {
                    if (t.TransitionPriority > highestPriority.TransitionPriority) highestPriority = t;
                }
            }

            return highestPriority;
        }

        public bool ExitTimePassed (Animator a, float normalizedTime)=> HasPassedExitTime(a,normalizedTime);
        private bool HasPassedExitTime(Animator animator, float requiredNormalized)
        {
            requiredNormalized = Mathf.Clamp01(requiredNormalized);
            if (requiredNormalized <= 0f) return true;
            if (animator == null) return _stateTimer.GetTime > 0.01f; // can't check animator, allow tiny progress

            var info = animator.GetCurrentAnimatorStateInfo(_animatorLayer);
            // If there's a transition in progress, check next state's normalized time too (helps blends)
            if (animator.IsInTransition(_animatorLayer))
            {
                var next = animator.GetNextAnimatorStateInfo(_animatorLayer);
                if (next.IsName("") == false)
                    return next.normalizedTime >= requiredNormalized;
            }

            return info.normalizedTime >= requiredNormalized;
        }
    }
}