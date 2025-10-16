using Arcatech.Units;
using KBCore.Refs;
using UnityEditor.Rendering.Universal;
using UnityEngine;
namespace Arcatech
{
    /// <summary>
    /// abstract class that handles inputs from Behavior tree or player commands
    /// </summary>
    [RequireComponent (typeof(ActiveGameUnitComponent))]
    public abstract class ActiveUnitsInputComponent : ValidatedMonoBehaviour, IPausableComponent, IKillableComponent
    {
        [SerializeField,Self] ActiveGameUnitComponent gameUnitComponent;
        protected virtual void RequestCombatAction(UnitActionType type)
        {
            if (Paused || _killed) return;
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

        protected bool _killed = false;
        public bool Killed => _killed;
        public bool Paused { get; set; } = false;
        public void Kill()
        {
            _killed = true;
        }
    }
}