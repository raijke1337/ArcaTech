using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;
using Arcatech.Units;
using KBCore.Refs;
using Newtonsoft.Json;
using UnityEditor.Rendering.Universal;
using UnityEngine;
namespace Arcatech
{
    /// <summary>
    /// class that handles inputs from Behavior tree or player commands
    /// </summary>
    [RequireComponent (typeof(EntityStateMachineComponent))]
    public class UnitInputsComponent : ValidatedMonoBehaviour, IPausableComponent, IKillableComponent
    {
        [SerializeField,Self] EntityStateMachineComponent gameUnitComponent;


        private List<IUnitCommandHandler> _commandHandlers;

        public bool RequestCombatAction(UnitActionType type)
        {
            if (Paused ||
                Killed)
            {
                Debug.Log($"{this} failed command {type} because paused or killed.");
                return false;
            }

            foreach (var handler in _commandHandlers)
            {
                if (!handler.DoUnitCommand(type))
                {
                    Debug.Log($"{handler} failed command {type}");
                    return false;
                }
            }

            return true;
        }

        private void OnEnable() => ControllerStartBindings(true);  
        private void OnDisable() => ControllerStartBindings(false);

        protected virtual void ControllerStartBindings(bool enabling)
        {
            // used in player inputs
        }

        private void Awake()
        {
            _commandHandlers = new();
            _commandHandlers.AddRange(GetComponents<IUnitCommandHandler>());
            if (_commandHandlers.Count == 0)
            {
                Debug.Log($"No unit command handlers found {gameUnitComponent.GetMainEntity.GetName}");
            }
        }
        public bool Killed { get; set; } = false;
        public bool Paused { get; set; } = false;

    }
}