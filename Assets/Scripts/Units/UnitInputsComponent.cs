using System.Collections.Generic;
using Arcatech.Interactions;
using Arcatech.Items;
using Arcatech.Units;
using Arcatech.Units.Control;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech
{
    [RequireComponent(
        typeof(EntityStateMachineComponent),
        typeof(InteractionComponent),
        typeof(LocomotionStateInjector))]
    public class UnitInputsComponent :
        ValidatedMonoBehaviour,
        IPausableComponent,
        IKillableComponent
    {
        [SerializeField, Self]
        private EntityStateMachineComponent stateMachine;

        private readonly List<IUnitCommandValidator> _commandValidators = new();
        private readonly List<IUnitCommandPerformer> _commandPerformers = new();

        private bool _killed;

        public Vector3 InputMovement { get; protected set; }

        public bool Paused { get; set; }

        public bool RequestCombatAction(UnitActionType type)
        {
            return RequestCombatAction(new UnitCommand(type));
        }

        public bool RequestCombatAction(UnitCommand command)
        {
            if (Paused)
                return false;

            if (_killed)
                return false;

            foreach (IUnitCommandValidator validator in _commandValidators)
            {
                if (!validator.CanDoUnitCommand(
                        command,
                        out string info))
                {
                    foreach (IUnitCommandPerformer performer in _commandPerformers)
                    {
                        performer.DoUnitCommand(command, false);
                    }

                    return false;
                }
            }

            bool accepted = stateMachine.TryCommandTransition(
                command,
                _commandPerformers);

            return accepted;
        }

        public bool CanPerformCombatAction(
            UnitCommand type,
            out string info)
        {
            info = "OK";

            foreach (IUnitCommandValidator validator in _commandValidators)
            {
                if (!validator.CanDoUnitCommand(type, out info))
                    return false;
            }

            return true;
        }

        public void SetKilled(IKillerComponent component, bool value)
        {
            _killed = value;
        }

        protected virtual void Awake()
        {
            _commandPerformers.AddRange(
                GetComponents<IUnitCommandPerformer>());

            _commandValidators.AddRange(
                GetComponents<IUnitCommandValidator>());
        }

        public void RegisterCommandValidator(
            IUnitCommandValidator validator)
        {
            if (!_commandValidators.Contains(validator))
                _commandValidators.Add(validator);
        }

        public void UnregisterCommandValidator(
            IUnitCommandValidator validator)
        {
            _commandValidators.Remove(validator);
        }

        public void RegisterCommandHandler(
            IUnitCommandPerformer performer)
        {
            if (!_commandPerformers.Contains(performer))
                _commandPerformers.Add(performer);
        }

        public void UnregisterCommandHandler(
            IUnitCommandPerformer performer)
        {
            _commandPerformers.Remove(performer);
        }
    }
}