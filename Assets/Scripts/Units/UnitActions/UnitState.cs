using Arcatech.Actions;
using System;
using System.Linq;
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
        public string info = "some context";
        public Transform Spawn;
    }



    public class UnitState
    {
        public string StateName { get; }
        private EntityStateMachineComponent _actor;
        public float TimeInState => _stateTimer.GetTime;
        private readonly StopwatchTimer _stateTimer;
        public bool AllowsAiming { get; }
        public bool AllowsMovement { get; }
        public bool Invulnerable { get; }
        public StateTransition[] Transitions { get; }
        public IActionResult[] OnEnterState { get;  }
        public IActionResult[] OnExitState { get; }
        
        private int _animatorHash;
        private int _animatorLayer;
        readonly float _crossfadeTime;
        
        public UnitState(string name,
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
                OnEnterState = new IActionResult[onEnter.Length];
                for (int i = 0; i < onEnter.Length; i++)
                {
                    OnEnterState[i] = onEnter[i].BuildActionResult();
                }
            }
            if (onExit != null && onExit.Length > 0)
            {
                OnExitState = new IActionResult[onExit.Length];
                for (int i = 0; i < onExit.Length; i++)
                {
                    OnExitState[i] = onExit[i].BuildActionResult();
                }
            }
        }
        


        
        public void StartState(StateMachineContext context, Animator animator)
        {
            _stateTimer.Reset();
            _stateTimer.Start();
            
            if (_actor.GetMainEntity.ShowingDebugs) Debug.Log($"Entering state {this}");
            
            // Apply animator crossfade if an animation name/hash was provided
            if (animator != null && _animatorHash != 0)
                animator.CrossFadeInFixedTime(_animatorHash, _crossfadeTime, _animatorLayer);

            // Execute on-enter actions
            
            if (OnEnterState != null)
            {
                foreach (var r in OnEnterState)
                {
                    r.ProduceResult(_actor.GetMainEntity, null,context.Spawn);
                }
            }
           // ActionStateChangedEvent.Invoke(UnitActionState.Started);
            //_actionState = UnitActionState.Started;
        }
        
        public void UpdateState(float delta)
        {
            _stateTimer?.Tick(delta);
        }
       
        
        public void ExitState(StateMachineContext context, Animator animator)
        {
            if (_actor.GetMainEntity.ShowingDebugs) Debug.Log($"Exit state {this} at {Time.time}, " +
                                                              $"time elapsed {_stateTimer.GetTime}");

            foreach (var a in OnExitState)
                a?.ProduceResult(_actor.GetMainEntity, null,context.Spawn);
            _stateTimer.Stop();

        }
        // Choose a valid transition based on conditions and optional exit-time requirement
        public StateTransition ChooseTransition(StateMachineContext ctx, Animator animator)
        {
            // Iterate transitions in the order provided; you can sort by priority if you add such a field
            foreach (var t in Transitions)
            {
                if (t == null) continue;

                // If this transition requires the animation to reach a normalized time before firing, check that
                if (!HasPassedExitTime(animator, t.ExitNormalizedTime)) continue;

                if (t.CanTransition(ctx)) return t;
            }
            return null;
        }

        private bool HasPassedExitTime(Animator animator, float requiredNormalized)
        {
            requiredNormalized = Mathf.Clamp01(requiredNormalized);
            if (requiredNormalized <= 0f) return true;
            if (animator == null) return _stateTimer.GetTime > 0.001f; // can't check animator, allow tiny progress

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


        //
        // float _totalActionTime;
        // float _exitActionTime;

       /// readonly Transform place;

       // public bool LockMovement { get; protected set; 
       // public IActionResult[] OnExitTime { get;private set; }
       



      //  public event UnityAction<UnitActionState> ActionStateChangedEvent = delegate { };

      //  UnitActionState _actionState = UnitActionState.None;
    //    public UnitActionState GetActionState => _actionState;

    // public static UnitState Build(EntityStateMachineComponent u, bool lck, NextActionSettings next, string anim,
    //     float exit, SerializedActionResult[] onstart, SerializedActionResult[] onfinish,
    //     SerializedActionResult[] onExit, Transform place, float crossfade)
    // {
    //     return new UnitState(u, lck, next, anim, exit, onstart, onfinish, onExit, place, crossfade);
    // }

        // // todo remove (check for transitions)
        // public bool CanAdvance(out SerializedUnitState next)
        // {
        //     next = null;
        //
        //     bool ok = Next.GetNextState != null && Next.CheckTime(_stateTimer.GetTime / _totalActionTime);
        //     if (ok)
        //     {
        //         next = Next.GetNextState;
        //     }
        //     return ok;
        // }

     //   private float starttime;

        // void ExitTimeAction()
        // {
        //     if (_actor.GetMainEntity.ShowingDebugs) Debug.Log($"Exit time state {this} at {Time.time}, time elapsed {_stateTimer.GetTime}");
        //     string ex = "";
        //     if (OnExitTime != null)
        //     {
        //         foreach (var r in OnExitTime)
        //         {
        //             if (r == null)
        //             {
        //                 ex += "NULL RESULT";
        //             }// bandaid TODO dunno why it happens
        //             else
        //             {
        //                 r.ProduceResult(_actor.GetMainEntity, null, place);
        //                 ex += (r.ToString() + ' ');
        //             }
        //         }
        //     }
        //     _actionState = UnitActionState.ExitTime;
        //     ActionStateChangedEvent.Invoke(UnitActionState.ExitTime);
        //     if (_actor.GetMainEntity.ShowingDebugs) { Debug.Log($"{this}, result {ex}"); }
        //
        // }


    }
   // private NextActionSettings Next { get; }
    // [Serializable]
    // public class NextActionSettings
    // {
    //     [SerializeField] SerializedUnitState _nextAnim;
    //     [SerializeField,Range(0f, 1f)] float _chainWindowEnd;
    //     public bool CheckTime(float currentPercent)
    //     {
    //         return (currentPercent <= _chainWindowEnd);
    //     }
    //     public SerializedUnitState GetNextState { get => _nextAnim; }
    // }
}