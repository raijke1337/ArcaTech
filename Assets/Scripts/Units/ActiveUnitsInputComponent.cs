using KBCore.Refs;
using System;
using UnityEngine;
namespace Arcatech
{
    /// <summary>
    /// abstract class that handles inputs from Behavior tree or player commands
    /// </summary>
    [RequireComponent (typeof(ActiveGameUnitComponent))]
    public abstract class ActiveUnitsInputComponent : ValidatedMonoBehaviour
    {
        [SerializeField,Self] ActiveGameUnitComponent gameUnitComponent;
        protected virtual void RequestCombatAction(UnitActionType type) => gameUnitComponent.Command(type);
        //protected virtual void RequestCombatAction(UnitActionType type) => UnitActionRequestedEvent.Invoke(type);
        // public event Action<UnitActionType> UnitActionRequestedEvent = delegate { };
        private void OnEnable()
        {
            ControllerStartBindings(true);  
        }
        private void OnDisable()
        {
            ControllerStartBindings(false);
        }

        protected abstract void ControllerStartBindings(bool enabling);
    }
}