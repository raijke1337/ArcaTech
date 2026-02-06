using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Interactions;
using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Units.Control;
using KBCore.Refs;
using NUnit.Framework.Constraints;
using UnityEngine;

namespace Arcatech.Units
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public partial class EntityStateMachineComponent : ValidatedMonoBehaviour, IPausableComponent, IKillableComponent, IStateAugmentorReceiver
    {
        public StateMachineContext GetContext => _context;

        [SerializeField, Self] BaseGameEntityComponent gameEntity;
        [SerializeField, Child] Animator animator;
        public BaseGameEntityComponent GetMainEntity => gameEntity;

        [Space, Header("States")] 
        [SerializeField] SerializedUnitState startingState;

        [SerializeField] public bool verboseDebugs = false;

        [SerializeField] StateMachineContext _context;
        UnitState _currentState;

        readonly List<StateTransition> _addedTransitions = new();
        readonly List<(StateTransition tr, bool local)> _candidates = new();

        // --- command context -------------------------------------------------
        readonly List<IUnitCommandPerformer> _pendingPerformers = new();
        UnitActionType _pendingAction = UnitActionType.None;

        bool HasPendingCommand => _pendingAction != UnitActionType.None;

        // ----- augmentor system ------------------------------------------------------

        List<IStateAugmentor> _activeAugmentors = new();
        private bool _killed;

        public void RegisterAugmentor(IStateAugmentor augmentor)
        {
            if (augmentor == null || _activeAugmentors.Contains(augmentor))
            {
                Debug.Log($"tried to register {augmentor} and failed");
                return;
            }
            
            _activeAugmentors.Add(augmentor);
            augmentor.Attach(this);
            if (GetMainEntity.ShowingDebugs) Debug.Log($"Register {augmentor}");
        }

        public void UnregisterAugmentor(IStateAugmentor augmentor)
        {
            if (augmentor == null || !_activeAugmentors.Contains(augmentor)) return;
            augmentor.Detach(this);
            _activeAugmentors.Remove(augmentor);
            
            if (GetMainEntity.ShowingDebugs) Debug.Log($"Deregister {augmentor}");
        }
        
        // --------------------------------------
        
        
        void Start()
        {

            _context = new StateMachineContext
            {
                Spawn = gameEntity.transform,
                Owner = gameEntity,
                CurrentState = null,
                Animator = animator,
                PendingCommand = UnitActionType.None
            };

            _currentState = startingState.Build();
            _context.CurrentState = _currentState;
            _context.Owner = gameEntity;
            _context.Aimers = GetComponentsInChildren<IAim>();
            _context.Movers = GetComponentsInChildren<IMove>();
            _context.Invulnerables = GetComponentsInChildren<IInvulnerability>();
            _context.Stats = GetComponentInChildren<EntityStatsComponent>();
            _context.Interactor = GetComponentInChildren<IInteractor>();
            
            var aug = GetComponentsInChildren<IStateAugmentor>();
            foreach (var a in aug)
            {
                RegisterAugmentor(a);
            }
            
            _currentState.EnterState(_context, animator);
        }

        void Update()
        {
            if (Paused || _killed) return;

            _currentState?.UpdateState(_context,animator,Time.deltaTime);

            int safety = 0;
            const int kMaxChain = 8;
            while (safety++ < kMaxChain)
            {
                if (!TransitionsInUpdate()) break;
            }
        }

        bool TransitionsInUpdate()
        {
            if (Paused || _killed) return false;

            var chosen = PickBestTransition(out _);
            if (chosen == null || chosen.NextState == null) return false;

            CommitTransition(chosen);
            return true;
        }

        // ---------------------------------------------------------------------
        // command entry point
        // ---------------------------------------------------------------------
        public bool TryCommandTransition(UnitActionType actionType,
            IEnumerable<IUnitCommandPerformer> commandPerformers)
        {
            LastCommandRejectReason = CommandRejectReason.None;

            if (Paused || _killed || _context.KnockDownState)
            {
                LastCommandRejectReason = CommandRejectReason.IncapacitatedState;
                return false;
            }
            

            CacheCommandContext(actionType, commandPerformers);
            _context.PendingCommand = actionType;

            bool committed = ValidateCommandTransition();

            if (!committed && LastCommandRejectReason == CommandRejectReason.None)
                LastCommandRejectReason = CommandRejectReason.NoValidTransitionYet;

            return committed;
        }

        void CacheCommandContext(UnitActionType actionType,
            IEnumerable<IUnitCommandPerformer> performers)
        {
            // Cancel any previous buffered command before storing a new one.
            if (HasPendingCommand)
                CompletePendingCommand(false);

            _pendingAction = actionType;
            _pendingPerformers.Clear();

            if (performers == null) return;
            foreach (var p in performers)
            {
                if (p == null) continue;
                if (!_pendingPerformers.Contains(p))
                    _pendingPerformers.Add(p);
            }
        }

        void CompletePendingCommand(bool success)
        {
            if (!HasPendingCommand) return;

            foreach (var performer in _pendingPerformers)
                performer?.DoUnitCommand(_pendingAction, success);

            _pendingPerformers.Clear();
            _pendingAction = UnitActionType.None;
            _context.ClearCommand();
        }

        bool ValidateCommandTransition()
        {
            if (_currentState == null)
            {
                LastCommandRejectReason = CommandRejectReason.NoCurrentState;
                CompletePendingCommand(false);
                return false;
            }

            var chosen = PickBestTransition(out _);
            if (chosen == null)
            {
                // keep command buffered, but make it clear why
                LastCommandRejectReason = CommandRejectReason.NoValidTransitionYet;
                return false;
            }

            CommitTransition(chosen);
            return true;
        }

        void CommitTransition(StateTransition tr)
        {
            if (tr == null || tr.NextState == null) return;

            bool consumesPending = HasPendingCommand && _context.PendingCommand != UnitActionType.None;

            // 1) Exit current state first. Any EndUse notifications emitted here
            //    should belong to the *previous* usable.
            _currentState?.ExitState(_context, animator);
            
            foreach (var aug in _activeAugmentors.ToArray())
                aug.OnStateExited(_currentState, _context);

            // 2) Now we can safely prepare the new usable without it getting cleared.
            if (consumesPending)
            {
                foreach (var performer in _pendingPerformers)
                    performer?.PrepareCommand(_pendingAction);
            }

            // 3) Run transition actions (if any).
            ProduceTransitionResults(tr);

            // 4) Enter the new state.
            _currentState = tr.NextState;
            _context.CurrentState = _currentState;
            _currentState.EnterState(_context, animator);
            
            foreach (var aug in _activeAugmentors.ToArray())
                aug.OnStateEntered(_currentState, _context);

            // 5) Notify performers about success or clear command.
            if (consumesPending)
                CompletePendingCommand(true);
            else
                _context.ClearCommand();
        }

        void ProduceTransitionResults(StateTransition tr)
        {
            if (tr?.OnTransition == null) return;

            foreach (var a in tr.OnTransition)
                a?.ProduceResult(_context.Owner, null, _context.Spawn.position, _context.Spawn.rotation);
        }

        // ---------------------------------------------------------------------
        // external helpers
        // ---------------------------------------------------------------------

        public void AddTransition(StateTransition transition)
        {
            if (transition == null || _addedTransitions.Contains(transition)) return;
            _addedTransitions.Add(transition);
        }

        public void RemoveTransition(StateTransition transition)
        {
            if (transition == null) return;
            _addedTransitions.Remove(transition);
        }
        

        // ---------------------------------------------------------------------
        // candidate / picker
        // ---------------------------------------------------------------------
        void RefreshCandidates()
        {
            _candidates.Clear();
            if (_currentState == null) return;

            bool canExit = _currentState.CanExitState(animator);

            foreach (var t in _currentState.Transitions ?? Array.Empty<StateTransition>())
            {
                if (t == null || t.NextState == null) continue;
                if (!t.CanTransition(_context)) continue;
                if (!canExit && !t.CanOverrideMinimumStateTime) continue;
                if (!_currentState.TransitionMinTimeInStateSatisfied(animator, t.ExitNormalizedTime)) continue;
                _candidates.Add((t, true));
            }

            foreach (var g in _addedTransitions)
            {
                if (g == null || g.NextState == null) continue;
                if (!g.CanTransition(_context)) continue;
                if (!canExit && !g.CanOverrideMinimumStateTime) continue;
                if (!_currentState.TransitionMinTimeInStateSatisfied(animator, g.ExitNormalizedTime)) continue;
                _candidates.Add((g, false));
            }
#if UNITY_EDITOR
            foreach (var c in _candidates)
            {
                _debugCandidates.Insert(0, new CandidateInfo
                {
                    Source = "Local",
                    NextState = c.tr.NextState.StateName,
                    Priority = c.tr.TransitionPriority,
                    ExitNormalizedTime = c.tr.ExitNormalizedTime,
                    OverrideMinTime = c.tr.CanOverrideMinimumStateTime,
                    CanTransition = true
                });
            }
            if (_debugCandidates.Count > 8)
                _debugCandidates.RemoveAt(_debugCandidates.Count - 1);
            #endif
        }

        StateTransition PickBestTransition(out bool wasLocal)
        {
            wasLocal = false;
            RefreshCandidates();
            if (_candidates.Count == 0) return null;

            var best = _candidates
                .OrderByDescending(c => c.tr.TransitionPriority)
                .ThenByDescending(c => c.local ? 0 : 1) // globals win ties
                .FirstOrDefault();

            if (best.tr == null) return null;

            wasLocal = best.local;
            return best.tr;
        }

        // ---------------------------------------------------------------------
        public bool Paused { get; set; }

        public void SetKilled(IKillerComponent comp, bool value)
        {
            _killed = value;
            _context.PendingCommand = UnitActionType.None;
        }
    }

    public interface IUnitContextProvider
    {
        public StateMachineContext GetContext { get; }
    }
}