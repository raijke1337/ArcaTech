using KBCore.Refs;
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
    }
}