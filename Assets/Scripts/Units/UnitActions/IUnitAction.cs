using Arcatech.Actions;

namespace Arcatech.Units
{
    public interface IUnitAction
    {
        public void StartState();
        public UnitActionState UpdateAction(float delta);
        public bool LockMovement { get; }

        public IActionResult[] OnEnterState { get; }
        public IActionResult[] OnExitState { get; }
    }


    public enum UnitActionState
    {
        None,
        Started,
        ExitTime,
        Completed
    }
}

