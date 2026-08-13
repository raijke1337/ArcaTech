using System;
using System.Collections;
using System.Collections.Generic;
using Arcatech.Items;
using Arcatech.Units;
using Arcatech.Units.Control;
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

        // --- State Machine bridge ---
        [SerializeField] private SerializedStateTransition dummyActivate;
        private StateTransition _activate;

        public void Attach(IStateAugmentorReceiver machine)
        {
            if (!dummyActivate) return;
            _activate ??= dummyActivate.Build();
            if (_activate != null) machine.AddTransition(_activate);
        }

        public void Detach(IStateAugmentorReceiver machine)
        {
            if (_activate != null) machine.RemoveTransition(_activate);
        }

        public void OnStateEntered(UnitState state, StateMachineContext context)
        {
        }

        public void OnStateExited(UnitState state, StateMachineContext context)
        {
        }

        // --- State core ---
        [SerializeField] private InteractionState _state;

        public InteractionState State
        {
            get => _state;
            private set
            {
                if (_state == value) return;
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

            if (State is InteractionState.Success or InteractionState.Failure or InteractionState.Cancelled)
            {
                StartCoroutine(ResetIdleNextFrame());
            }
            else
            {
                _active = null; // ← и здесь тоже
                State = InteractionState.Idle;
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
                _active = null;  // ← очищаем завершённое взаимодействие
                State = InteractionState.Idle;
            }
        }

        // --- Commands ---
        public bool CanDoUnitCommand(UnitCommand command, out string info)
        {
            info = "Interaction";
            if (command.Type == UnitActionType.Use)
                return State == InteractionState.Idle;
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
                _active = _target;
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
            if (interactable == null) return;
            if (!_inRange.Contains(interactable))
                _inRange.Add(interactable);
            RefreshTarget();
        }

        public void UnregisterInteractive(InteractableComponent interactable)
        {
            if (interactable == null) return;
            _inRange.Remove(interactable);

            if (_target == interactable)
            {
                _target = null;
                if (_active == interactable)
                {
                    // Отменяем только если объект ещё выполняет взаимодействие.
                    // Если он уже финишировал (IsAvailable == true), просто отпускаем ссылку.
                    if (interactable != null && !interactable.IsAvailable)
                    {
                        interactable.CancelInteraction();
                    }
                    _active = null;
                }
            }

            RefreshTarget();
        }

        private void RefreshTarget()
        {
            _inRange.RemoveAll(x => x == null || !x.gameObject.activeInHierarchy);
            if (_inRange.Count == 0)
            {
                _target = null;
                return;
            }

            // Можно добавить сортировку по дистанции/углу
            _target = _inRange[0];
        }

        public BaseGameEntityComponent Entity => entity;
    }
}