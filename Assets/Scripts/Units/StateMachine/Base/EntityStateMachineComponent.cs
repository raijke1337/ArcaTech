using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Interactions;
using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Units.Control;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Units
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public partial class EntityStateMachineComponent : ValidatedMonoBehaviour, IPausableComponent, IKillableComponent, IStateAugmentorReceiver
    {
        [SerializeField, Self] BaseGameEntityComponent gameEntity;
        [SerializeField, Child] Animator animator;
        public BaseGameEntityComponent GetMainEntity => gameEntity;

        [Space, Header("States")] 
        [SerializeField] SerializedUnitState defaultState;

        private UnitState _defaultState;
        [SerializeField] public bool verboseDebugs = false;

        [SerializeField] StateMachineContext _context;
        UnitState _currentState;

        readonly List<StateTransition> _addedTransitions = new();
        readonly List<(StateTransition tr, bool local)> _candidates = new();

        // --- command context -------------------------------------------------
        readonly List<IUnitCommandPerformer> _pendingPerformers = new();

        UnitCommand _pendingCommand = UnitCommand.None;

        bool HasPendingCommand =>
            _pendingCommand.Type != UnitActionType.None;
        public UnitCommand PendingCommand =>
            _pendingCommand;

        public UnitActionType PendingActionType =>
            _pendingCommand.Type;
        
        [Space, Header("Command buffering")]
        [SerializeField, Tooltip("Max seconds a command may stay buffered before it is auto-cancelled. <= 0 disables the timeout.")]
        float _commandBufferTimeout = 0.5f;
        float _pendingCommandStamp = -1f;
        // ----- augmentor system ------------------------------------------------------

        List<IStateAugmentor> _activeAugmentors = new();
        private bool _killed;

        public void RegisterAugmentor(IStateAugmentor augmentor)
        {
            if (augmentor == null || _activeAugmentors.Contains(augmentor))
            {
              //  Debug.Log($"tried to register {augmentor} and failed");
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
                PendingCommand = UnitCommand.None
            };

            _defaultState = defaultState.Build();
            _currentState = _defaultState;
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
        
        
        const int kMaxChain = 8;
        void Update()
        {
            if (Paused) return;

            _currentState?.UpdateState(_context, animator, Time.deltaTime);

            if (_killed) return;
            
            int safety = 0;
            while (safety++ < kMaxChain)
            {
                if (!TransitionsInUpdate()) break;
            }
            TickCommandTimeout();

            // No valid transition was found this frame.
            // If the current state has finished, fall back to the default state.
            TryFallbackToDefault();
        }
        void TryFallbackToDefault()
        {
            if (Paused || _killed) return;
            if (_currentState == null || _defaultState == null) return;
            if (ReferenceEquals(_currentState, _defaultState)) return;

            // НЕ сбрасываемся, если есть забуференная команда — пусть она отработает.
           // if (HasPendingCommand) return;

            if (!_currentState.HasCompleted(_context)) return;
            if (PickBestTransition(out _) != null) return;

            ResetToDefaultState();
        }
        void ResetToDefaultState()
        {
            if (verboseDebugs && GetMainEntity != null && GetMainEntity.ShowingDebugs)
                Debug.Log($"[{name}] No valid transition & state finished -> reset to default '{_defaultState.StateName}'.");

            // Mirror the exit/enter flow used by CommitTransition so augmentors stay in sync.
            _currentState?.ExitState(_context, animator);

            foreach (var aug in _activeAugmentors.ToArray())
                aug.OnStateExited(_currentState, _context);

            _currentState = _defaultState;
            _context.CurrentState = _currentState;
            _currentState.EnterState(_context, animator);

            foreach (var aug in _activeAugmentors.ToArray())
                aug.OnStateEntered(_currentState, _context);

            // IMPORTANT: a fallback to default is NOT a command rejection.
            // Keep any buffered command alive so it can be re-evaluated from the
            // default state next frame, instead of completing it as failure.
            if (!HasPendingCommand)
                _context.ClearCommand();
            // else: leave the command buffered. Do NOT call CompletePendingCommand(false).
        }
        void TickCommandTimeout()
        {
            if (!HasPendingCommand)
                return;

            if (_commandBufferTimeout <= 0f)
                return;

            if (_pendingCommandStamp < 0f)
                return;

            if (Time.time - _pendingCommandStamp <
                _commandBufferTimeout)
            {
                return;
            }

            if (verboseDebugs &&
                GetMainEntity != null &&
                GetMainEntity.ShowingDebugs)
            {
                Debug.Log(
                    $"[{name}] Command '{_pendingCommand.Type}' " +
                    $"timed out after {_commandBufferTimeout:0.##}s " +
                    "-> cancelled.");
            }

            CompletePendingCommand(false);
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
        public bool TryCommandTransition(
            UnitCommand command,
            IEnumerable<IUnitCommandPerformer> commandPerformers)
        {
            LastCommandRejectReason =
                CommandRejectReason.None;

            if (Paused ||
                _killed ||
                _context.KnockDownState)
            {
                LastCommandRejectReason =
                    CommandRejectReason.IncapacitatedState;

                return false;
            }

            CacheCommandContext(
                command,
                commandPerformers);

            _context.SetCommand(command);

            bool committed =
                ValidateCommandTransition();

            if (!committed &&
                LastCommandRejectReason ==
                CommandRejectReason.None)
            {
                LastCommandRejectReason =
                    CommandRejectReason.NoValidTransitionYet;
            }

            return committed;
        }

        void CacheCommandContext(
            UnitCommand command,
            IEnumerable<IUnitCommandPerformer> performers)
        {
            // Если уже была другая забуференная команда,
            // она считается отклоненной.
            if (HasPendingCommand)
                CompletePendingCommand(false);

            _pendingCommand = command;

            _pendingPerformers.Clear();

            if (performers != null)
            {
                foreach (IUnitCommandPerformer performer in performers)
                {
                    if (performer == null)
                        continue;

                    if (!_pendingPerformers.Contains(performer))
                        _pendingPerformers.Add(performer);
                }
            }

            _pendingCommandStamp = Time.time;
        }

        void CompletePendingCommand(bool success)
        {
            if (!HasPendingCommand)
                return;

            UnitCommand completedCommand =
                _pendingCommand;

            foreach (IUnitCommandPerformer performer
                     in _pendingPerformers)
            {
                performer?.DoUnitCommand(
                    completedCommand,
                    success);
            }

            _pendingPerformers.Clear();

            _pendingCommand = UnitCommand.None;
            _pendingCommandStamp = -1f;

            _context.ClearCommand();
        }

        bool ValidateCommandTransition()
        {
            if (_currentState == null)
            {
                LastCommandRejectReason =
                    CommandRejectReason.NoCurrentState;

                CompletePendingCommand(false);
                return false;
            }

            StateTransition chosen =
                PickBestTransition(out _);

            if (chosen == null)
            {
                LastCommandRejectReason =
                    CommandRejectReason.NoValidTransitionYet;

                return false;
            }

            CommitTransition(chosen);
            return true;
        }

        void CommitTransition(StateTransition transition)
        {
            if (transition == null ||
                transition.NextState == null)
            {
                return;
            }

            bool consumesPending =
                HasPendingCommand &&
                _context.HasPendingCommand;

            UnitCommand command =
                _pendingCommand;

            _currentState?.ExitState(
                _context,
                animator);

            foreach (IStateAugmentor augmentor
                     in _activeAugmentors.ToArray())
            {
                augmentor.OnStateExited(
                    _currentState,
                    _context);
            }

            if (consumesPending)
            {
                foreach (IUnitCommandPerformer performer
                         in _pendingPerformers)
                {
                    performer?.PrepareCommand(command);
                }
            }

            ProduceTransitionResults(transition);

            _currentState =
                transition.NextState;

            _context.CurrentState =
                _currentState;

            _currentState.EnterState(
                _context,
                animator);

            foreach (IStateAugmentor augmentor
                     in _activeAugmentors.ToArray())
            {
                augmentor.OnStateEntered(
                    _currentState,
                    _context);
            }

            if (consumesPending)
            {
                CompletePendingCommand(true);
            }
            else
            {
                _context.ClearCommand();
            }
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

            bool canExit = _currentState.CanExitState(_context);

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
                // Skip transitions that lead into the state we're already in.
                if (g.NextState.StateName == _currentState.StateName) continue;
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
        public bool Paused
        {
            get => _paused;
            set
            {
                if (_paused && !value && HasPendingCommand)
                {
                    // Resuming: don't count paused time against the command.
                    _pendingCommandStamp = Time.time;
                }
                _paused = value;
            }
        }
        bool _paused;

        public void SetKilled(
            IKillerComponent component,
            bool value)
        {
            _killed = value;

            if (value)
            {
                if (HasPendingCommand) CompletePendingCommand(false);
                _pendingCommand = UnitCommand.None;
                _context.ClearCommand();
            }
            else
            {
                Start();
            }

        }
    }
}