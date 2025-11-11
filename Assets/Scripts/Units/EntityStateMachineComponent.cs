using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;
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

    public class EntityStateMachineComponent : ValidatedMonoBehaviour, IPausableComponent,IKillableComponent
    {
        [SerializeField, Self] BaseGameEntityComponent gameEntity;
        [SerializeField, Child] protected Animator animator;

        [Space, Header("States")] [SerializeField]
        private SerializedUnitState startingState;
        private StateMachineContext _context;
        public BaseGameEntityComponent GetMainEntity => gameEntity;
        private UnitState _currentState;


        private List<StateTransition> _addedTransitions = new();
        protected void Start()
        {
            
            _context = new StateMachineContext() { Spawn = gameEntity.transform, Owner = gameEntity };
            _currentState = startingState.Build();
            _context.CurrentState = _currentState;
            _context.Owner = gameEntity;
            _context.Aimers = GetComponentsInChildren<IAim>();
            _context.Movers = GetComponentsInChildren<IMove>();
            _context.Invulnerabiles= GetComponentsInChildren<IInvulnerability>();
            _currentState.EnterState(_context, animator);

        }

        private void Update()
        {
            if (Paused || Killed) return;
            _currentState?.UpdateState(Time.deltaTime);
            
            int safety = 0;
            const int kMaxChain = 8;
            while (safety++ < kMaxChain)
            {
                bool committed = UpdateTransitions();
                if (!committed) break;
            }
        }
        bool ExecuteTransitionActions(StateTransition tr)
        {
            if (tr.OnTransition == null) return true;
            foreach (var a in tr.OnTransition)
            {
                if (a == null) continue;
                bool ok = a.ProduceResult(_context.Owner, null, _context.Spawn);
                if (!ok) return false;
            }
            return true;
        }

        private bool UpdateTransitions()
        {
            if (Paused || Killed) return false;
            // Evaluate global transitions (highest priority)
            var orderedGlobals = _addedTransitions.OrderByDescending(t => t.TransitionPriority);
            foreach (var g in orderedGlobals)
            {
                if (g == null || g.NextState == null) continue;
                if (!g.CanTransition(_context)) continue;
                // If this transition requires exit time, check it (pass CurrentState.animator info if needed)
                // Here we assume StateTransition carries ExitNormalizedTime and we can call helper
                if (!_currentState.ExitTimePassed(animator,g.ExitNormalizedTime)) continue;

                // Run onTransition actions (performers). Abort on any failure
                if (!ExecuteTransitionActions(g)) 
                { 
                    _context.ClearCommand();
                    return false; 
                }

                CommitTransition(g);
                return true;
            }
            if (_currentState == null) return false;
            
            var transition = _currentState.ChooseTransition(_context, animator);
            if (transition == null || transition.NextState == null)
            {
                // No transition right now
                return false;
            }

            // 1) Execute transition 'performers' (onTransition). Pass PendingCommand as data.
            if (transition.OnTransition != null)
            {
                if (!ExecuteTransitionActions(transition))
                {
                    // Abort transition if any action failed
                    Debug.LogWarning($"Transition action failed, aborting transition from {_currentState}");
                    _context.ClearCommand(); // or keep it if you want to retry
                    return false; 
                }
            }
            CommitTransition(transition);
            return true;
        }

        void CommitTransition(StateTransition tr)
        {
            // 2) Commit the transition
            _currentState.ExitState(_context, animator);
            _currentState = tr.NextState;
            _context.CurrentState = _currentState;
            _currentState.EnterState(_context, animator);

            // 3) Command consumed
            _context.ClearCommand();
        }
        
        public bool TryCommandTransition(UnitActionType actionType, IEnumerable<IUnitCommandPerformer> commandPerformers)
        {
            if (Paused || Killed) return false;
            _context.PendingCommand = actionType;
            return ValidateCommandTransition(commandPerformers);
        }
        
        bool ValidateCommandTransition(IEnumerable<IUnitCommandPerformer> commandPerformers)
        {
            if (_currentState == null)
            {
                _context.ClearCommand();
                return false;
            }

            var transition = _currentState.ChooseTransition(_context,animator);
            
            if (transition == null || transition.NextState == null)
            {
                _context.ClearCommand();
                return false;
            }
            
            if (commandPerformers != null)
            {
                foreach (var handler in commandPerformers)
                {
                    bool executed = handler.DoUnitCommand(_context.PendingCommand, true);
                    if (!executed)
                    {
                        Debug.LogWarning($"Performer {handler} failed to execute {_context.PendingCommand}; aborting transition.");
                        _context.ClearCommand();
                        return false;
                    }
                }
            }
            CommitTransition(transition);
            return true;
        }
        public void ForceUnitState(UnitState forcedState, bool immediate = true)
        {
            if (forcedState == null) return;
            _currentState?.ExitState(_context, animator);
            _currentState = forcedState;
            _context.CurrentState = _currentState;
            _currentState.EnterState(_context, animator);
            _context.ClearCommand();
        }
        public void AddTransition(StateTransition transition)
        {
            if (transition == null || _addedTransitions.Contains(transition)) return;
            _addedTransitions.Add(transition);
        }

        public void RemoveTransition(StateTransition transition)
        {
            if (transition == null || !_addedTransitions.Contains(transition)) return;
            _addedTransitions.Remove(transition);
        }
        
        public bool Paused { get; set; }
        public bool Killed { get; set; }
    }

}