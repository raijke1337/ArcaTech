using System;
using System.Collections;
using System.Collections.Generic;
using Arcatech.Units;
using Arcatech.Units.Control;
using AYellowpaper.SerializedCollections;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class InteractionComponent : ValidatedMonoBehaviour, IInteractor, IUnitCommandPerformer,
        IUnitCommandValidator, IStateAugmentor
    {
        [SerializeField, Self] private BaseGameEntityComponent entity;

        private readonly List<InteractableComponent> _inRange = new();
        private InteractableComponent _target;
        private InteractableComponent _active;

        public event Action<InteractionState> StateChanged;

        //--- State Machine bridge ---
        [SerializeField] private SerializedDictionary<InteractionState, SerializedStateTransition> animatedStates;
        [SerializeField, Tooltip("Исходные Motion-клипы generic-состояний в базовом Animator Controller.")]
        private SerializedDictionary<InteractionState, AnimationClip> baseClips = new();

        private readonly Dictionary<InteractionState, StateTransition> stateTransitions = new();
        private Animator animator;
        private InteractionAnimationProfileSO activeProfile;
        private InteractionAnimationProfileSO appliedProfile;
        private InteractionAnimationsSwitcherComponent enteredSwitcher;
        private InteractionState? enteredState;
        public InteractionState? AnimationState => enteredState;

        public void Attach(IStateAugmentorReceiver machine)
        {
            animator = machine.Context.Animator;
            if (animatedStates == null)
            {
                Debug.LogError($"Interaction component not set up! {name}");
                return;
            }
            foreach (var pair in animatedStates)
            {
                if (pair.Value == null || pair.Value.nextState == null) continue;
                if (!stateTransitions.TryGetValue(pair.Key, out var transition))
                {
                    transition = pair.Value.Build();
                    stateTransitions.Add(pair.Key, transition);
                }
                machine.AddTransition(transition);
            }
            RefreshAnimationProfile();
        }
        
        public void Detach(IStateAugmentorReceiver machine)
        {
            foreach (var pair in stateTransitions)
            {
                machine.RemoveTransition(pair.Value);
            }
            ApplyProfile(null);
            animator = null;
            enteredSwitcher = null;
            enteredState = null;
        }

        public void OnStateEntered(UnitState state, StateMachineContext context)
        {
            foreach (var pair in stateTransitions)
            {
                if (pair.Value.NextState != state) continue;
                var target = _active ?? _target;
                enteredSwitcher = target != null ? target.AnimationSwitcher : null;
                enteredState = pair.Key;
                // Длительное действие (например, мини-игра) может оставаться в этой
                // фазе после мгновенного выхода из её анимационного состояния.
                if (state.SkipAnimation) pair.Value.IsEnabled = false;
                break;
            }
        }

        public void OnStateExited(UnitState state, StateMachineContext context)
        {
            if (!enteredState.HasValue) return;
            var exited = enteredState.Value;
            var switcher = enteredSwitcher;
            enteredSwitcher = null;
            enteredState = null;
            if (switcher != null) switcher.StateAnimationExited?.Invoke(exited);
            RefreshAnimationProfile();
        }

        internal void BeginInteraction(InteractableComponent target)
        {
            if (_active == target) return;
            _active = target;
            activeProfile = ProfileOf(target);
            RefreshAnimationProfile();
        }

        private static InteractionAnimationProfileSO ProfileOf(InteractableComponent target)
        {
            var switcher = target != null ? target.AnimationSwitcher : null;
            return switcher != null && switcher.isActiveAndEnabled ? switcher.Profile : null;
        }

        private void RefreshAnimationProfile()
        {
            // Терминальный клип может играть дольше, чем логический статус Success/Failure/Cancelled.
            if (enteredState is not (InteractionState.Success or InteractionState.Failure or InteractionState.Cancelled))
                ApplyProfile(_active is null ? ProfileOf(_target) : activeProfile);
        }

        private void ApplyProfile(InteractionAnimationProfileSO profile)
        {
            if (stateTransitions.Count == 0 || appliedProfile == profile) return;
            appliedProfile = profile;
            foreach (var pair in stateTransitions)
            {
                bool skip = profile != null && profile.SkipsAnimation(pair.Key);
                pair.Value.NextState.SkipAnimation = skip;
                // Пустая поза рядом с предметом вообще не требует выхода из обычного Idle.
                pair.Value.IsEnabled = !skip || pair.Key != InteractionState.Idle;
            }
            if (baseClips.Count == 0) return;
            var clipOverrides = AnimatorClipOverrides.For(animator);
            if (clipOverrides == null) return;
            foreach (var pair in baseClips)
                clipOverrides.SetClip(pair.Value, profile != null ? profile.GetClip(pair.Key) : null);
            clipOverrides.Apply();
        }

        // --- State core ---
        [SerializeField] private InteractionState _state;

        public InteractionState State
        {
            get => _state;
            private set
            {
                if (_state == value) return;
                if (_state != InteractionState.Idle && stateTransitions.TryGetValue(_state, out var previous))
                    previous.IsEnabled = true;
                _state = value;
                if (entity.ShowingDebugs) Debug.Log($"[{entity.GetName}] Interaction State = {value}");
                StateChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Вызывается ТОЛЬКО из InteractableComponent для промежуточных состояний.
        /// </summary>
        public void SetInteractionState(InteractionState state)
        {
            State = state;
        }

        /// <summary>
        /// Вызывается из InteractableComponent после терминального статуса.
        /// Даём один кадр на реакцию подписчикам, затем сбрасываем в Idle.
        /// </summary>
        public void ResetToIdle()
        {
            if (State == InteractionState.Idle) return;

            // Вызов может прийти при выгрузке сцены / откате к чекпоинту, когда интерактор уже
            // уничтожен или выключен: корутину на нём запустить нельзя.
            if (this == null) return;

            bool terminal = State is InteractionState.Success or InteractionState.Failure or InteractionState.Cancelled;
            if (terminal && isActiveAndEnabled)
            {
                StartCoroutine(ResetIdleNextFrame());
            }
            else
            {
                FinishInteraction();
            }
        }

        private IEnumerator ResetIdleNextFrame()
        {
            yield return null; // конец текущего кадра
            yield return null; // один полный кадр подписчикам на реакцию

            if (State is InteractionState.Success or 
                InteractionState.Failure or 
                InteractionState.Cancelled)
            {
                FinishInteraction();
            }
        }

        private void FinishInteraction()
        {
            _active = null;
            activeProfile = null;
            RefreshTarget();
            State = InteractionState.Idle;
        }

        // --- Commands ---
        public bool CanDoUnitCommand(UnitCommand command, out string info)
        {
            info = "Interaction";
            if (command.Type == UnitActionType.Use)
                return State == InteractionState.Idle && _target != null && _target.IsAvailable;
            return true;
        }

        public void PrepareCommand(UnitCommand command)
        {
            if (command.Type == UnitActionType.Use && State == InteractionState.Idle)
                State = InteractionState.Starting;
        }

        public void DoUnitCommand(UnitCommand command, bool wasSuccessful)
        {
            if (command.Type != UnitActionType.Use || !wasSuccessful) return;

            // Машина может принять более приоритетный переход (например, получение урона)
            // в кадре команды Use. В этом случае предмет запускать ещё рано.
            if (stateTransitions.ContainsKey(InteractionState.Starting) && enteredState != InteractionState.Starting)
            {
                if (State == InteractionState.Starting && _active == null) State = InteractionState.Idle;
                return;
            }

            // Ветка 1: отмена текущего активного взаимодействия
            if (_active != null && _active == _target)
            {
                _active.CancelInteraction();
                return;
            }

            // Ветка 2: старт нового взаимодействия
            if (_target != null && _target.IsAvailable)
            {
                State = InteractionState.Starting;
                var ctx = new InteractionContext
                {
                    Interactor = this,
                    State = InteractionState.Starting,
                };
                BeginInteraction(_target);
                _target.StartInteraction(ctx);
                return;
            }

            // Ветка 3 (crucial): цели нет или она занята — откатываем готовность
            if (State == InteractionState.Starting)
            {
                State = InteractionState.Idle;
            }
        }

        // --- Trigger zone logic ---
        public void RegisterInteractive(InteractableComponent interactable)
        {
            if (interactable == null || !interactable.isActiveAndEnabled || _inRange.Contains(interactable)) return;
            _inRange.Add(interactable);
            interactable.Disabled += UnregisterInteractive;
            RefreshTarget();
        }

        public void UnregisterInteractive(InteractableComponent interactable)
        {
            if (interactable == null) return;
            if (_inRange.Remove(interactable))
                interactable.Disabled -= UnregisterInteractive;

            if (_active == interactable && !interactable.IsAvailable)
                interactable.CancelInteraction();
            RefreshTarget();
        }

        private void RefreshTarget()
        {
            _inRange.RemoveAll(x => x == null);
            _target = _inRange.Count > 0 ? _inRange[0] : null;
            RefreshAnimationProfile();
        }

        public BaseGameEntityComponent Entity => entity;

        private void OnDestroy()
        {
            foreach (var target in _inRange)
                if (target != null) target.Disabled -= UnregisterInteractive;
        }

        /// <summary>
        /// for idle interact state transition. Like "thinking" near a console.
        /// </summary>
        public bool HasValidTarget => (_target != null && _target.isActiveAndEnabled) || _active is not null;
    }
}
