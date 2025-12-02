using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Units.Control;
using KBCore.Refs;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Arcatech.Units
{
    /// <summary>
    /// new component to define a unit that has some state (e.g. idle, attacking, stunned...)
    /// </summary>
    [RequireComponent(typeof(BaseGameEntityComponent))]

    public class EntityStateMachineComponent : ValidatedMonoBehaviour, IPausableComponent, IKillableComponent
    {
        [SerializeField, Self] BaseGameEntityComponent gameEntity;
        [SerializeField, Child] protected Animator animator;

        [Space, Header("States")] [SerializeField]
        private SerializedUnitState startingState;

        private StateMachineContext _context;
        public BaseGameEntityComponent GetMainEntity => gameEntity;
        private UnitState _currentState;

        readonly string _animatorParameterName = "TimeInState";
        int _animatorParameter;

        private List<StateTransition> _addedTransitions = new();

        protected void Start()
        {
            _animatorParameter = Animator.StringToHash(_animatorParameterName);
            
            _context = new StateMachineContext() { Spawn = gameEntity.transform, Owner = gameEntity };
            _currentState = startingState.Build();
            _context.CurrentState = _currentState;
            _context.Owner = gameEntity;
            _context.Aimers = GetComponentsInChildren<IAim>();
            _context.Movers = GetComponentsInChildren<IMove>();
            _context.Invulnerables = GetComponentsInChildren<IInvulnerability>();
                 _candidates = new List<(StateTransition tr, bool local)>();
            _context.Stats = GetComponentInChildren<EntityStatsComponent>();
            
            _currentState.EnterState(_context, animator);
        }

        private void Update()
        {
            if (Paused || Killed) return;
            _currentState?.UpdateState(Time.deltaTime);
            animator.SetFloat(_animatorParameter, _currentState?.TimeInState ?? 0);

            int safety = 0;
            const int kMaxChain = 8;
            while (safety++ < kMaxChain)
            {
                bool committed = TransitionsInUpdate();
                if (!committed) break;
            }
        }

        public bool TryCommandTransition(UnitActionType actionType,
            IEnumerable<IUnitCommandPerformer> commandPerformers)
        {
            if (Paused || Killed) return false;

            _context.PendingCommand = actionType;
            bool validated = ValidateCommandTransition();
            foreach (var v in commandPerformers)
            {
                v.DoUnitCommand(actionType, validated);
            }
            return validated;
        }
   
        private bool TransitionsInUpdate()
        {
            if (Paused || Killed) return false;

            // Pick the best transition among local and global candidates
            bool wasLocal;
            var chosen = PickBestTransition(out wasLocal);
            if (chosen == null || chosen.NextState == null)
            {
                return false;
            }

            // For runtime updates (Update loop) we want to clear PendingCommand on commit
            if (!TryCommitTransition(chosen))
            {
                // aborted by action/performer
                return false;
            }
            return true;
        }

        // New helper: try to run command performers (if provided), run OnTransition actions and commit.
        bool TryCommitTransition(StateTransition tr/*, IEnumerable<IUnitCommandPerformer> commandPerformers*/)
        {
            if (tr?.NextState == null) return false;
            if (!ExecuteTransitionActions(tr))
            {
              //  Debug.LogWarning($"Transition action failed, aborting transition from {_currentState}");
                _context.ClearCommand();
                return false;
            }
            CommitTransition(tr);
            return true;
        }

        /// final action
        private void CommitTransition(StateTransition tr)
        {
            if (tr == null || tr.NextState == null) return;
            
            _currentState.ExitState(_context, animator);
            _currentState = tr.NextState;
            _context.CurrentState = _currentState;
            _currentState.EnterState(_context, animator);
            
            _context.ClearCommand();
        }

        
        bool ExecuteTransitionActions(StateTransition tr)
        {
            if (tr.OnTransition == null) return true;
            foreach (var a in tr.OnTransition)
            {
                if (a == null) continue;
                bool ok = a.ProduceResult(_context.Owner, null, _context.Spawn.position, _context.Spawn.rotation);
                if (!ok) return false;
            }

            return true;
        }     
        bool ValidateCommandTransition()
        {
            if (_currentState == null)
            {
                _context.ClearCommand();
                return false;
            }
            bool wasLocal;
            var chosen = PickBestTransition(out wasLocal);
            if (chosen is { NextState: not null }) return TryCommitTransition(chosen);
            _context.ClearCommand();
            return false;
        }

        public void ForceUnitState(UnitState forcedState, bool immediate = true)
        {
            if (forcedState == null) return;
            _currentState?.ExitState(_context, animator);
            _currentState = forcedState;
            _context.CurrentState = _currentState;
            _context.ClearCommand();
            _currentState.EnterState(_context, animator);
        }

        public void AddTransition(StateTransition transition)
        {
            if (transition == null || _addedTransitions.Contains(transition)) return;
           // Debug.Log($"added transition to {transition.NextState}");
            _addedTransitions.Add(transition);
        }

        public void RemoveTransition(StateTransition transition)
        {
            if (transition == null || !_addedTransitions.Contains(transition)) return;
            _addedTransitions.Remove(transition);
        }

        private List<(StateTransition tr, bool local)> _candidates;
        private void RefreshCandidates()
        {
            _candidates.Clear();
            var candidates = new List<(StateTransition, bool)>();
            if (_currentState == null) return;
            
            bool canExit = _currentState.CanExitState(animator);
            
            foreach (var t in _currentState.Transitions ?? Array.Empty<StateTransition>())
            {
                if (t == null || t.NextState == null) continue;
                if (!t.CanTransition(_context))continue;
                if (!canExit && !t.CanOverrideMinimumStateTime) continue;
                if (!_currentState.TransitionMinTimeInStateSatisfied(animator, t.ExitNormalizedTime)) continue;

                candidates.Add((t, true));
            }

            // global transitions
            foreach (var g in _addedTransitions)
            {
                if (g == null || g.NextState == null) continue;
                if (!g.CanTransition(_context))continue;
                if (!canExit && !g.CanOverrideMinimumStateTime)continue;
                if (!_currentState.TransitionMinTimeInStateSatisfied(animator, g.ExitNormalizedTime)) continue;
                candidates.Add((g, false));
            }
        }

        
        private StateTransition PickBestTransition(out bool wasLocal)
        {
            wasLocal = false;
            RefreshCandidates();
            if (_candidates.Count == 0) return null;

            var best = _candidates
                .OrderByDescending(c => c.tr.TransitionPriority)
                .ThenByDescending(c => c.local ? 0 : 1) // global wins ties
                .FirstOrDefault();

            if (best.tr == null) return null;
            
            wasLocal = best.local;
            return best.tr;
        }
        
        public bool Paused { get; set; }
        public bool Killed { get; set; }
    }

}