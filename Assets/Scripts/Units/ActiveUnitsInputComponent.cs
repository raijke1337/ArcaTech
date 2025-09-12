using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
namespace Arcatech
{
    /// <summary>
    /// abstract class that handles inputs from Behavior tree or player commands
    /// </summary>
    [RequireComponent (typeof(ActiveGameUnitComponent))]
    public abstract class ActiveUnitsInputComponent : ValidatedMonoBehaviour, IPausableComponent
    {
        [SerializeField,Self] ActiveGameUnitComponent gameUnitComponent;
        protected virtual void RequestCombatAction(UnitActionType type)
        {
            if (Paused) return;
             gameUnitComponent.Command(type);
        }
        private void OnEnable()
        {
            ControllerStartBindings(true);  
        }
        protected virtual void OnDisable()
        {
            ControllerStartBindings(false);
            Debug.Log($"Disable called {this}");
        }

        protected abstract void ControllerStartBindings(bool enabling);

        public bool Paused { get; set; } = false;
    }
}