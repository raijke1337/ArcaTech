using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;
using Arcatech.Units;
using KBCore.Refs;
using Unity.Behavior;
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
        private bool _killed = false;

        public bool RequestCombatAction(UnitActionType type)
        {
            if (Paused) return false;
            if (_killed) return false;
            
            if (stateMachine.verboseDebugs && stateMachine.GetMainEntity.ShowingDebugs) Debug.Log($"[Input] At {Time.time} Request {type} (validators: {_commandValidators.Count})");
            foreach (var v in _commandValidators)
            {
                if (!v.CanDoUnitCommand(type, out string info))
                {
                    if (stateMachine.GetMainEntity.ShowingDebugs) Debug.Log($"[Input] Command fail {type} in {v}.{info} at {Time.time}.");
                    foreach (var p in _commandPerformers)
                    {
                        p.DoUnitCommand(type, false);
                    }
                    // this should be in state machine but I think this bandaid is fine enough for now
                    return false;
                }
            }
            var ok = stateMachine.TryCommandTransition(type,_commandPerformers);
            if (stateMachine.GetMainEntity.ShowingDebugs)
            {
                Debug.Log($"[Inputs] Command: {type}, state machine response: {(ok? "OK" : stateMachine.LastCommandRejectReason)}");
            }
            return ok;
        }

        public bool CanPerformCombatAction(UnitActionType type, out string info)
        {
            info = "OK";
            foreach (var v in _commandValidators)
            {
                if (!v.CanDoUnitCommand(type, out info)) return false; 
                info = "OK";
            }
            
            return true;
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


        public void SetKilled(IKillerComponent comp, bool value)
        {
            _killed = value;
        }

        public bool Paused { get; set; } = false;

    }
}