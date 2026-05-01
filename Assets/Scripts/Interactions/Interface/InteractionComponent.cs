using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using Arcatech.Items;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class InteractionComponent : ValidatedMonoBehaviour, IInteractor, IUnitCommandPerformer, IUnitCommandValidator,IStateAugmentor
    {
        [SerializeField, Self] private BaseGameEntityComponent entity;
        private readonly List<InteractableComponent> _inRange = new();
        private InteractableComponent _target;
        private InteractableComponent _active;
        
        #region placeholder
        [SerializeField] private SerializedStateTransition dummyActivate;
        private StateTransition _activate;
        public void Attach(IStateAugmentorReceiver machine)
        {
            _activate ??= dummyActivate.Build();
            machine.AddTransition(_activate);
        }

        public void Detach(IStateAugmentorReceiver machine)
        {
            machine.RemoveTransition(_activate);
        }

        public void OnStateEntered(UnitState state, StateMachineContext context)
        {
        }

        public void OnStateExited(UnitState state, StateMachineContext context)
        {
        }
        
        #endregion
        public bool CanDoUnitCommand(UnitActionType type, out string info)
        {
            info = "Interaction";
            if (type == UnitActionType.Use)
            {
                return State == InteractionState.Idle;
            }
            return true;
        }
        
        public void PrepareCommand(UnitActionType type)
        {
            if (type == UnitActionType.Use)
            {
                State = InteractionState.Starting;
            }
        }

        public void DoUnitCommand(UnitActionType type, bool wasSuccessful)
        {
            if (type == UnitActionType.Use && wasSuccessful)
            {
                var ctx = new InteractionContext
                {
                    Interactor = this,
                    InteractionPoint = _target.InteractionPoint ? _target.InteractionPoint.position : transform.position
                };
                
                State = InteractionState.InProgress;
                if (_active != null && _active == _target)
                {
                    // Повторное нажатие = запрос отмены (для коробки, длительных действий)
                    _active.CancelInteraction();
                    _active = null;
                }
                else if (_target != null && _target.IsAvailable)
                {

                    _active = _target;
                    _target.StartInteraction(ctx);
                }
            }
            State = InteractionState.Idle;
        }

        public void RegisterInteractive(InteractableComponent interactable)
        {
            _inRange.Add(interactable);
            RefreshTarget();
        }
        public void UnregisterInteractive(InteractableComponent interactable)
        {
            _inRange.Remove(interactable);
            if (_target == interactable)
            {
                // Если уходим из зоны во время процесса — отменяем
                if (_active == interactable) interactable.CancelInteraction();
                _target = null;
                _active = null;
            }
            RefreshTarget();
        }

        public InteractionState State { get; private set; }
        public BaseGameEntityComponent Entity => entity;

        void RefreshTarget()
        {
            // Простая сортировка по дистанции
            _inRange.RemoveAll(x => x == null);
            if (_inRange.Count == 0) return;
            _target = _inRange[0]; // для MVP; позже можно по angle/distance
        }

        public void SetInteractionState(InteractionState state)
        {
            Debug.Log($"{state} in {entity}");
            State = state;
        }
    }
}