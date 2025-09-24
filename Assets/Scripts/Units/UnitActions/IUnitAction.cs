using Arcatech.Actions;

namespace Arcatech.Units
{
    public interface IUnitAction
    {
        public void StartAction();
        public UnitActionState UpdateAction(float delta);
        public bool LockMovement { get; }
    }


    public enum UnitActionState
    {
        None,
        Started,
        ExitTime,
        Completed
    }



    public interface IUnitState
    {
        public ActionResult[] OnEnterState { get; }
        public ActionResult[] OnExitState { get; }
        
    }
}