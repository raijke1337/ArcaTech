using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
namespace Arcatech
{
    /// <summary>
    /// class that handles inputs from Behavior tree or player commands
    /// </summary>
    [RequireComponent (typeof(EntityStateMachineComponent))]
    public class UnitInputsComponent : ValidatedMonoBehaviour, IPausableComponent, IKillableComponent
    {
        private List <IUnitCommandValidator> _commandValidators;
        [SerializeField,Self] EntityStateMachineComponent stateMachine;
        private List<IUnitCommandPerformer> _commandPerformers;
        
        public bool RequestCombatAction(UnitActionType type)
        {
            if (Paused ||
                Killed)
            {
                return false;
            }

            foreach (var v in _commandValidators)
            {
                if (!v.CanDoUnitCommand(type, out string info))
                {
                    Debug.Log($"{this} failed command {type} in {v}.{info}");
                    return false;
                }
            }
            var ok = stateMachine.TryCommandTransition(type);
            
            foreach (var v in _commandPerformers)
            {
                v.DoUnitCommand(type, ok);
            }
            
            return ok;
        }

        public Vector3 InputMovement { get; protected set; }
        private void OnEnable() => ControllerStartBindings(true);  
        private void OnDisable() => ControllerStartBindings(false);

        protected virtual void ControllerStartBindings(bool enabling)
        {
            if (enabling)
            {
                _commandPerformers ??= new();
                _commandPerformers.AddRange(GetComponents<IUnitCommandPerformer>());
                if (_commandPerformers.Count == 0)
                {
                    Debug.Log($"No unit command handlers found");
                }
                _commandValidators ??= new();
                _commandValidators.AddRange(GetComponents<IUnitCommandValidator>());
                if (_commandValidators.Count == 0)
                {
                    Debug.Log("No unit command validators found");
                }
            }
            else
            {
                _commandPerformers.Clear();
                _commandValidators.Clear();
            }
        }

        public void RegisterCommandValidator(IUnitCommandValidator validator)
        {
            _commandValidators ??= new();
            if (!_commandValidators.Contains(validator)) _commandValidators.Add(validator);
        }

        public void UnregisterCommandValidator(IUnitCommandValidator validator)
        {
            if (_commandValidators.Contains(validator)) _commandValidators.Remove(validator);
        }

        public void RegisterCommandHandler(IUnitCommandPerformer performer)
        {
            _commandPerformers ??= new();
            if (!_commandPerformers.Contains(performer)) _commandPerformers.Add(performer);
        }

        public void UnregisterCommandHandler(IUnitCommandPerformer performer)
        {
            if (_commandPerformers.Contains(performer)) _commandPerformers.Remove(performer);
        }

        public bool Killed { get; set; } = false;
        public bool Paused { get; set; } = false;

    }
}