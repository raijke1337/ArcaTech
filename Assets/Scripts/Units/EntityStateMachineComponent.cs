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
            _context.Invulnerabiles = GetComponentsInChildren<IInvulnerability>();
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

        // New helper: try to run command performers (if provided), run OnTransition actions and commit.
        bool TryCommitTransition(StateTransition tr, IEnumerable<IUnitCommandPerformer> commandPerformers)
        {
            if (tr == null || tr.NextState == null) return false;

            // Run any command performers (validation / execution).
            if (commandPerformers != null)
            {
                foreach (var handler in commandPerformers)
                {
                    bool executed = handler.DoUnitCommand(_context.PendingCommand, true);
                    if (!executed)
                    {
                        Debug.LogWarning(
                            $"Performer {handler} failed to execute {_context.PendingCommand}; aborting transition.");
                        _context.ClearCommand();
                        return false;
                    }
                }
            }

            // Run transition OnTransition actions
            if (!ExecuteTransitionActions(tr))
            {
                Debug.LogWarning($"Transition action failed, aborting transition from {_currentState}");
                _context.ClearCommand();
                return false;
            }

            CommitTransition(tr);
            return true;
        }

        private bool UpdateTransitions()
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
            if (!TryCommitTransition(chosen, null))
            {
                // aborted by action/performer
                return false;
            }
            return true;
        }

        /// final action
        void CommitTransition(StateTransition tr)
        {
            if (tr == null || tr.NextState == null) return;

            _context.ClearCommand();
            
            _currentState.ExitState(_context, animator);
            Debug.Log("Exiting state "+_context.CurrentState);
            
            _currentState = tr.NextState;
            _context.CurrentState = _currentState;
            _currentState.EnterState(_context, animator);

            Debug.Log("Entering state "+_context.CurrentState);
        }

        public bool TryCommandTransition(UnitActionType actionType,
            IEnumerable<IUnitCommandPerformer> commandPerformers)
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

            // Use the same selection logic (local + global), but when committing from a command
            // we want to preserve the PendingCommand so chained transitions in the new state can consume it.
            bool wasLocal;
            var chosen = PickBestTransition(out wasLocal);
            if (chosen == null || chosen.NextState == null)
            {
                _context.ClearCommand();
                return false;
            }

            // Try to commit, passing the performers and DO NOT clear pending command (so newly-entered state can read it)
            if (!TryCommitTransition(chosen, commandPerformers))
            {
                // TryCommitTransition cleared the command on failure
                return false;
            }

            return true;
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
            Debug.Log($"added transition to {transition.NextState}");
            _addedTransitions.Add(transition);
        }

        public void RemoveTransition(StateTransition transition)
        {
            if (transition == null || !_addedTransitions.Contains(transition)) return;
            _addedTransitions.Remove(transition);
        }


        private List<(StateTransition tr, bool isLocal)> CollectCandidates()
        {
            var candidates = new List<(StateTransition, bool)>();
            if (_currentState == null) return candidates;

            bool canExit = _currentState.CanExitState(animator);

            // 1) current state's transitions
            var localTransitions = _currentState.Transitions ?? Array.Empty<StateTransition>();
            foreach (var t in localTransitions)
            {
                if (t == null || t.NextState == null) continue;
                if (!t.CanTransition(_context)) continue;

                // If state can't exit yet and transition does not allow during minimum state, skip it.
                if (!canExit && !t.CanOverrideMinimumStateTime) continue;

                // exitNormalTime is still respected — check it relative to currentState
                if (!_currentState.TransitionMinTimeInStateSatisfied(animator, t.ExitNormalizedTime)) continue;

                candidates.Add((t, true));
            }

            // 2) global transitions
            foreach (var g in _addedTransitions)
            {
                if (g == null || g.NextState == null) continue;
                if (!g.CanTransition(_context)) continue;
                if (!canExit && !g.CanOverrideMinimumStateTime) continue;
                if (!_currentState.TransitionMinTimeInStateSatisfied(animator, g.ExitNormalizedTime)) continue;
                candidates.Add((g, false));
            }

            return candidates;
        }

// Choose the best candidate by (priority desc, isLocal preferred)
// Returns best transition or null if none
        private StateTransition PickBestTransition(out bool wasLocal)
        {
            wasLocal = false;
            var candidates = CollectCandidates();
            if (candidates.Count == 0) return null;

            // Order by priority descending, and prefer local when priorities tie
            var best = candidates
                .OrderByDescending(c => c.tr.TransitionPriority)
                .ThenByDescending(c => c.isLocal ? 1 : 0) // local wins ties
                .FirstOrDefault();

            if (best.tr == null) return null;
            wasLocal = best.isLocal;
            
            var debug = ($"Pick {best.tr.NextState.StateName} from {candidates.Count}:");
            foreach (var c in candidates)
            {
                debug += ($"\n {c.tr.NextState.StateName}");
            }

            debug += "\n";
            debug += best.tr.DebugConditions;
            Debug.Log(debug);   
            
            return best.tr;
        }


        public bool Paused { get; set; }
        public bool Killed { get; set; }
    }

}