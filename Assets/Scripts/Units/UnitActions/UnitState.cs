using Arcatech.Actions;
using System;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Units
{
    public class UnitState : IUnitAction
    {
        public static UnitState Build(ActiveGameUnitComponent u, bool lck, NextActionSettings next, string anim, float exit, SerializedActionResult[] onstart, SerializedActionResult[] onfinish, SerializedActionResult[] onExit, Transform place, float crossfade)
        {
            return new UnitState(u, lck, next, anim, exit,onstart, onfinish, onExit,place,crossfade);
        }

        UnitState(ActiveGameUnitComponent u, bool locks, NextActionSettings next, string animatorSt, float exitTimeMult, SerializedActionResult[] onstart, SerializedActionResult[] onfinish, SerializedActionResult[] onExit, Transform place,float crossfade)
        {
            Actor = u;
            LockMovement = locks;
            Next = next;
            animatorState = animatorSt;
            _crossfadeTime = crossfade;

            this.place = place;
            animator = u.GetComponentInChildren<Animator>();
            
            if (HasState(animatorState)) stateHash = Animator.StringToHash(animatorSt);
            
            
            if (animatorState != null && animator.runtimeAnimatorController.animationClips.Any(t=>t.name == animatorState))
            {

                var clip = animator.runtimeAnimatorController.animationClips.First(t => t.name == animatorState);
                var clipLength = clip.length;

                AnimatorController ac = animator.runtimeAnimatorController as AnimatorController;
                var l = ac.layers;

                foreach (var layer in l)
                {
                    var s = layer.stateMachine.states;
                    foreach (var state in s)
                    {
                        if (state.state.name == animatorState)
                        {
                            var animSpeedMult = state.state.speed;
                            //Debug.Log($"found anim speed {animSpeedMult} for animation {_animationName}");

                            _totalActionTime = clipLength / animSpeedMult;
                            _exitActionTime = _totalActionTime * exitTimeMult;
                            if (u.GetMainEntity.ShowingDebugs) Debug.Log($"Action {this} exit at {_exitActionTime} complete at {_totalActionTime}");
                            break;
                        }
                    }
                }
            }


            if (onstart != null && onstart.Length > 0)
            {
                OnEnterState = new ActionResult[onstart.Length]; 
                for (int i = 0; i < onstart.Length; i++)
                {
                    OnEnterState[i] = onstart[i].BuildActionResult();
                }
            }
            if (onfinish != null && onfinish.Length > 0)
            {
                OnExitState = new IActionResult[onfinish.Length];
                for (int i = 0; i < onfinish.Length; i++)
                {
                    OnExitState[i] = onfinish[i].BuildActionResult();
                }
            }
            if (onExit != null && onExit.Length > 0)
            {

                OnExitTime = new IActionResult[onExit.Length];
                for (int i = 0; i < onExit.Length; i++)
                {
                    OnExitTime[i] = onExit[i].BuildActionResult();
                }
            }

            _actionTimer = new StopwatchTimer();
        }
        
        private bool HasState(string stateName, int layer = 0)
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                return false;
        
            // Get the state hash
            stateHash = Animator.StringToHash(stateName);
        
            // Check if the state exists in the specified layer
            return animator.HasState(layer, stateHash);
        }

        private int stateHash;
       
        
        protected Animator animator;
        protected ActiveGameUnitComponent Actor;
        public bool LockMovement { get; protected set; }
        
        public IActionResult[] OnEnterState { get; private set;  }
        public IActionResult[] OnExitState { get; private set; }
        public IActionResult[] OnExitTime { get;private set; }
        protected NextActionSettings Next { get; }


        StopwatchTimer _actionTimer;
        readonly float _totalActionTime;
        readonly float _exitActionTime;

        readonly Transform place;
        readonly string animatorState;
        readonly float _crossfadeTime;

        public event UnityAction<UnitActionState> ActionStateChangedEvent = delegate { };

        UnitActionState _actionState = UnitActionState.None;
        public UnitActionState GetActionState => _actionState;



        public UnitActionState UpdateAction(float delta)
        {
            _actionTimer?.Tick(delta);
            
            if (_actionTimer.GetTime >= _exitActionTime && _actionState == UnitActionState.Started)
            {
                ExitTimeAction();
            }
            if (_actionTimer.GetTime >= _totalActionTime && _actionState == UnitActionState.ExitTime)
            {
                ExitState();
            }
            return _actionState;
        }

        public bool CanAdvance(out SerializedUnitState next)
        {
            next = null;

            bool ok = Next.GetNextState != null && Next.CheckTime(_actionTimer.GetTime / _totalActionTime);
            if (ok)
            {
                next = Next.GetNextState;
            }
            return ok;
        }
        
        public void StartState()
        {
            string start = "";
            
            animator.CrossFade(stateHash, _crossfadeTime);
            
            if (OnEnterState != null)
            {
                foreach (var r in OnEnterState)
                {
                    if (r == null)
                    {
                        start += "NULL RESULT";
                    }// bandaid TODO dunno why it happens
                    else
                    {
                        r.ProduceResult(Actor.GetMainEntity, null, place);
                        start += (r.ToString() + ' ');
                    }
                }
            }
            ActionStateChangedEvent.Invoke(UnitActionState.Started);
            _actionTimer.Reset();
            _actionTimer.Start();
            _actionState = UnitActionState.Started;

           // if (Actor.UnitDebug) { Debug.Log($"{this}, result {start}"); }

        }
        void ExitTimeAction()
        {
            string ex = "";
            if (OnExitTime != null)
            {
                foreach (var r in OnExitTime)
                {
                    if (r == null)
                    {
                        ex += "NULL RESULT";
                    }// bandaid TODO dunno why it happens
                    else
                    {
                        r.ProduceResult(Actor.GetMainEntity, null, place);
                        ex += (r.ToString() + ' ');
                    }
                }
            }
            _actionState = UnitActionState.ExitTime;
            ActionStateChangedEvent.Invoke(UnitActionState.ExitTime);
            if (Actor.GetMainEntity.ShowingDebugs) { Debug.Log($"{this}, result {ex}"); }

        }
        public void ExitState()
        {
            if (_actionState == UnitActionState.Completed)
            {
                _actionState = UnitActionState.None;
                return;
            }
            string fin = "";
            if (OnExitState != null)
            {
                foreach (var r in OnExitState)
                {
                    if (r == null)
                    {
                        fin +=  "NULL RESULT";
                    }// bandaid TODO dunno why it happens
                    else
                    {
                        r.ProduceResult(Actor.GetMainEntity, null, place);
                        fin += (r.ToString() + ' ');
                    }
                }
            };
            
            ActionStateChangedEvent.Invoke(UnitActionState.Completed);
            _actionState = UnitActionState.Completed;
            _actionTimer.Stop();


        }
        public override string ToString()
        {
            return animatorState+ " state: "+_actionState;
        }

    }

    [Serializable]
    public class NextActionSettings
    {
        [SerializeField] SerializedUnitState _nextAnim;
        [SerializeField,Range(0f, 1f)] float _chainWindowEnd;
        public bool CheckTime(float currentPercent)
        {
            return (currentPercent <= _chainWindowEnd);
        }
        public SerializedUnitState GetNextState { get => _nextAnim; }
    }
}